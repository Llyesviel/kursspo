using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Airport.Data;
using Airport.Models;

namespace Airport.Controllers
{
    public class FlightController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FlightController(ApplicationDbContext context)
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
                .FirstOrDefaultAsync(m => m.Id == id);
            if (flight == null)
            {
                return NotFound();
            }

            return View(flight);
        }

        // GET: Flight/Create
        public IActionResult Create()
        {
            ViewData["AircraftId"] = new SelectList(_context.Aircrafts, "Id", "Name");
            return View();
        }

        // POST: Flight/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FlightNumber,AircraftId,DepartureTime,AvailableSeats,Price")] Flight flight)
        {
            if (ModelState.IsValid)
            {
                // Устанавливаем количество свободных мест равным количеству мест в самолете
                var aircraft = await _context.Aircrafts.FindAsync(flight.AircraftId);
                if (aircraft != null)
                {
                    flight.AvailableSeats = aircraft.SeatCount;
                }

                _context.Add(flight);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AircraftId"] = new SelectList(_context.Aircrafts, "Id", "Name", flight.AircraftId);
            return View(flight);
        }

        // GET: Flight/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var flight = await _context.Flights.FindAsync(id);
            if (flight == null)
            {
                return NotFound();
            }
            ViewData["AircraftId"] = new SelectList(_context.Aircrafts, "Id", "Name", flight.AircraftId);
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

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(flight);
                    await _context.SaveChangesAsync();
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
                return RedirectToAction(nameof(Index));
            }
            ViewData["AircraftId"] = new SelectList(_context.Aircrafts, "Id", "Name", flight.AircraftId);
            return View(flight);
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