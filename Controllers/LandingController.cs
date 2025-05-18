using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Airport.Data;
using Airport.Models;
using Airport.Services;

namespace Airport.Controllers
{
    public class LandingController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public LandingController(ApplicationDbContext context, NotificationService notificationService)
            : base(notificationService)
        {
            _context = context;
        }

        // GET: Landing
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
                
                ViewBag.FlightInfo = $"Рейс {flight.FlightNumber} ({flight.Aircraft.Name})";
                ViewBag.FlightId = flightId;
                
                var landings = await _context.Landings
                    .Where(l => l.FlightId == flightId)
                    .OrderBy(l => l.Time)
                    .ToListAsync();
                
                return View(landings);
            }
            else
            {
                // Получаем данные без сортировки по времени (TimeSpan)
                var landings = await _context.Landings
                    .Include(l => l.Flight)
                    .OrderBy(l => l.Flight.FlightNumber)
                    .ToListAsync();
                
                // Сортируем результаты в памяти
                landings = landings
                    .OrderBy(l => l.Flight.FlightNumber)
                    .ThenBy(l => l.Time)
                    .ToList();
                
                return View(landings);
            }
        }

        // GET: Landing/Create
        public async Task<IActionResult> Create(int? flightId)
        {
            if (flightId.HasValue)
            {
                var flight = await _context.Flights.FindAsync(flightId);
                if (flight != null)
                {
                    ViewBag.FlightId = new SelectList(new[] { flight }, "Id", "FlightNumber");
                    return View(new Landing { FlightId = flight.Id });
                }
            }
            
            var flights = await _context.Flights.OrderBy(f => f.FlightNumber).ToListAsync();
            ViewBag.FlightId = new SelectList(flights, "Id", "FlightNumber");
            return View();
        }

        // POST: Landing/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Location,Time,FlightId")] Landing landing)
        {
            Console.WriteLine($"=== НАЧАЛО СОЗДАНИЯ ПОСАДКИ ===");
            Console.WriteLine($"Получены данные: ID={landing.Id}, Местоположение={landing.Location}, Время={landing.Time}, FlightId={landing.FlightId}");
            
            // Проверка валидности FlightId
            if (landing.FlightId <= 0)
            {
                ModelState.AddModelError("FlightId", "Выберите рейс из списка");
            }
            else
            {
                // Если FlightId валидный, удаляем любые уже имеющиеся ошибки для этого поля
                if (ModelState.ContainsKey("FlightId"))
                {
                    ModelState.Remove("FlightId");
                }
                
                // Проверка существования рейса в базе данных
                var flight = await _context.Flights.FindAsync(landing.FlightId);
                if (flight == null)
                {
                    ModelState.AddModelError("FlightId", $"Рейс с ID={landing.FlightId} не найден в базе данных");
                }
            }
            
            // Проверяем наличие обязательного поля Location
            if (string.IsNullOrEmpty(landing.Location))
            {
                ModelState.AddModelError("Location", "Укажите место посадки");
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
                
                // Нужно создать новый SelectList с текущими данными
                if (landing.FlightId > 0)
                {
                    var flight = await _context.Flights.FindAsync(landing.FlightId);
                    if (flight != null)
                    {
                        ViewBag.FlightId = new SelectList(new[] { flight }, "Id", "FlightNumber", landing.FlightId);
                        return View(landing);
                    }
                }
                
                var flights = await _context.Flights.OrderBy(f => f.FlightNumber).ToListAsync();
                ViewBag.FlightId = new SelectList(flights, "Id", "FlightNumber", landing.FlightId);
                return View(landing);
            }

            try
            {
                // Проверяем, что время посадки после времени вылета рейса
                var flight = await _context.Flights.FindAsync(landing.FlightId);
                if (flight == null)
                {
                    AddNotification("Ошибка", "Рейс не найден", NotificationService.NotificationType.Error);
                    var flights = await _context.Flights.OrderBy(f => f.FlightNumber).ToListAsync();
                    ViewBag.FlightId = new SelectList(flights, "Id", "FlightNumber", landing.FlightId);
                    return View(landing);
                }
                
                if (landing.Time <= flight.DepartureTime)
                {
                    Console.WriteLine($"Ошибка: время посадки ({landing.Time}) должно быть позже времени вылета рейса ({flight.DepartureTime})");
                    AddNotification("Ошибка", $"Время посадки должно быть позже времени вылета рейса ({flight.DepartureTime.ToString("dd.MM.yyyy HH:mm")})", NotificationService.NotificationType.Error);
                    var flights = await _context.Flights.OrderBy(f => f.FlightNumber).ToListAsync();
                    ViewBag.FlightId = new SelectList(flights, "Id", "FlightNumber", landing.FlightId);
                    return View(landing);
                }
                
                Console.WriteLine($"Добавление посадки: Место={landing.Location}, Время={landing.Time}, FlightId={landing.FlightId}");
                _context.Add(landing);
                var result = await _context.SaveChangesAsync();
                Console.WriteLine($"Результат сохранения: {result} записей добавлено");
                
                AddNotification("Успешно", "Посадка успешно создана", NotificationService.NotificationType.Success);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении посадки: {ex.Message}");
                AddNotification("Ошибка", $"Не удалось сохранить: {ex.Message}", NotificationService.NotificationType.Error);
                var flights = await _context.Flights.OrderBy(f => f.FlightNumber).ToListAsync();
                ViewBag.FlightId = new SelectList(flights, "Id", "FlightNumber", landing.FlightId);
                return View(landing);
            }
        }

        // GET: Landing/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var landing = await _context.Landings
                .Include(l => l.Flight)
                .FirstOrDefaultAsync(l => l.Id == id);
            if (landing == null)
            {
                return NotFound();
            }
            ViewBag.FlightId = new SelectList(await _context.Flights.ToListAsync(), "Id", "FlightNumber", landing.FlightId);
            return View(landing);
        }

        // POST: Landing/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Location,Time,FlightId")] Landing landing)
        {
            if (id != landing.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        AddNotification("Ошибка валидации", 
                            $"Поле {state.Key}: {error.ErrorMessage}", 
                            NotificationService.NotificationType.Error);
                    }
                }
                
                // Нужно создать новый SelectList с текущими данными
                if (landing.FlightId > 0)
                {
                    var flight = await _context.Flights.FindAsync(landing.FlightId);
                    if (flight != null)
                    {
                        ViewBag.FlightId = new SelectList(new[] { flight }, "Id", "FlightNumber", landing.FlightId);
                        return View(landing);
                    }
                }
                
                ViewBag.FlightId = new SelectList(await _context.Flights.ToListAsync(), "Id", "FlightNumber", landing.FlightId);
                return View(landing);
            }

            try
            {
                // Проверяем, что время посадки после времени вылета рейса
                var flight = await _context.Flights.FindAsync(landing.FlightId);
                if (flight == null)
                {
                    AddNotification("Ошибка", "Рейс не найден", NotificationService.NotificationType.Error);
                    ViewBag.FlightId = new SelectList(await _context.Flights.ToListAsync(), "Id", "FlightNumber", landing.FlightId);
                    return View(landing);
                }
                
                if (landing.Time <= flight.DepartureTime)
                {
                    AddNotification("Ошибка", "Время посадки должно быть позже времени вылета рейса", NotificationService.NotificationType.Error);
                    ViewBag.FlightId = new SelectList(await _context.Flights.ToListAsync(), "Id", "FlightNumber", landing.FlightId);
                    return View(landing);
                }

                _context.Update(landing);
                await _context.SaveChangesAsync();
                AddNotification("Успешно", "Посадка успешно обновлена", NotificationService.NotificationType.Success);
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LandingExists(landing.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                AddNotification("Ошибка", $"Не удалось сохранить: {ex.Message}", NotificationService.NotificationType.Error);
                ViewBag.FlightId = new SelectList(await _context.Flights.ToListAsync(), "Id", "FlightNumber", landing.FlightId);
                return View(landing);
            }
        }

        // GET: Landing/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var landing = await _context.Landings
                .Include(l => l.Flight)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (landing == null)
            {
                return NotFound();
            }

            // Если запрос был отправлен с использованием POST, удаляем объект
            if (Request.Method == "POST")
            {
                _context.Landings.Remove(landing);
                await _context.SaveChangesAsync();
                AddNotification("Успешно", "Посадка успешно удалена", NotificationService.NotificationType.Success);
                
                if (Request.Query.ContainsKey("flightId"))
                {
                    return RedirectToAction(nameof(Index), new { flightId = Request.Query["flightId"] });
                }
                return RedirectToAction(nameof(Index));
            }

            return View(landing);
        }

        private bool LandingExists(int id)
        {
            return _context.Landings.Any(e => e.Id == id);
        }
    }
} 