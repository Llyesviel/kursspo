using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Airport.Services
{
    public class NotificationService
    {
        public enum NotificationType
        {
            Success,
            Error,
            Warning,
            Info
        }

        public class Notification
        {
            public string Title { get; set; }
            public string Message { get; set; }
            public NotificationType Type { get; set; }
            public bool IsDismissible { get; set; } = true;
        }

        private readonly List<Notification> _notifications = new List<Notification>();

        public void AddNotification(string title, string message, NotificationType type, bool isDismissible = true)
        {
            _notifications.Add(new Notification
            {
                Title = title,
                Message = message,
                Type = type,
                IsDismissible = isDismissible
            });
        }

        public List<Notification> GetNotifications()
        {
            var notifications = new List<Notification>(_notifications);
            _notifications.Clear();
            return notifications;
        }

        public bool HasNotifications()
        {
            return _notifications.Count > 0;
        }
    }
} 