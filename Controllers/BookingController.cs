using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Airport.Data;
using Airport.Models;
using Airport.Services;
using System.Threading.Tasks;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Airport.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly NotificationService _notificationService;

        public BookingController(ApplicationDbContext context, NotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        // AJAX: Получение информации о рейсе
        [HttpGet]
        public async Task<IActionResult> GetFlightDetails(int id)
        {
            var flight = await _context.Flights
                .Include(f => f.Aircraft)
                .Include(f => f.Landings)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (flight == null)
            {
                return NotFound();
            }

            return PartialView("_FlightDetails", flight);
        }

        // AJAX: Форма бронирования
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> BookingForm(int flightId)
        {
            var flight = await _context.Flights
                .Include(f => f.Aircraft)
                .Include(f => f.Landings)
                .Include(f => f.Departures)
                .FirstOrDefaultAsync(f => f.Id == flightId);

            if (flight == null || flight.AvailableSeats <= 0)
            {
                _notificationService.AddNotification("Ошибка", "Рейс не найден или нет свободных мест", NotificationService.NotificationType.Error);
                return RedirectToAction("SearchFlights", "Home");
            }

            // Получаем города отправления и прибытия
            string departureCity = flight.Departures?.FirstOrDefault()?.Location ?? "Неизвестно";
            string arrivalCity = flight.Landings?.FirstOrDefault()?.Location ?? "Неизвестно";

            var bookingViewModel = new BookingViewModel
            {
                FlightId = flightId,
                FlightNumber = flight.FlightNumber,
                DepartureTime = flight.DepartureTime,
                Price = flight.Price,
                DepartureCity = departureCity,
                ArrivalCity = arrivalCity
            };

            // Отображаем полное представление
            return View(bookingViewModel);
        }

        // GET: Бронирование билета (перенаправление на форму)
        [HttpGet]
        [Authorize]
        public IActionResult BookTicket(int flightId)
        {
            // Перенаправляем на метод BookingForm с тем же ID рейса
            return RedirectToAction("BookingForm", new { flightId = flightId });
        }

        // POST: Бронирование билета
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookTicket(BookingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Проверяем наличие свободных мест на рейсе
            var flight = await _context.Flights.FindAsync(model.FlightId);
            if (flight == null || flight.AvailableSeats <= 0)
            {
                return BadRequest("Нет свободных мест на данном рейсе");
            }

            // Генерируем случайный номер места
            var seatNumber = GenerateRandomSeat();

            // Создаем уникальный номер билета
            var ticketNumber = GenerateTicketNumber();

            // Создаем новый билет
            var ticket = new Ticket
            {
                TicketNumber = ticketNumber,
                FlightId = model.FlightId,
                Date = DateTime.Today,
                Time = DateTime.Now.TimeOfDay,
                CashboxNumber = "0", // Для онлайн-бронирований
                SeatNumber = seatNumber,
                PassengerName = model.PassengerName,
                DocumentNumber = model.DocumentNumber,
                ContactPhone = model.ContactPhone,
                ContactEmail = model.ContactEmail,
                PurchaseSource = "Online",
                Status = "Paid",
                UserId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value ?? "0")
            };

            // Уменьшаем количество свободных мест
            flight.AvailableSeats--;
            
            _context.Update(flight);
            _context.Add(ticket);
            await _context.SaveChangesAsync();

            _notificationService.AddNotification("Успешно", "Билет успешно забронирован. Перейдите в раздел 'Мои билеты' для просмотра.", NotificationService.NotificationType.Success);
            TempData["Notifications"] = System.Text.Json.JsonSerializer.Serialize(_notificationService.GetNotifications());

            // Возвращаем JSON с результатом бронирования
            return Json(new { success = true, message = "Билет успешно забронирован", ticketId = ticket.Id });
        }

        // GET: Мои билеты
        [HttpGet]
        public async Task<IActionResult> MyTickets()
        {
            var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value ?? "0");
            var tickets = await _context.Tickets
                .Include(t => t.Flight)
                .Include(t => t.Flight.Aircraft)
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.Date)
                .ThenByDescending(t => t.Time)
                .ToListAsync();

            return View(tickets);
        }
        
        // GET: История перелетов
        [HttpGet]
        public async Task<IActionResult> FlightHistory()
        {
            var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value ?? "0");
            var tickets = await _context.Tickets
                .Include(t => t.Flight)
                .Include(t => t.Flight.Aircraft)
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.Date)
                .ThenByDescending(t => t.Time)
                .ToListAsync();

            return View(tickets);
        }
        
        // Вспомогательный метод для генерации случайного места
        private string GenerateRandomSeat()
        {
            var random = new Random();
            int row = random.Next(1, 30);
            char seat = (char)('A' + random.Next(0, 6)); // A-F
            return $"{row}{seat}";
        }
        
        // Метод для генерации уникального номера билета
        private string GenerateTicketNumber()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            
            string part1 = new string(Enumerable.Repeat(chars, 4)
                .Select(s => s[random.Next(s.Length)]).ToArray());
                
            string part2 = new string(Enumerable.Repeat(chars, 4)
                .Select(s => s[random.Next(s.Length)]).ToArray());
                
            string part3 = new string(Enumerable.Repeat(chars, 4)
                .Select(s => s[random.Next(s.Length)]).ToArray());
                
            return $"{part1}-{part2}-{part3}";
        }
    }

    public class BookingViewModel
    {
        public int Id { get; set; }
        public int FlightId { get; set; }
        public int DepartureId { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public string FlightNumber { get; set; } = string.Empty;
        public string DepartureCity { get; set; } = string.Empty;
        public string ArrivalCity { get; set; } = string.Empty;
        public int SeatNumber { get; set; }
        
        [Required(ErrorMessage = "ФИО пассажира обязательно")]
        [RegularExpression(@"^[А-ЯЁ][а-яё]+ [А-ЯЁ][а-яё]+ [А-ЯЁ][а-яё]+$", 
            ErrorMessage = "ФИО должно быть в формате 'Иванов Иван Иванович'")]
        [Display(Name = "ФИО пассажира")]
        public string PassengerName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Номер документа обязателен")]
        [RegularExpression(@"^\d{4}\s\d{6}$", 
            ErrorMessage = "Номер документа должен быть в формате '1234 567899' (10 цифр)")]
        [Display(Name = "Номер документа")]
        public string DocumentNumber { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Контактный телефон обязателен")]
        [Display(Name = "Контактный телефон")]
        public string ContactPhone { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Введите корректный email адрес")]
        [Display(Name = "Email")]
        public string ContactEmail { get; set; } = string.Empty;
        
        public decimal Price { get; set; }
        public DateTime BookingDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
} 