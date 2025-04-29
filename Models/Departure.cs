using System;
using System.ComponentModel.DataAnnotations;

namespace Airport.Models
{
    public class Departure
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Укажите место вылета")]
        public string Location { get; set; } = null!;

        [Required(ErrorMessage = "Укажите время вылета")]
        public DateTime Time { get; set; }

        [Required(ErrorMessage = "Выберите рейс")]
        public int FlightId { get; set; }

        // Навигационное свойство
        public Flight? Flight { get; set; }
    }
} 