// Функции для административной панели

// Переключение мобильного меню
document.addEventListener('DOMContentLoaded', function() {
    const menuToggle = document.getElementById('menuToggle');
    const adminSidebar = document.getElementById('adminSidebar');
    const adminOverlay = document.getElementById('adminOverlay');
    const adminContent = document.getElementById('adminContent');
    
    if (menuToggle && adminSidebar && adminOverlay && adminContent) {
        menuToggle.addEventListener('click', function() {
            adminSidebar.classList.toggle('show');
            adminOverlay.classList.toggle('show');
            adminContent.classList.toggle('sidebar-open');
        });
        
        adminOverlay.addEventListener('click', function() {
            adminSidebar.classList.remove('show');
            adminOverlay.classList.remove('show');
            adminContent.classList.remove('sidebar-open');
        });
    }
    
    // Активация текущего пункта меню
    const currentPath = window.location.pathname;
    const menuLinks = document.querySelectorAll('.admin-menu-link');
    
    menuLinks.forEach(link => {
        const href = link.getAttribute('href');
        if (href && currentPath.startsWith(href)) {
            link.classList.add('active');
        }
    });
});

// Подтверждение удаления с модальным окном
function confirmDelete(url, name) {
    const modal = document.getElementById('deleteConfirmationModal');
    if (modal) {
        const modalBody = modal.querySelector('.modal-body p');
        const confirmBtn = document.getElementById('confirmDeleteButton');
        
        modalBody.textContent = `Вы уверены, что хотите удалить "${name}"?`;
        confirmBtn.setAttribute('data-url', url);
        
        const bootstrapModal = new bootstrap.Modal(modal);
        bootstrapModal.show();
        
        confirmBtn.onclick = function() {
            window.location.href = this.getAttribute('data-url');
        };
    }
} 