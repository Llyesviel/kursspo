using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Airport.Data;
using Airport.Models;
using Airport.Services;

namespace Airport.Controllers
{
    public class FlightController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public FlightController(ApplicationDbContext context, NotificationService notificationService)
            : base(notificationService)
        {
            _context = context;
        }

        // GET: Flight
        public async Task<IActionResult> Index(string searchString)
        {
            try
            {
                // Сохраняем уведомления между запросами
                if (TempData["Notifications"] != null)
                {
                    TempData.Keep("Notifications");
                }
                
                var flights = _context.Flights
                    .Include(f => f.Aircraft)
                    .AsQueryable();
                    
                // Фильтр по номеру рейса
                if (!string.IsNullOrEmpty(searchString))
                {
                    flights = flights.Where(f => f.FlightNumber.Contains(searchString));
                }
                
                ViewBag.CurrentSearchString = searchString;
                
                // Выводим информацию о запросе в консоль для диагностики
                Console.WriteLine($"Запрос рейсов выполнен. Параметр поиска: '{searchString}'. Результатов: {await flights.CountAsync()}");
                
                return View(await flights.ToListAsync());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении списка рейсов: {ex.Message}");
                AddNotification("Ошибка", $"Не удалось загрузить список рейсов: {ex.Message}", NotificationService.NotificationType.Error);
                return View(new List<Flight>());
            }
        }

        // GET: Flight/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var flight = await _context.Flights
                .Include(f => f.Aircraft)
                .Include(f => f.Landings)
                .Include(f => f.Departures)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (flight == null)
            {
                return NotFound();
            }

            return View(flight);
        }

        // GET: Flight/Create
        public async Task<IActionResult> Create()
        {
            try 
            {
                // Получаем список самолетов для выпадающего списка
                var aircrafts = await _context.Aircrafts.ToListAsync();
                
                if (aircrafts == null || !aircrafts.Any())
                {
                    Console.WriteLine("Список самолетов пуст!");
                    AddNotification("Внимание", "В системе нет самолетов. Добавьте хотя бы один самолет перед созданием рейса.", NotificationService.NotificationType.Warning);
                }
                else
                {
                    Console.WriteLine($"Загружено {aircrafts.Count} самолетов для выпадающего списка");
                }
                
                // Создаем SelectList для выпадающего меню
                ViewBag.AircraftId = new SelectList(aircrafts, "Id", "Name");
                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке формы создания рейса: {ex.Message}");
                AddNotification("Ошибка", $"Не удалось загрузить страницу создания рейса: {ex.Message}", NotificationService.NotificationType.Error);
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Flight/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FlightNumber,AircraftId,DepartureTime,AvailableSeats,Price,Status")] Flight flight)
        {
            try
            {
                // Логирование полученных данных для отладки
                Console.WriteLine("\n=== НАЧАЛО СОЗДАНИЯ РЕЙСА ===");
                Console.WriteLine($"Полученные данные: ID={flight.Id}, FlightNumber={flight.FlightNumber}, AircraftId={flight.AircraftId}");
                Console.WriteLine($"DepartureTime={flight.DepartureTime}, AvailableSeats={flight.AvailableSeats}, Price={flight.Price}, Status={flight.Status}");
                
                // Получаем список самолетов для возможного повторного отображения формы
                var aircrafts = await _context.Aircrafts.ToListAsync();
                ViewBag.AircraftId = new SelectList(aircrafts, "Id", "Name", flight.AircraftId);
                
                // Если ModelState содержит ошибку для Aircraft (навигационное свойство),
                // удаляем эту ошибку, так как мы работаем с AircraftId
                if (ModelState.ContainsKey("Aircraft"))
                {
                    ModelState.Remove("Aircraft");
                }
                
                // Проверяем валидность AircraftId вручную, чтобы быть уверенными
                if (flight.AircraftId <= 0)
                {
                    ModelState.AddModelError("AircraftId", "Выберите самолет");
                    return View(flight);
                }
                
                // Проверяем наличие самолета в базе данных
                var aircraft = await _context.Aircrafts.FindAsync(flight.AircraftId);
                if (aircraft == null)
                {
                    ModelState.AddModelError("AircraftId", $"Самолет с ID={flight.AircraftId} не найден в базе данных");
                    return View(flight);
                }
                
                // Проверяем другие обязательные поля
                if (string.IsNullOrEmpty(flight.FlightNumber))
                {
                    ModelState.AddModelError("FlightNumber", "Номер рейса обязателен");
                }
                
                if (flight.Price <= 0)
                {
                    ModelState.AddModelError("Price", "Цена должна быть положительной");
                }
                
                // Если статус не выбран, устанавливаем "Активен" по умолчанию
                if (string.IsNullOrEmpty(flight.Status))
                {
                    flight.Status = "Активен";
                    Console.WriteLine("Статус не выбран, установлено значение по умолчанию: Активен");
                }
                
                // Устанавливаем доступные места равными общему количеству мест в самолете, если не указано иное
                if (flight.AvailableSeats <= 0)
                {
                    flight.AvailableSeats = aircraft.SeatCount;
                    Console.WriteLine($"Установлено автоматическое количество мест: {flight.AvailableSeats}");
                }
                
                // Проверяем, чтобы доступные места не превышали общее количество мест в самолете
                if (flight.AvailableSeats > aircraft.SeatCount)
                {
                    flight.AvailableSeats = aircraft.SeatCount;
                    Console.WriteLine($"Количество мест ограничено вместимостью самолета: {flight.AvailableSeats}");
                }
                
                // Вывод всех ошибок валидации для диагностики
                if (!ModelState.IsValid)
                {
                    Console.WriteLine("Ошибки валидации модели:");
                    foreach (var state in ModelState)
                    {
                        if (state.Value.Errors.Count > 0)
                        {
                            Console.WriteLine($"Поле: {state.Key}");
                            foreach (var error in state.Value.Errors)
                            {
                                Console.WriteLine($"  - {error.ErrorMessage}");
                            }
                        }
                    }
                    
                    return View(flight);
                }
                
                // Устанавливаем связанный объект Aircraft в null, чтобы избежать проблем с отслеживанием EF Core
                flight.Aircraft = null;
                flight.Landings = null;
                flight.Tickets = null;
                flight.Departures = null;
                
                Console.WriteLine($"Добавление рейса: {flight.FlightNumber}, Самолет: {flight.AircraftId}, Дата вылета: {flight.DepartureTime}");
                
                _context.Add(flight);
                var result = await _context.SaveChangesAsync();
                
                Console.WriteLine($"Результат сохранения: {result} записей изменено");
                Console.WriteLine($"ID созданного рейса: {flight.Id}");
                
                // Проверяем, сохранился ли рейс
                var savedFlight = await _context.Flights.FindAsync(flight.Id);
                if (savedFlight != null)
                {
                    Console.WriteLine($"Рейс успешно сохранен в базе данных. ID: {savedFlight.Id}, Номер: {savedFlight.FlightNumber}, Статус: {savedFlight.Status}");
                    string statusMessage = !string.IsNullOrEmpty(savedFlight.Status) ? $" со статусом \"{savedFlight.Status}\"" : "";
                    
                    AddNotification("Успешно", $"Рейс {savedFlight.FlightNumber}{statusMessage} успешно добавлен", NotificationService.NotificationType.Success);
                }
                else
                {
                    Console.WriteLine($"Рейс не найден в базе данных после сохранения!");
                    AddNotification("Успешно", "Рейс успешно добавлен", NotificationService.NotificationType.Success);
                }
                
                Console.WriteLine("=== КОНЕЦ СОЗДАНИЯ РЕЙСА ===\n");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при добавлении рейса: {ex.Message}");
                Console.WriteLine($"Стек вызовов: {ex.StackTrace}");
                
                // Логируем также внутренние исключения, если они есть
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Внутреннее исключение: {ex.InnerException.Message}");
                    Console.WriteLine($"Стек внутреннего исключения: {ex.InnerException.StackTrace}");
                }
                
                AddNotification("Ошибка", $"Ошибка при добавлении рейса: {ex.Message}", NotificationService.NotificationType.Error);
                
                // Заново создаем ViewBag для возврата к форме
                ViewBag.AircraftId = new SelectList(await _context.Aircrafts.ToListAsync(), "Id", "Name", flight.AircraftId);
                return View(flight);
            }
        }

        // GET: Flight/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var flight = await _context.Flights
                .Include(f => f.Aircraft)
                .FirstOrDefaultAsync(f => f.Id == id);
            if (flight == null)
            {
                return NotFound();
            }
            ViewBag.AircraftId = new SelectList(await _context.Aircrafts.ToListAsync(), "Id", "Name", flight.AircraftId);
            return View(flight);
        }

        // POST: Flight/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FlightNumber,AircraftId,DepartureTime,AvailableSeats,Price,Status")] Flight flight)
        {
            try
            {
                // Логирование полученных данных для отладки
                Console.WriteLine("\n=== НАЧАЛО РЕДАКТИРОВАНИЯ РЕЙСА ===");
                Console.WriteLine($"Полученные данные: ID={flight.Id}, FlightNumber={flight.FlightNumber}, AircraftId={flight.AircraftId}");
                Console.WriteLine($"DepartureTime={flight.DepartureTime}, AvailableSeats={flight.AvailableSeats}, Price={flight.Price}, Status={flight.Status}");
                
                // Получаем список самолетов для возможного повторного отображения формы
                var aircrafts = await _context.Aircrafts.ToListAsync();
                ViewBag.AircraftId = new SelectList(aircrafts, "Id", "Name", flight.AircraftId);
                
                if (id != flight.Id)
                {
                    Console.WriteLine($"Несоответствие ID: параметр ID={id}, Flight.ID={flight.Id}");
                    AddNotification("Ошибка", $"Ошибка идентификации рейса: переданный ID {id} не соответствует ID рейса {flight.Id}", NotificationService.NotificationType.Error);
                    return NotFound();
                }

                // Если ModelState содержит ошибку для Aircraft (навигационное свойство),
                // удаляем эту ошибку, так как мы работаем с AircraftId
                if (ModelState.ContainsKey("Aircraft"))
                {
                    ModelState.Remove("Aircraft");
                }

                // Проверяем валидность AircraftId вручную, чтобы быть уверенными
                if (flight.AircraftId <= 0)
                {
                    ModelState.AddModelError("AircraftId", "Выберите самолет");
                    return View(flight);
                }
                
                // Проверяем наличие самолета в базе данных
                var aircraft = await _context.Aircrafts.FindAsync(flight.AircraftId);
                if (aircraft == null)
                {
                    ModelState.AddModelError("AircraftId", $"Самолет с ID={flight.AircraftId} не найден в базе данных");
                    return View(flight);
                }
                
                // Проверяем другие обязательные поля
                if (string.IsNullOrEmpty(flight.FlightNumber))
                {
                    ModelState.AddModelError("FlightNumber", "Номер рейса обязателен");
                }
                
                if (flight.Price <= 0)
                {
                    ModelState.AddModelError("Price", "Цена должна быть положительной");
                }
                
                // Если статус не выбран, устанавливаем "Активен" по умолчанию
                if (string.IsNullOrEmpty(flight.Status))
                {
                    flight.Status = "Активен";
                    Console.WriteLine("Статус не выбран, установлено значение по умолчанию: Активен");
                }
                
                // Проверяем, что количество доступных мест не превышает общее количество мест в самолете
                if (flight.AvailableSeats > aircraft.SeatCount)
                {
                    flight.AvailableSeats = aircraft.SeatCount;
                    Console.WriteLine($"Количество мест ограничено вместимостью самолета: {flight.AvailableSeats}");
                }
                
                // Проверяем, что количество доступных мест не меньше, чем количество уже забронированных мест
                int bookedSeats = await _context.Tickets
                    .Where(t => t.FlightId == flight.Id && t.Status != "Cancelled")
                    .CountAsync();
                    
                Console.WriteLine($"Забронировано мест: {bookedSeats}");
                    
                if (flight.AvailableSeats < bookedSeats)
                {
                    ModelState.AddModelError("AvailableSeats", $"Число доступных мест не может быть меньше числа уже забронированных мест ({bookedSeats})");
                    return View(flight);
                }
                
                // Вывод всех ошибок валидации для диагностики
                if (!ModelState.IsValid)
                {
                    Console.WriteLine("Ошибки валидации модели:");
                    foreach (var state in ModelState)
                    {
                        if (state.Value.Errors.Count > 0)
                        {
                            Console.WriteLine($"Поле: {state.Key}");
                            foreach (var error in state.Value.Errors)
                            {
                                Console.WriteLine($"  - {error.ErrorMessage}");
                            }
                        }
                    }
                    
                    return View(flight);
                }
                
                // Устанавливаем связанный объект Aircraft в null, чтобы избежать проблем с отслеживанием EF Core
                flight.Aircraft = null;
                flight.Landings = null;
                flight.Tickets = null;
                flight.Departures = null;
                
                try
                {
                    // Логируем детали рейса перед обновлением
                    Console.WriteLine($"Обновление рейса ID: {flight.Id}, Номер: {flight.FlightNumber}, Самолет: {(aircraft?.Name ?? "неизвестно")}, Дата вылета: {flight.DepartureTime}");
                    
                    _context.Update(flight);
                    var result = await _context.SaveChangesAsync();
                    
                    Console.WriteLine($"Результат сохранения: {result} записей изменено");
                    
                    // Проверяем, обновился ли рейс
                    var updatedFlight = await _context.Flights
                        .AsNoTracking()
                        .FirstOrDefaultAsync(f => f.Id == flight.Id);
                    
                    if (updatedFlight != null)
                    {
                        Console.WriteLine($"Рейс успешно обновлен в базе данных: ID={updatedFlight.Id}, Номер={updatedFlight.FlightNumber}, Самолет={updatedFlight.AircraftId}, Статус={updatedFlight.Status}");
                        string statusMessage = !string.IsNullOrEmpty(updatedFlight.Status) ? $" со статусом \"{updatedFlight.Status}\"" : "";
                        
                        AddNotification("Успешно", $"Рейс {updatedFlight.FlightNumber}{statusMessage} успешно обновлен", NotificationService.NotificationType.Success);
                    }
                    else
                    {
                        Console.WriteLine($"Рейс не найден в базе данных после обновления!");
                        AddNotification("Успешно", "Рейс успешно обновлен", NotificationService.NotificationType.Success);
                    }
                    
                    Console.WriteLine("=== КОНЕЦ РЕДАКТИРОВАНИЯ РЕЙСА ===\n");
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!FlightExists(flight.Id))
                    {
                        Console.WriteLine($"Ошибка конкурентного обновления: рейс с ID {flight.Id} не найден");
                        AddNotification("Ошибка", $"Рейс с ID {flight.Id} не найден", NotificationService.NotificationType.Error);
                        return NotFound();
                    }
                    else
                    {
                        Console.WriteLine($"Ошибка конкурентного обновления: {ex.Message}");
                        Console.WriteLine($"Стек вызовов: {ex.StackTrace}");
                        AddNotification("Ошибка", $"Ошибка при обновлении рейса: {ex.Message}", NotificationService.NotificationType.Error);
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обновлении рейса: {ex.Message}");
                Console.WriteLine($"Стек вызовов: {ex.StackTrace}");
                
                // Логируем также внутренние исключения, если они есть
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Внутреннее исключение: {ex.InnerException.Message}");
                    Console.WriteLine($"Стек внутреннего исключения: {ex.InnerException.StackTrace}");
                }
                
                AddNotification("Ошибка", $"Ошибка при обновлении рейса: {ex.Message}", NotificationService.NotificationType.Error);
                return View(flight);
            }
        }

        // GET: Flight/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            Console.WriteLine($"\n=== НАЧАЛО УДАЛЕНИЯ РЕЙСА ===");
            Console.WriteLine($"Получен запрос на удаление рейса с ID: {id}");
            Console.WriteLine($"Метод запроса: {Request.Method}");
            
            if (id == null)
            {
                Console.WriteLine("ID не указан. Возвращаем NotFound()");
                return NotFound();
            }

            var flight = await _context.Flights
                .Include(f => f.Aircraft)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (flight == null)
            {
                Console.WriteLine($"Рейс с ID {id} не найден. Возвращаем NotFound()");
                return NotFound();
            }

            Console.WriteLine($"Найден рейс: ID={flight.Id}, Номер={flight.FlightNumber}, Самолет={flight.Aircraft?.Name}");

            // Если запрос был отправлен с использованием POST, удаляем объект
            if (Request.Method == "POST")
            {
                Console.WriteLine($"Получен POST запрос. Начинаем удаление рейса ID={flight.Id}");
                
                // Проверяем наличие связанных билетов
                var relatedTickets = await _context.Tickets.Where(t => t.FlightId == flight.Id).ToListAsync();
                if (relatedTickets.Any())
                {
                    Console.WriteLine($"У рейса есть связанные билеты ({relatedTickets.Count} шт.). Проверка статусов...");
                    
                    bool hasActiveTickets = relatedTickets.Any(t => t.Status != "Cancelled");
                    if (hasActiveTickets)
                    {
                        Console.WriteLine("Обнаружены активные билеты! Отправляем предупреждение.");
                        AddNotification("Предупреждение", "Нельзя удалить рейс с активными билетами. Сначала отмените все билеты.", NotificationService.NotificationType.Warning);
                        return RedirectToAction(nameof(Index));
                    }
                    
                    Console.WriteLine("Все билеты отменены. Можно продолжить удаление.");
                }
                else
                {
                    Console.WriteLine("У рейса нет связанных билетов.");
                }
                
                // Проверяем состояние контекста перед удалением
                Console.WriteLine("Состояние контекста перед удалением:");
                foreach (var entry in _context.ChangeTracker.Entries())
                {
                    Console.WriteLine($"- Сущность: {entry.Entity.GetType().Name}, Состояние: {entry.State}");
                }
                
                _context.Flights.Remove(flight);
                
                // Проверяем состояние контекста после удаления объекта, но перед сохранением
                Console.WriteLine("Состояние контекста после вызова Remove():");
                foreach (var entry in _context.ChangeTracker.Entries())
                {
                    Console.WriteLine($"- Сущность: {entry.Entity.GetType().Name}, Состояние: {entry.State}");
                }
                
                var result = await _context.SaveChangesAsync();
                Console.WriteLine($"Результат сохранения: {result} записей изменено");
                
                // Проверяем, удалился ли рейс
                var deletedFlight = await _context.Flights.FindAsync(id);
                if (deletedFlight == null)
                {
                    Console.WriteLine($"Рейс с ID {id} успешно удален из базы данных");
                }
                else
                {
                    Console.WriteLine($"Рейс с ID {id} все еще существует в базе данных после удаления!");
                }
                
                AddNotification("Успешно", "Рейс успешно удален", NotificationService.NotificationType.Success);
                Console.WriteLine("=== КОНЕЦ УДАЛЕНИЯ РЕЙСА ===\n");
                return RedirectToAction(nameof(Index));
            }

            Console.WriteLine($"Метод запроса не POST. Возвращаем представление для подтверждения удаления.");
            Console.WriteLine("=== КОНЕЦ УДАЛЕНИЯ РЕЙСА (без фактического удаления) ===\n");
            return View(flight);
        }

        private bool FlightExists(int id)
        {
            return _context.Flights.Any(e => e.Id == id);
        }
    }
} 