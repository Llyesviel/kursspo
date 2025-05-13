namespace Airport.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        
        // Уникальный номер билета (например, XXXX-XXXX-XXXX)
        public string TicketNumber { get; set; } = null!;
        
        // Кассовая информация
        public string CashboxNumber { get; set; } = null!;
        
        // Временная информация
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        
        // Информация о месте
        public string SeatNumber { get; set; } = null!;
        
        // Информация о пассажире
        public string? PassengerName { get; set; }
        public string? DocumentNumber { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }
        
        // Источник покупки (Online/Offline)
        public string PurchaseSource { get; set; } = "Offline";
        
        // Статус билета (Booked, Paid, CheckedIn, Cancelled)
        public string Status { get; set; } = "Paid";
        
        // Идентификатор пользователя (для онлайн-бронирований)
        public int? UserId { get; set; }
        
        // Внешний ключ
        public int FlightId { get; set; }
        
        // Навигационные свойства
        public Flight Flight { get; set; } = null!;
        public User? User { get; set; }
    }
} 