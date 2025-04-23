namespace Airport.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public string CashboxNumber { get; set; } = null!;
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }

        // Внешний ключ
        public int FlightId { get; set; }
        
        // Навигационное свойство
        public Flight Flight { get; set; } = null!;
    }
} 