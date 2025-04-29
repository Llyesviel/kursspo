using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Airport.Data;
using Airport.Models;
using Airport.Services;

namespace Airport.Controllers
{
    public class TicketController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public TicketController(ApplicationDbContext context, NotificationService notificationService)
            : base(notificationService)
        {
            _context = context;
        }

        // GET: Ticket
        public async Task<IActionResult> Index()
        {
            var tickets = await _context.Tickets
                .Include(t => t.Flight)
                .ToListAsync();
            return View(tickets);
        }

        // GET: Ticket/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .Include(t => t.Flight)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // GET: Ticket/Create
        public IActionResult Create(int? flightId)
        {
            // Получаем только рейсы с доступными местами
            var availableFlights = _context.Flights
                .Where(f => f.AvailableSeats > 0)
                .Select(f => new
                {
                    Id = f.Id,
                    DisplayText = $"{f.FlightNumber} - {f.DepartureTime} ({f.AvailableSeats} мест)"
                });

            ViewData["FlightId"] = new SelectList(availableFlights, "Id", "DisplayText", flightId);
            
            // Если указан flightId, заполняем текущую дату и время
            var ticket = new Ticket 
            { 
                Date = DateTime.Today,
                Time = DateTime.Now.TimeOfDay
            };
            
            if (flightId.HasValue)
            {
                ticket.FlightId = flightId.Value;
            }
            
            return View(ticket);
        }

        // POST: Ticket/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CashboxNumber,FlightId,Date,Time,SeatNumber,PassengerName,DocumentNumber,ContactPhone,ContactEmail")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                // Проверяем наличие свободных мест на рейсе
                var flight = await _context.Flights.FindAsync(ticket.FlightId);
                if (flight == null || flight.AvailableSeats <= 0)
                {
                    ModelState.AddModelError("FlightId", "Нет свободных мест на данном рейсе");
                    return View(ticket);
                }

                // Генерируем уникальный номер билета
                ticket.TicketNumber = GenerateTicketNumber();
                
                // Если место не указано, генерируем случайное
                if (string.IsNullOrEmpty(ticket.SeatNumber))
                {
                    ticket.SeatNumber = GenerateRandomSeat();
                }
                
                // Устанавливаем источник покупки и статус
                ticket.PurchaseSource = "Offline";
                ticket.Status = "Paid";

                // Уменьшаем количество свободных мест
                flight.AvailableSeats--;
                _context.Update(flight);

                // Добавляем билет
                _context.Add(ticket);
                await _context.SaveChangesAsync();
                AddNotification("Успешно", "Билет успешно создан", NotificationService.NotificationType.Success);
                return RedirectToAction(nameof(Index));
            }

            var availableFlights = _context.Flights
                .Where(f => f.AvailableSeats > 0)
                .Select(f => new
                {
                    Id = f.Id,
                    DisplayText = $"{f.FlightNumber} - {f.DepartureTime} ({f.AvailableSeats} мест)"
                });

            ViewData["FlightId"] = new SelectList(availableFlights, "Id", "DisplayText", ticket.FlightId);
            return View(ticket);
        }

        // GET: Ticket/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }

            var flights = _context.Flights.Select(f => new
            {
                Id = f.Id,
                DisplayText = $"{f.FlightNumber} - {f.DepartureTime} ({f.AvailableSeats} мест)"
            });

            ViewData["FlightId"] = new SelectList(flights, "Id", "DisplayText", ticket.FlightId);
            return View(ticket);
        }

        // POST: Ticket/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CashboxNumber,FlightId,Date,Time")] Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var originalTicket = await _context.Tickets
                        .AsNoTracking()
                        .FirstOrDefaultAsync(t => t.Id == id);

                    if (originalTicket != null && originalTicket.FlightId != ticket.FlightId)
                    {
                        // Если рейс изменился, вернем место на старый рейс
                        var oldFlight = await _context.Flights.FindAsync(originalTicket.FlightId);
                        if (oldFlight != null)
                        {
                            oldFlight.AvailableSeats++;
                            _context.Update(oldFlight);
                        }

                        // И займем место на новом рейсе
                        var newFlight = await _context.Flights.FindAsync(ticket.FlightId);
                        if (newFlight != null)
                        {
                            if (newFlight.AvailableSeats <= 0)
                            {
                                ModelState.AddModelError("FlightId", "Нет свободных мест на выбранном рейсе");
                                var flights = _context.Flights.Select(f => new
                                {
                                    Id = f.Id,
                                    DisplayText = $"{f.FlightNumber} - {f.DepartureTime} ({f.AvailableSeats} мест)"
                                });
                                ViewData["FlightId"] = new SelectList(flights, "Id", "DisplayText", ticket.FlightId);
                                return View(ticket);
                            }

                            newFlight.AvailableSeats--;
                            _context.Update(newFlight);
                        }
                    }

                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                    AddNotification("Успешно", "Билет успешно обновлен", NotificationService.NotificationType.Success);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketExists(ticket.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            var availableFlights = _context.Flights.Select(f => new
            {
                Id = f.Id,
                DisplayText = $"{f.FlightNumber} - {f.DepartureTime} ({f.AvailableSeats} мест)"
            });

            ViewData["FlightId"] = new SelectList(availableFlights, "Id", "DisplayText", ticket.FlightId);
            return View(ticket);
        }

        // GET: Ticket/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .Include(t => t.Flight)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            // Если запрос был отправлен с использованием POST, удаляем объект
            if (Request.Method == "POST")
            {
                // Возвращаем место на рейс
                var flight = ticket.Flight;
                if (flight != null)
                {
                    flight.AvailableSeats++;
                    _context.Update(flight);
                }

                _context.Tickets.Remove(ticket);
                await _context.SaveChangesAsync();
                AddNotification("Успешно", "Билет успешно удален", NotificationService.NotificationType.Success);
                return RedirectToAction(nameof(Index));
            }

            return View(ticket);
        }

        private bool TicketExists(int id)
        {
            return _context.Tickets.Any(e => e.Id == id);
        }

        // Вспомогательный метод для генерации случайного места
        private string GenerateRandomSeat()
        {
            var random = new Random();
            int row = random.Next(1, 30);
            char seat = (char)('A' + random.Next(0, 6)); // A-F
            return $"{row}{seat}";
        }

        // Метод для генерации уникального номера билета
        private string GenerateTicketNumber()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            
            string part1 = new string(Enumerable.Repeat(chars, 4)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        
            string part2 = new string(Enumerable.Repeat(chars, 4)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        
            string part3 = new string(Enumerable.Repeat(chars, 4)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        
            return $"{part1}-{part2}-{part3}";
        }
    }
} 