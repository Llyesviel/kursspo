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
        public async Task<IActionResult> Index()
        {
            var flights = await _context.Flights
                .Include(f => f.Aircraft)
                .ToListAsync();
            return View(flights);
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
            ViewBag.AircraftId = new SelectList(await _context.Aircrafts.ToListAsync(), "Id", "Name");
            return View();
        }

        // POST: Flight/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FlightNumber,AircraftId,DepartureTime,AvailableSeats,Price")] Flight flight)
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
                ViewBag.AircraftId = new SelectList(await _context.Aircrafts.ToListAsync(), "Id", "Name", flight.AircraftId);
                return View(flight);
            }

            try
            {
                // Устанавливаем количество свободных мест равным количеству мест в самолете
                var aircraft = await _context.Aircrafts.FindAsync(flight.AircraftId);
                if (aircraft != null)
                {
                    flight.AvailableSeats = aircraft.SeatCount;
                }

                _context.Add(flight);
                var result = await _context.SaveChangesAsync();
                AddNotification("Отладка", $"Строк изменено: {result}", NotificationService.NotificationType.Info);
                AddNotification("Успешно", "Рейс успешно создан", NotificationService.NotificationType.Success);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                AddNotification("Ошибка", $"Не удалось сохранить: {ex.Message}", NotificationService.NotificationType.Error);
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,FlightNumber,AircraftId,DepartureTime,AvailableSeats,Price")] Flight flight)
        {
            if (id != flight.Id)
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
                ViewBag.AircraftId = new SelectList(await _context.Aircrafts.ToListAsync(), "Id", "Name", flight.AircraftId);
                return View(flight);
            }

            try
            {
                var existingFlight = await _context.Flights
                    .Include(f => f.Aircraft)
                    .FirstOrDefaultAsync(f => f.Id == id);

                if (existingFlight == null)
                {
                    return NotFound();
                }

                existingFlight.FlightNumber = flight.FlightNumber;
                existingFlight.AircraftId = flight.AircraftId;
                existingFlight.DepartureTime = flight.DepartureTime;
                existingFlight.AvailableSeats = flight.AvailableSeats;
                existingFlight.Price = flight.Price;

                _context.Update(existingFlight);
                var result = await _context.SaveChangesAsync();
                AddNotification("Отладка", $"Строк изменено: {result}", NotificationService.NotificationType.Info);
                AddNotification("Успешно", "Рейс успешно обновлен", NotificationService.NotificationType.Success);
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FlightExists(flight.Id))
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
                ViewBag.AircraftId = new SelectList(await _context.Aircrafts.ToListAsync(), "Id", "Name", flight.AircraftId);
                return View(flight);
            }
        }

        // GET: Flight/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var flight = await _context.Flights
                .Include(f => f.Aircraft)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (flight == null)
            {
                return NotFound();
            }

            // Если запрос был отправлен с использованием POST, удаляем объект
            if (Request.Method == "POST")
            {
                _context.Flights.Remove(flight);
                await _context.SaveChangesAsync();
                AddNotification("Успешно", "Рейс успешно удален", NotificationService.NotificationType.Success);
                return RedirectToAction(nameof(Index));
            }

            return View(flight);
        }

        private bool FlightExists(int id)
        {
            return _context.Flights.Any(e => e.Id == id);
        }
    }
} 