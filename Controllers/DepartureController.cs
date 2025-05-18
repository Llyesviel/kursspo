using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Airport.Data;
using Airport.Models;
using Airport.Services;

namespace Airport.Controllers
{
    public class DepartureController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public DepartureController(ApplicationDbContext context, NotificationService notificationService)
            : base(notificationService)
        {
            _context = context;
        }

        // GET: Departure
        public async Task<IActionResult> Index(int? flightId)
        {
            if (flightId.HasValue)
            {
                var flight = await _context.Flights
                    .Include(f => f.Aircraft)
                    .FirstOrDefaultAsync(f => f.Id == flightId);

                if (flight == null)
                {
                    return NotFound();
                }
                
                string flightNumber = flight.FlightNumber ?? "Без номера";
                string aircraftName = flight.Aircraft?.Name ?? "Неизвестно";
                ViewBag.FlightInfo = $"Рейс {flightNumber} ({aircraftName})";
                ViewBag.FlightId = flightId;
                
                var departures = await _context.Departures
                    .Where(d => d.FlightId == flightId)
                    .OrderBy(d => d.Time)
                    .ToListAsync();
                
                return View(departures);
            }
            else
            {
                // Получаем данные без сортировки по времени (TimeSpan)
                var departures = await _context.Departures
                    .Include(d => d.Flight)
                    .ThenInclude(f => f != null ? f.Aircraft : null)
                    .OrderBy(d => d.Flight != null ? d.Flight.FlightNumber : string.Empty)
                    .ToListAsync();
                
                // Сортируем результаты в памяти
                departures = departures
                    .OrderBy(d => d.Flight != null ? d.Flight.FlightNumber : string.Empty)
                    .ThenBy(d => d.Time)
                    .ToList();
                
                return View(departures);
            }
        }

        // GET: Departure/Create
        public async Task<IActionResult> Create(int? flightId)
        {
            if (flightId.HasValue)
            {
                var flight = await _context.Flights.FindAsync(flightId);
                if (flight != null)
                {
                    ViewBag.FlightId = new SelectList(new[] { flight }, "Id", "FlightNumber");
                    return View(new Departure { FlightId = flight.Id });
                }
            }
            
            ViewBag.FlightId = new SelectList(await _context.Flights.ToListAsync(), "Id", "FlightNumber");
            return View();
        }

        // POST: Departure/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Location,Time,FlightId")] Departure departure)
        {
            Console.WriteLine($"=== НАЧАЛО СОЗДАНИЯ ВЫЛЕТА ===");
            Console.WriteLine($"Получены данные: ID={departure.Id}, Местоположение={departure.Location}, Время={departure.Time}, FlightId={departure.FlightId}");
            
            if (!ModelState.IsValid)
            {
                Console.WriteLine("Ошибки валидации модели:");
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine($"- Поле {state.Key}: {error.ErrorMessage}");
                        AddNotification("Ошибка валидации", 
                            $"Поле {state.Key}: {error.ErrorMessage}", 
                            NotificationService.NotificationType.Error);
                    }
                }
                ViewBag.FlightId = new SelectList(await _context.Flights.ToListAsync(), "Id", "FlightNumber", departure.FlightId);
                return View(departure);
            }

            try
            {
                // Проверяем, что время вылета после времени вылета рейса
                var flight = await _context.Flights.FindAsync(departure.FlightId);
                if (flight != null && departure.Time <= flight.DepartureTime)
                {
                    AddNotification("Ошибка", "Время вылета должно быть позже времени вылета рейса", NotificationService.NotificationType.Error);
                    ViewBag.FlightId = new SelectList(await _context.Flights.ToListAsync(), "Id", "FlightNumber", departure.FlightId);
                    return View(departure);
                }

                Console.WriteLine($"Создание нового вылета: Местоположение={departure.Location}, Время={departure.Time}, FlightId={departure.FlightId}");
                _context.Add(departure);
                await _context.SaveChangesAsync();
                Console.WriteLine("Вылет успешно сохранен в базе данных");
                AddNotification("Успешно", "Вылет успешно создан", NotificationService.NotificationType.Success);
                
                // Возвращаем на страницу всех вылетов без фильтрации
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при создании вылета: {ex.Message}");
                AddNotification("Ошибка", $"Не удалось сохранить: {ex.Message}", NotificationService.NotificationType.Error);
                ViewBag.FlightId = new SelectList(await _context.Flights.ToListAsync(), "Id", "FlightNumber", departure.FlightId);
                return View(departure);
            }
        }

        // GET: Departure/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var departure = await _context.Departures
                .Include(d => d.Flight)
                .FirstOrDefaultAsync(d => d.Id == id);
            if (departure == null)
            {
                return NotFound();
            }
            ViewBag.FlightId = new SelectList(await _context.Flights.ToListAsync(), "Id", "FlightNumber", departure.FlightId);
            return View(departure);
        }

        // POST: Departure/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Location,Time,FlightId")] Departure departure)
        {
            Console.WriteLine($"=== НАЧАЛО РЕДАКТИРОВАНИЯ ВЫЛЕТА ===");
            Console.WriteLine($"Получены данные: ID={departure.Id}, Местоположение={departure.Location}, Время={departure.Time}, FlightId={departure.FlightId}");
            
            if (id != departure.Id)
            {
                Console.WriteLine($"Ошибка: ID в URL ({id}) не соответствует ID в модели ({departure.Id})");
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                Console.WriteLine("Ошибки валидации модели:");
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine($"- Поле {state.Key}: {error.ErrorMessage}");
                        AddNotification("Ошибка валидации", 
                            $"Поле {state.Key}: {error.ErrorMessage}", 
                            NotificationService.NotificationType.Error);
                    }
                }
                ViewBag.FlightId = new SelectList(await _context.Flights.ToListAsync(), "Id", "FlightNumber", departure.FlightId);
                return View(departure);
            }

            try
            {
                // Проверяем, что время вылета после времени вылета рейса
                var flight = await _context.Flights.FindAsync(departure.FlightId);
                if (flight != null && departure.Time <= flight.DepartureTime)
                {
                    Console.WriteLine("Ошибка: время вылета должно быть позже времени вылета рейса");
                    AddNotification("Ошибка", "Время вылета должно быть позже времени вылета рейса", NotificationService.NotificationType.Error);
                    ViewBag.FlightId = new SelectList(await _context.Flights.ToListAsync(), "Id", "FlightNumber", departure.FlightId);
                    return View(departure);
                }

                Console.WriteLine($"Обновление вылета ID={departure.Id}: Местоположение={departure.Location}, Время={departure.Time}, FlightId={departure.FlightId}");
                _context.Update(departure);
                await _context.SaveChangesAsync();
                Console.WriteLine("Вылет успешно обновлен в базе данных");
                AddNotification("Успешно", "Вылет успешно обновлен", NotificationService.NotificationType.Success);
                
                // Возвращаем на страницу всех вылетов без фильтрации
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!DepartureExists(departure.Id))
                {
                    Console.WriteLine($"Ошибка конкурентного обновления: вылет ID={departure.Id} не найден");
                    return NotFound();
                }
                else
                {
                    Console.WriteLine($"Ошибка конкурентного обновления: {ex.Message}");
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обновлении вылета: {ex.Message}");
                AddNotification("Ошибка", $"Не удалось сохранить: {ex.Message}", NotificationService.NotificationType.Error);
                ViewBag.FlightId = new SelectList(await _context.Flights.ToListAsync(), "Id", "FlightNumber", departure.FlightId);
                return View(departure);
            }
        }

        // GET: Departure/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            Console.WriteLine($"=== ЗАПРОС НА УДАЛЕНИЕ ВЫЛЕТА ===");
            Console.WriteLine($"Запрошенный ID: {id}");
            
            if (id == null)
            {
                Console.WriteLine("Ошибка: ID не указан");
                return NotFound();
            }

            var departure = await _context.Departures
                .Include(d => d.Flight)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (departure == null)
            {
                Console.WriteLine($"Ошибка: вылет с ID={id} не найден");
                return NotFound();
            }

            Console.WriteLine($"Найден вылет ID={departure.Id}, Местоположение={departure.Location}, Время={departure.Time}, FlightId={departure.FlightId}");

            // Если запрос был отправлен с использованием POST, удаляем объект
            if (Request.Method == "POST")
            {
                Console.WriteLine($"Удаление вылета ID={departure.Id}");
                _context.Departures.Remove(departure);
                await _context.SaveChangesAsync();
                Console.WriteLine("Вылет успешно удален из базы данных");
                AddNotification("Успешно", "Вылет успешно удален", NotificationService.NotificationType.Success);
                
                // Возвращаем на страницу всех вылетов без фильтрации
                return RedirectToAction(nameof(Index));
            }

            return View(departure);
        }

        private bool DepartureExists(int id)
        {
            return _context.Departures.Any(e => e.Id == id);
        }
    }
} 