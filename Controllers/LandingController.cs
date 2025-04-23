using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Airport.Data;
using Airport.Models;

namespace Airport.Controllers
{
    public class LandingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LandingController(ApplicationDbContext context)
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
        public IActionResult Create(int? flightId)
        {
            if (flightId.HasValue)
            {
                ViewBag.FlightId = new SelectList(_context.Flights, "Id", "FlightNumber", flightId);
                return View(new Landing { FlightId = flightId.Value, Time = DateTime.Now });
            }
            else
            {
                ViewBag.FlightId = new SelectList(_context.Flights, "Id", "FlightNumber");
                return View();
            }
        }

        // POST: Landing/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Location,Time,FlightId")] Landing landing)
        {
            if (ModelState.IsValid)
            {
                _context.Add(landing);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { flightId = landing.FlightId });
            }
            ViewBag.FlightId = new SelectList(_context.Flights, "Id", "FlightNumber", landing.FlightId);
            return View(landing);
        }

        // GET: Landing/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var landing = await _context.Landings.FindAsync(id);
            if (landing == null)
            {
                return NotFound();
            }
            ViewBag.FlightId = new SelectList(_context.Flights, "Id", "FlightNumber", landing.FlightId);
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

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(landing);
                    await _context.SaveChangesAsync();
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
                return RedirectToAction(nameof(Index), new { flightId = landing.FlightId });
            }
            ViewBag.FlightId = new SelectList(_context.Flights, "Id", "FlightNumber", landing.FlightId);
            return View(landing);
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