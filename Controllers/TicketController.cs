using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Airport.Data;
using Airport.Models;

namespace Airport.Controllers
{
    public class TicketController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TicketController(ApplicationDbContext context)
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
        public async Task<IActionResult> Create([Bind("Id,CashboxNumber,FlightId,Date,Time")] Ticket ticket)
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

                // Уменьшаем количество свободных мест
                flight.AvailableSeats--;
                _context.Update(flight);

                // Добавляем билет
                _context.Add(ticket);
                await _context.SaveChangesAsync();
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
                return RedirectToAction(nameof(Index));
            }

            return View(ticket);
        }

        private bool TicketExists(int id)
        {
            return _context.Tickets.Any(e => e.Id == id);
        }
    }
} 