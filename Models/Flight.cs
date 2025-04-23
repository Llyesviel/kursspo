namespace Airport.Models
{
    public class Flight
    {
        public int Id { get; set; }
        public string FlightNumber { get; set; } = null!;
        public DateTime DepartureTime { get; set; }
        public int AvailableSeats { get; set; }
        public decimal Price { get; set; }

        // Внешний ключ
        public int AircraftId { get; set; }
        
        // Навигационные свойства
        public Aircraft Aircraft { get; set; } = null!;
        public ICollection<Landing> Landings { get; set; } = new List<Landing>();
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

        // Процент загруженности рейса (не сохраняется в базу данных)
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public int LoadPercentage { get; set; }
    }
} 