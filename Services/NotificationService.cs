using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Airport.Services
{
    public class NotificationService
    {
        private readonly List<Notification> _notifications = new List<Notification>();
        
        public enum NotificationType
        {
            Success,
            Info,
            Warning,
            Error
        }
        
        public class Notification
        {
            public string Title { get; set; }
            public string Message { get; set; }
            public NotificationType Type { get; set; }
            public bool IsDismissible { get; set; }
            public DateTime CreatedAt { get; set; }
            
            public string GetIcon()
            {
                return Type switch
                {
                    NotificationType.Success => "fas fa-check-circle",
                    NotificationType.Info => "fas fa-info-circle",
                    NotificationType.Warning => "fas fa-exclamation-triangle",
                    NotificationType.Error => "fas fa-times-circle",
                    _ => "fas fa-bell"
                };
            }
            
            public string GetCssClass()
            {
                return Type switch
                {
                    NotificationType.Success => "success",
                    NotificationType.Info => "info",
                    NotificationType.Warning => "warning",
                    NotificationType.Error => "danger",
                    _ => "secondary"
                };
            }
        }
        
        public void AddNotification(string title, string message, NotificationType type, bool isDismissible = true)
        {
            _notifications.Add(new Notification 
            { 
                Title = title, 
                Message = message, 
                Type = type, 
                IsDismissible = isDismissible,
                CreatedAt = DateTime.Now
            });
        }
        
        public List<Notification> GetNotifications()
        {
            var notificationsList = new List<Notification>(_notifications);
            _notifications.Clear();
            return notificationsList;
        }

        public bool HasNotifications()
        {
            return _notifications.Count > 0;
        }
    }
} 