using Microsoft.AspNetCore.Mvc;
using Airport.Services;

namespace Airport.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly NotificationService _notificationService;

        protected BaseController(NotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        protected void AddNotification(string title, string message, NotificationService.NotificationType type, bool isDismissible = true)
        {
            _notificationService.AddNotification(title, message, type, isDismissible);
            TempData["Notifications"] = System.Text.Json.JsonSerializer.Serialize(_notificationService.GetNotifications());
        }

        protected IActionResult RedirectWithNotification(string action, string controller, string title, string message, NotificationService.NotificationType type)
        {
            AddNotification(title, message, type);
            return RedirectToAction(action, controller);
        }
    }
} 