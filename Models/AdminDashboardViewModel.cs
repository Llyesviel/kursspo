using System.Collections.Generic;

namespace Airport.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalAircrafts { get; set; }
        public int TotalFlights { get; set; }
        public int TotalTickets { get; set; }
        public int TotalLandings { get; set; }
        public int TotalDepartures { get; set; }
        public List<Ticket> RecentTickets { get; set; } = new List<Ticket>();
        public List<Flight> UpcomingFlights { get; set; } = new List<Flight>();
    }
} 