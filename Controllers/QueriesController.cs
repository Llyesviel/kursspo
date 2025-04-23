using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Airport.Data;
using Airport.Models;
using Airport.ViewModels;

namespace Airport.Controllers
{
    public class QueriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QueriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Queries/AvailableSeats
        public async Task<IActionResult> AvailableSeats(int? flightId)
        {
            var flights = await _context.Flights
                .Select(f => new
                {
                    Id = f.Id,
                    DisplayText = $"{f.FlightNumber} - {f.DepartureTime} ({f.AvailableSeats} мест)"
                })
                .ToListAsync();

            ViewData["FlightId"] = new SelectList(flights, "Id", "DisplayText", flightId);

            if (flightId.HasValue)
            {
                var flight = await _context.Flights
                    .Include(f => f.Aircraft)
                    .FirstOrDefaultAsync(f => f.Id == flightId);

                if (flight != null)
                {
                    ViewData["AvailableSeats"] = flight.AvailableSeats;
                    ViewData["TotalSeats"] = flight.Aircraft.SeatCount;
                    ViewData["FlightNumber"] = flight.FlightNumber;
                }
            }

            return View();
        }

        // GET: Queries/DirectFlights
        public async Task<IActionResult> DirectFlights()
        {
            var directFlights = await _context.Flights
                .Include(f => f.Aircraft)
                .Where(f => !f.Landings.Any())
                .Select(f => new FlightSummaryViewModel
                {
                    Id = f.Id,
                    FlightNumber = f.FlightNumber,
                    DepartureTime = f.DepartureTime,
                    Aircraft = f.Aircraft.Name,
                    AvailableSeats = f.AvailableSeats,
                    TotalSeats = f.Aircraft.SeatCount
                })
                .ToListAsync();

            return View(directFlights);
        }

        // GET: Queries/FlightsByAircraft
        public async Task<IActionResult> FlightsByAircraft(int? aircraftId)
        {
            var aircraft = await _context.Aircrafts
                .Select(a => new
                {
                    Id = a.Id,
                    DisplayText = $"{a.Name} ({a.Category})"
                })
                .ToListAsync();

            ViewData["AircraftId"] = new SelectList(aircraft, "Id", "DisplayText", aircraftId);

            if (aircraftId.HasValue)
            {
                var flights = await _context.Flights
                    .Include(f => f.Aircraft)
                    .Where(f => f.Aircraft.Id == aircraftId)
                    .Select(f => new FlightSummaryViewModel
                    {
                        Id = f.Id,
                        FlightNumber = f.FlightNumber,
                        DepartureTime = f.DepartureTime,
                        Aircraft = f.Aircraft.Name,
                        AvailableSeats = f.AvailableSeats,
                        TotalSeats = f.Aircraft.SeatCount
                    })
                    .ToListAsync();

                return View(flights);
            }

            return View(Enumerable.Empty<FlightSummaryViewModel>());
        }

        // GET: Queries/AircraftLoadByDate
        public async Task<IActionResult> AircraftLoadByDate(DateTime? date)
        {
            if (!date.HasValue)
            {
                date = DateTime.Today;
            }

            var flightLoads = await _context.Flights
                .Include(f => f.Aircraft)
                .Where(f => f.DepartureTime.Date == date.Value.Date)
                .Select(f => new AircraftLoadViewModel
                {
                    FlightNumber = f.FlightNumber,
                    AircraftName = f.Aircraft.Name,
                    DepartureTime = f.DepartureTime,
                    OccupiedSeats = f.Aircraft.SeatCount - f.AvailableSeats,
                    TotalSeats = f.Aircraft.SeatCount,
                    LoadPercentage = ((double)(f.Aircraft.SeatCount - f.AvailableSeats) / f.Aircraft.SeatCount) * 100
                })
                .ToListAsync();

            ViewData["SelectedDate"] = date.Value.ToString("yyyy-MM-dd");
            return View(flightLoads);
        }

        // GET: Queries/MostExpensiveFlights
        public async Task<IActionResult> MostExpensiveFlights()
        {
            var flights = await _context.Flights
                .Include(f => f.Aircraft)
                .Include(f => f.Tickets)
                .Select(f => new ExpensiveFlightViewModel
                {
                    FlightNumber = f.FlightNumber,
                    AircraftName = f.Aircraft.Name,
                    DepartureTime = f.DepartureTime,
                    TicketCount = f.Tickets.Count,
                    TotalRevenue = f.Tickets.Count * 100 // Предполагаемая стоимость билета
                })
                .OrderByDescending(f => f.TotalRevenue)
                .Take(5)
                .ToListAsync();

            return View(flights);
        }

        // GET: Queries/UnderutilizedFlights
        public async Task<IActionResult> UnderutilizedFlights(int minEmptySeats = 10)
        {
            var flights = await _context.Flights
                .Include(f => f.Aircraft)
                .Where(f => f.AvailableSeats >= minEmptySeats)
                .Select(f => new UnderutilizedFlightViewModel
                {
                    FlightNumber = f.FlightNumber,
                    AircraftName = f.Aircraft.Name,
                    DepartureTime = f.DepartureTime,
                    AvailableSeats = f.AvailableSeats,
                    TotalSeats = f.Aircraft.SeatCount,
                    EmptyPercentage = ((double)f.AvailableSeats / f.Aircraft.SeatCount) * 100
                })
                .OrderByDescending(f => f.EmptyPercentage)
                .ToListAsync();

            ViewData["MinEmptySeats"] = minEmptySeats;
            return View(flights);
        }
    }
} 