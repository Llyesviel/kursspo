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
                
                ViewBag.FlightInfo = $"Рейс {flight.FlightNumber} ({flight.Aircraft.Name})";
                ViewBag.FlightId = flightId;
                
                var departures = await _context.Departures
                    .Where(d => d.FlightId == flightId)
                    .OrderBy(d => d.Time)
                    .ToListAsync();
                
                return View(departures);
            }
            else
            {
                var departures = await _context.Departures
                    .Include(d => d.Flight)
                    .OrderBy(d => d.Flight.FlightNumber)
                    .ThenBy(d => d.Time)
                    .ToListAsync();
                
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

                _context.Add(departure);
                await _context.SaveChangesAsync();
                AddNotification("Успешно", "Вылет успешно создан", NotificationService.NotificationType.Success);
                return RedirectToAction(nameof(Index), new { flightId = departure.FlightId });
            }
            catch (Exception ex)
            {
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
            if (id != departure.Id)
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

                _context.Update(departure);
                await _context.SaveChangesAsync();
                AddNotification("Успешно", "Вылет успешно обновлен", NotificationService.NotificationType.Success);
                return RedirectToAction(nameof(Index), new { flightId = departure.FlightId });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DepartureExists(departure.Id))
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
                ViewBag.FlightId = new SelectList(await _context.Flights.ToListAsync(), "Id", "FlightNumber", departure.FlightId);
                return View(departure);
            }
        }

        // GET: Departure/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var departure = await _context.Departures
                .Include(d => d.Flight)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (departure == null)
            {
                return NotFound();
            }

            // Если запрос был отправлен с использованием POST, удаляем объект
            if (Request.Method == "POST")
            {
                _context.Departures.Remove(departure);
                await _context.SaveChangesAsync();
                AddNotification("Успешно", "Вылет успешно удален", NotificationService.NotificationType.Success);
                
                if (Request.Query.ContainsKey("flightId"))
                {
                    return RedirectToAction(nameof(Index), new { flightId = Request.Query["flightId"] });
                }
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