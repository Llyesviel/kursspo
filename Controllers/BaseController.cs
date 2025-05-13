using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Airport.Services;
using System.Text.Json;
using System.Collections.Generic;

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
        
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);
            
            // Больше не сохраняем уведомления между запросами
            // Закомментировано, чтобы уведомления показывались только один раз
            // if (TempData.ContainsKey("Notifications"))
            // {
            //     TempData.Keep("Notifications");
            // }
        }

        protected void AddNotification(string title, string message, NotificationService.NotificationType type, bool isDismissible = true)
        {
            _notificationService.AddNotification(title, message, type, isDismissible);
            
            // Если уже есть уведомления в TempData, загружаем их и добавляем новое
            List<NotificationService.Notification> existingNotifications = new List<NotificationService.Notification>();
            
            if (TempData.ContainsKey("Notifications") && TempData["Notifications"] != null)
            {
                var json = TempData["Notifications"]?.ToString();
                if (!string.IsNullOrEmpty(json))
                {
                    existingNotifications = JsonSerializer.Deserialize<List<NotificationService.Notification>>(json) ?? new List<NotificationService.Notification>();
                }
            }
            
            // Добавляем новые уведомления
            existingNotifications.AddRange(_notificationService.GetNotifications());
            
            // Сохраняем обратно в TempData
            TempData["Notifications"] = JsonSerializer.Serialize(existingNotifications);
        }

        protected IActionResult RedirectWithNotification(string action, string controller, string title, string message, NotificationService.NotificationType type)
        {
            AddNotification(title, message, type);
            return RedirectToAction(action, controller);
        }
    }
} 