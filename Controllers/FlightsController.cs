using Microsoft.AspNetCore.Mvc;
using Airport.Services;
using Airport.Models;
using Airport.Data;

namespace Airport.Controllers
{
    public class FlightsController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public FlightsController(ApplicationDbContext context, NotificationService notificationService)
            : base(notificationService)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var flights = _context.Flights.ToList();
            if (!flights.Any())
            {
                AddNotification("Информация", "На данный момент нет доступных рейсов.", NotificationService.NotificationType.Info);
            }
            return View(flights);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var flight = _context.Flights.Find(id);
            if (flight == null)
            {
                return RedirectWithNotification("Index", "Flights", "Ошибка", "Рейс не найден.", NotificationService.NotificationType.Error);
            }

            try
            {
                _context.Flights.Remove(flight);
                _context.SaveChanges();
                return RedirectWithNotification("Index", "Flights", "Успех", "Рейс успешно удален.", NotificationService.NotificationType.Success);
            }
            catch (Exception ex)
            {
                return RedirectWithNotification("Index", "Flights", "Ошибка", $"Не удалось удалить рейс: {ex.Message}", NotificationService.NotificationType.Error);
            }
        }

        // ... existing code ...
    }
} 