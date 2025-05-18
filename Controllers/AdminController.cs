using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Airport.Data;
using Airport.Models;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Airport.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin
        public async Task<IActionResult> Index()
        {
            // Собираем статистику для дашборда
            var stats = new AdminDashboardViewModel
            {
                TotalAircrafts = await _context.Aircrafts.CountAsync(),
                TotalFlights = await _context.Flights.CountAsync(),
                TotalTickets = await _context.Tickets.CountAsync(),
                TotalLandings = await _context.Landings.CountAsync(),
                TotalDepartures = await _context.Departures.CountAsync(),
                
                // Получаем последние 5 зарегистрированных билетов
                RecentTickets = await _context.Tickets
                    .Include(t => t.Flight)
                    .OrderByDescending(t => t.Id)
                    .Take(5)
                    .ToListAsync(),
                
                // Получаем ближайшие 5 рейсов
                UpcomingFlights = await _context.Flights
                    .Include(f => f.Aircraft)
                    .Where(f => f.DepartureTime > System.DateTime.Now)
                    .OrderBy(f => f.DepartureTime)
                    .Take(5)
                    .ToListAsync()
            };
            
            return View(stats);
        }
        
        // GET: Admin/Users
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }
        
        // GET: Admin/UserDetails/5
        public async Task<IActionResult> UserDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }
        
        // GET: Admin/EditUser/5
        public async Task<IActionResult> EditUser(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            
            return View(user);
        }
        
        // POST: Admin/EditUser/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(int id, [Bind("Id,Username,Email,Role")] User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            // Получаем существующего пользователя из базы для сохранения хеша пароля
            var existingUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
            if (existingUser == null)
            {
                return NotFound();
            }

            // Сохраняем пароль от исходного пользователя
            user.PasswordHash = existingUser.PasswordHash;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Users));
            }
            return View(user);
        }
        
        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }

        // GET: Admin/Departures
        public async Task<IActionResult> Departures()
        {
            var departures = await _context.Departures
                .Include(d => d.Flight)
                .ThenInclude(f => f.Aircraft)
                .ToListAsync();

            return View(departures);
        }

        // GET: Admin/FlightDepartures/5
        public IActionResult FlightDepartures(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var flight = _context.Flights
                .Include(f => f.Aircraft)
                .Include(f => f.Departures)
                .FirstOrDefault(f => f.Id == id);

            if (flight == null)
            {
                return NotFound();
            }

            ViewBag.FlightId = flight.Id;
            ViewBag.FlightInfo = $"Рейс {flight.FlightNumber} ({(flight.Aircraft != null ? flight.Aircraft.Name : "Неизвестно")})";

            var departures = _context.Departures
                .Where(d => d.FlightId == id)
                .Include(d => d.Flight)
                .ToList();

            return View("Departures", departures);
        }

        // GET: Admin/CreateDeparture
        public async Task<IActionResult> CreateDeparture()
        {
            ViewBag.FlightId = new SelectList(await _context.Flights.ToListAsync(), "Id", "FlightNumber");
            return View();
        }

        // POST: Admin/CreateDeparture
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDeparture([Bind("Id,Location,Time,FlightId")] Departure departure)
        {
            if (ModelState.IsValid)
            {
                _context.Add(departure);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Departure");
            }
            ViewBag.FlightId = new SelectList(await _context.Flights.ToListAsync(), "Id", "FlightNumber", departure.FlightId);
            return View(departure);
        }

        // GET: Admin/EditDeparture/5
        public async Task<IActionResult> EditDeparture(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var departure = await _context.Departures.FindAsync(id);
            if (departure == null)
            {
                return NotFound();
            }
            ViewBag.FlightId = new SelectList(await _context.Flights.ToListAsync(), "Id", "FlightNumber", departure.FlightId);
            return View(departure);
        }

        // POST: Admin/EditDeparture/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDeparture(int id, [Bind("Id,Location,Time,FlightId")] Departure departure)
        {
            if (id != departure.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(departure);
                    await _context.SaveChangesAsync();
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
                return RedirectToAction("Index", "Departure");
            }
            ViewBag.FlightId = new SelectList(await _context.Flights.ToListAsync(), "Id", "FlightNumber", departure.FlightId);
            return View(departure);
        }

        // POST: Admin/DeleteDeparture/5
        [HttpPost]
        public async Task<IActionResult> DeleteDeparture(int id)
        {
            var departure = await _context.Departures.FindAsync(id);
            if (departure != null)
            {
                _context.Departures.Remove(departure);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index", "Departure");
        }

        private bool DepartureExists(int id)
        {
            return _context.Departures.Any(e => e.Id == id);
        }
        
        // GET: Admin/DeleteUser/5
        public async Task<IActionResult> DeleteUser(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Tickets)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (user == null)
            {
                return NotFound();
            }
            
            // Проверяем, не пытается ли администратор удалить самого себя
            if (User.Identity.Name == user.Username)
            {
                TempData["ErrorMessage"] = "Вы не можете удалить свою собственную учетную запись.";
                return RedirectToAction(nameof(Users));
            }
            
            int ticketsCount = 0;
            
            // Проверяем и удаляем связанные записи
            if (user.Tickets != null && user.Tickets.Any())
            {
                ticketsCount = user.Tickets.Count;
                _context.Tickets.RemoveRange(user.Tickets);
            }
            
            try
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                
                var successMessage = $"Пользователь {user.Username} успешно удален из системы.";
                if (ticketsCount > 0)
                {
                    successMessage += $" Также удалено связанных билетов: {ticketsCount}.";
                }
                
                TempData["SuccessMessage"] = successMessage;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Произошла ошибка при удалении: {ex.Message}";
            }
            
            return RedirectToAction(nameof(Users));
        }
        
        // POST: Admin/DeleteUser/5 - obsolete
        [HttpPost, ActionName("DeleteUser")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUserConfirmed(int id)
        {
            // Перенаправляем на GET метод выше
            return RedirectToAction(nameof(DeleteUser), new { id });
        }
    }
} 