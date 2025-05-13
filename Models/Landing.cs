namespace Airport.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Landing
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Укажите место посадки")]
        public string Location { get; set; } = null!;
        
        [Required(ErrorMessage = "Укажите время посадки")]
        public DateTime Time { get; set; }
        
        // Внешний ключ
        [Required(ErrorMessage = "Выберите рейс")]
        [Range(1, int.MaxValue, ErrorMessage = "Необходимо выбрать рейс")]
        public int FlightId { get; set; }
        
        // Навигационное свойство
        [ForeignKey("FlightId")]
        public Flight? Flight { get; set; }
    }
} 