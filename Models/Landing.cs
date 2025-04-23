namespace Airport.Models
{
    public class Landing
    {
        public int Id { get; set; }
        public string Location { get; set; } = null!;
        public DateTime Time { get; set; }
        
        // Внешний ключ
        public int FlightId { get; set; }
        
        // Навигационное свойство
        public Flight Flight { get; set; } = null!;
    }
} 