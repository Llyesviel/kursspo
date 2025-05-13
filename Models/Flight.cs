using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Airport.Models
{
    public class Flight
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Номер рейса обязателен")]
        [Display(Name = "№ рейса")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Номер рейса должен содержать от 2 до 50 символов")]
        public string FlightNumber { get; set; } = null!;
        
        [Required(ErrorMessage = "Время вылета обязательно")]
        [Display(Name = "Время вылета")]
        public DateTime DepartureTime { get; set; }
        
        [Required(ErrorMessage = "Количество доступных мест обязательно")]
        [Range(0, int.MaxValue, ErrorMessage = "Количество мест не может быть отрицательным")]
        [Display(Name = "Доступные места")]
        public int AvailableSeats { get; set; }
        
        [Required(ErrorMessage = "Цена билета обязательна")]
        [Range(0.01, 1000000, ErrorMessage = "Цена должна быть положительной и не более 1 000 000")]
        [Display(Name = "Цена")]
        public decimal Price { get; set; }

        // Внешний ключ
        [Required(ErrorMessage = "Выберите самолет из списка")]
        [Range(1, int.MaxValue, ErrorMessage = "Выберите самолет из списка")]
        [Display(Name = "Самолет")]
        public int AircraftId { get; set; }
        
        // Добавляем статус рейса
        [Display(Name = "Статус")]
        public string? Status { get; set; }
        
        // Навигационные свойства
        [ForeignKey("AircraftId")]
        public Aircraft? Aircraft { get; set; }
        
        public ICollection<Landing>? Landings { get; set; } = new List<Landing>();
        public ICollection<Ticket>? Tickets { get; set; } = new List<Ticket>();
        public ICollection<Departure>? Departures { get; set; } = new List<Departure>();

        // Процент загруженности рейса (не сохраняется в базу данных)
        [NotMapped]
        public int LoadPercentage { get; set; }
    }
} 