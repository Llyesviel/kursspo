function showNotification(title, message, type, isDismissible = true) {
    const notificationContainer = document.getElementById('notification-container') || createNotificationContainer();
    const notification = document.createElement('div');
    notification.className = `alert alert-${getAlertType(type)} ${isDismissible ? 'alert-dismissible' : ''} fade show`;
    notification.role = 'alert';

    let icon = '';
    switch (type) {
        case 'Success':
            icon = '<i class="fas fa-check-circle"></i>';
            break;
        case 'Error':
            icon = '<i class="fas fa-exclamation-circle"></i>';
            break;
        case 'Warning':
            icon = '<i class="fas fa-exclamation-triangle"></i>';
            break;
        case 'Info':
            icon = '<i class="fas fa-info-circle"></i>';
            break;
    }

    notification.innerHTML = `
        ${isDismissible ? '<button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>' : ''}
        <strong>${icon} ${title}</strong>
        <p class="mb-0">${message}</p>
    `;

    notificationContainer.appendChild(notification);

    if (isDismissible) {
        setTimeout(() => {
            notification.remove();
        }, 5000);
    }
}

function getAlertType(type) {
    switch (type) {
        case 'Success':
            return 'success';
        case 'Error':
            return 'danger';
        case 'Warning':
            return 'warning';
        case 'Info':
            return 'info';
        default:
            return 'info';
    }
}

function createNotificationContainer() {
    const container = document.createElement('div');
    container.id = 'notification-container';
    container.className = 'position-fixed top-0 end-0 p-3';
    container.style.zIndex = '1050';
    document.body.appendChild(container);
    return container;
}

// Функция для отображения уведомлений из TempData
function showNotificationsFromTempData() {
    const notifications = document.getElementById('temp-notifications');
    if (notifications) {
        const data = JSON.parse(notifications.dataset.notifications);
        data.forEach(notification => {
            showNotification(notification.title, notification.message, notification.type, notification.isDismissible);
        });
    }
}

// Вызываем функцию при загрузке страницы
document.addEventListener('DOMContentLoaded', showNotificationsFromTempData); 