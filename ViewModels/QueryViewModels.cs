using System;

namespace Airport.ViewModels
{
    public class AircraftLoadViewModel
    {
        public string FlightNumber { get; set; } = null!;
        public string AircraftName { get; set; } = null!;
        public DateTime DepartureTime { get; set; }
        public int OccupiedSeats { get; set; }
        public int TotalSeats { get; set; }
        public double LoadPercentage { get; set; }
    }

    public class ExpensiveFlightViewModel
    {
        public string FlightNumber { get; set; } = null!;
        public string AircraftName { get; set; } = null!;
        public DateTime DepartureTime { get; set; }
        public int TicketCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class UnderutilizedFlightViewModel
    {
        public string FlightNumber { get; set; } = null!;
        public string AircraftName { get; set; } = null!;
        public DateTime DepartureTime { get; set; }
        public int AvailableSeats { get; set; }
        public int TotalSeats { get; set; }
        public double EmptyPercentage { get; set; }
    }
} 