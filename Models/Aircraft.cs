using System.ComponentModel.DataAnnotations;

namespace Airport.Models
{
    public class Aircraft
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Поле Название обязательно для заполнения")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Поле Категория обязательно для заполнения")]
        public string Category { get; set; } = null!;

        [Required(ErrorMessage = "Поле Количество мест обязательно для заполнения")]
        [Range(1, int.MaxValue, ErrorMessage = "Введите значение больше или равное 1")]
        public int SeatCount { get; set; }

        // Навигационное свойство
        public ICollection<Flight> Flights { get; set; } = new List<Flight>();
    }
} 