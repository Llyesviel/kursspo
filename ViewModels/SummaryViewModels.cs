using System;
using System.Collections.Generic;

namespace Airport.ViewModels
{
    public class AircraftSummaryViewModel
    {
        public int Id { get; set; }
        public string Model { get; set; } = null!;
        public string Manufacturer { get; set; } = null!;
        public int TotalSeats { get; set; }
        public int FlightsCount { get; set; }
    }

    public class FlightSummaryViewModel
    {
        public int Id { get; set; }
        public string FlightNumber { get; set; } = null!;
        public DateTime DepartureTime { get; set; }
        public string Aircraft { get; set; } = null!;
        public int AvailableSeats { get; set; }
        public int TotalSeats { get; set; }
        public List<LandingSummaryViewModel> Landings { get; set; } = new();
    }

    public class LandingSummaryViewModel
    {
        public string Location { get; set; } = null!;
        public TimeSpan Time { get; set; }
    }

    public class TicketSummaryViewModel
    {
        public int Id { get; set; }
        public string CashboxNumber { get; set; } = null!;
        public string FlightNumber { get; set; } = null!;
        public string AircraftModel { get; set; } = null!;
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public DateTime DepartureTime { get; set; }
    }
} 