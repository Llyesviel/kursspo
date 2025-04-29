using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Airport.Services;

namespace Airport.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BaseController : Controller
    {
        protected readonly NotificationService _notificationService;

        protected BaseController(NotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            
            // Устанавливаем макет для админской части сайта
            ViewData["IsAdminLayout"] = true;
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