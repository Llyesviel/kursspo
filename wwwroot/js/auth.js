document.addEventListener('DOMContentLoaded', function() {
    // Анимация появления формы
    const authCard = document.querySelector('.auth-card');
    if (authCard) {
        setTimeout(() => {
            authCard.classList.add('visible');
        }, 100);
    }

    // Добавляем класс для активного поля ввода
    const inputs = document.querySelectorAll('.auth-form-group input');
    inputs.forEach(input => {
        // При загрузке проверяем, есть ли значение
        if (input.value.trim() !== '') {
            input.classList.add('has-value');
        }
        
        // При фокусе добавляем класс
        input.addEventListener('focus', function() {
            this.parentElement.classList.add('focused');
        });
        
        // При потере фокуса проверяем, есть ли значение
        input.addEventListener('blur', function() {
            this.parentElement.classList.remove('focused');
            if (this.value.trim() !== '') {
                this.classList.add('has-value');
            } else {
                this.classList.remove('has-value');
            }
        });
        
        // При вводе проверяем, есть ли значение
        input.addEventListener('input', function() {
            if (this.value.trim() !== '') {
                this.classList.add('has-value');
            } else {
                this.classList.remove('has-value');
            }
        });
    });
}); 