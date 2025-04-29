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
                var landings = await _context.Landings
                    .Include(l => l.Flight)
                    .OrderBy(l => l.Flight.FlightNumber)
                    .ThenBy(l => l.Time)
                    .ToListAsync();
                
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
            
            ViewBag.FlightId = new SelectList(await _context.Flights.ToListAsync(), "Id", "FlightNumber");
            return View();
        }

        // POST: Landing/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Location,Time,FlightId")] Landing landing)
        {
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

                _context.Add(landing);
                await _context.SaveChangesAsync();
                AddNotification("Успешно", "Посадка успешно создана", NotificationService.NotificationType.Success);
                return RedirectToAction(nameof(Index), new { flightId = landing.FlightId });
            }
            catch (Exception ex)
            {
                AddNotification("Ошибка", $"Не удалось сохранить: {ex.Message}", NotificationService.NotificationType.Error);
                ViewBag.FlightId = new SelectList(await _context.Flights.ToListAsync(), "Id", "FlightNumber", landing.FlightId);
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
                return RedirectToAction(nameof(Index), new { flightId = landing.FlightId });
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