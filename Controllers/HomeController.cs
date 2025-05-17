using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Airport.Models;
using Airport.Data;
using Microsoft.EntityFrameworkCore;

namespace Airport.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return RedirectToAction("MainPage");
        }

        [AllowAnonymous]
        public IActionResult MainPage()
        {
            return View("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> FlightStatus(string flightNumber = "")
        {
            if (string.IsNullOrEmpty(flightNumber))
            {
                return View(new List<Flight>());
            }

            var flights = await _context.Flights
                .Include(f => f.Aircraft)
                .Include(f => f.Landings)
                .Where(f => f.FlightNumber.Contains(flightNumber))
                .OrderBy(f => f.DepartureTime)
                .ToListAsync();

            ViewBag.FlightNumber = flightNumber;
            return View(flights);
        }

        // Поиск ближайшего рейса до заданного пункта с наличием свободных мест
        [AllowAnonymous]
        public async Task<IActionResult> SearchFlights(string departure, string destination, DateTime? departureDate)
        {
            // Если дата не указана, используем текущую дату
            if (!departureDate.HasValue)
            {
                departureDate = DateTime.Today;
            }

            // Загружаем все рейсы с их отправлением и прибытием
            var flights = await _context.Flights
                .Include(f => f.Aircraft)
                .Include(f => f.Landings)
                .Include(f => f.Departures)
                .Where(f => f.AvailableSeats > 0 && 
                            f.DepartureTime >= departureDate.Value)
                .ToListAsync();

            // Фильтруем рейсы по городу отправления и прибытия
            var filteredFlights = flights
                .Where(f => 
                    // Если город отправления указан, проверяем его в Departures
                    (string.IsNullOrEmpty(departure) || 
                     f.Departures.Any(d => d.Location.Contains(departure, StringComparison.OrdinalIgnoreCase))) &&
                    
                    // Если город прибытия указан, проверяем его в Landings
                    (string.IsNullOrEmpty(destination) || 
                     f.Landings.Any(l => l.Location.Contains(destination, StringComparison.OrdinalIgnoreCase)))
                )
                .OrderBy(f => f.DepartureTime)
                .ToList();

            ViewBag.Departure = departure;
            ViewBag.Destination = destination;
            ViewBag.DepartureDate = departureDate.Value.ToString("yyyy-MM-dd");
            
            return View(filteredFlights);
        }

        // Перенаправление на новую страницу "Мои билеты"
        public IActionResult MyTickets()
        {
            return RedirectToAction("MyTickets", "Booking");
        }

        // Отчет о проданных билетах
        public async Task<IActionResult> TicketSalesReport()
        {
            var tickets = await _context.Tickets
                .Include(t => t.Flight)
                .Include(t => t.Flight.Aircraft)
                .Include(t => t.Flight.Landings)
                .OrderBy(t => t.Date)
                .ThenBy(t => t.Time)
                .ToListAsync();

            ViewBag.TotalSales = tickets.Sum(t => t.Flight.Price);
            ViewBag.TicketCount = tickets.Count;
            
            // Добавляем статистику по типам продаж
            ViewBag.OnlineTickets = tickets.Count(t => t.PurchaseSource == "Online");
            ViewBag.OfflineTickets = tickets.Count(t => t.PurchaseSource == "Offline");
            ViewBag.OnlineSales = tickets.Where(t => t.PurchaseSource == "Online").Sum(t => t.Flight.Price);
            ViewBag.OfflineSales = tickets.Where(t => t.PurchaseSource == "Offline").Sum(t => t.Flight.Price);

            return View(tickets);
        }

        // Отчет о загруженности рейсов
        public async Task<IActionResult> FlightLoadReport()
        {
            var flights = await _context.Flights
                .Include(f => f.Aircraft)
                .Where(f => f.DepartureTime >= DateTime.Today)
                .OrderBy(f => f.DepartureTime)
                .ToListAsync();

            foreach (var flight in flights)
            {
                // Вычисляем процент загруженности рейса
                flight.LoadPercentage = 100 - (flight.AvailableSeats * 100 / flight.Aircraft.SeatCount);
            }

            return View(flights);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
} 