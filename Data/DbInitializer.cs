using Airport.Models;
using System.Security.Cryptography;
using System.Text;

namespace Airport.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // Проверяем, что база данных создана
            context.Database.EnsureCreated();

            // Инициализация пользователей
            if (!context.Users.Any())
            {
                // Добавляем тестового пользователя
                var user = new User
                {
                    Username = "admin",
                    Email = "admin@airport.com",
                    PasswordHash = HashPassword("admin123"),
                    Role = "Admin"
                };
                
                context.Users.Add(user);
                context.SaveChanges();
            }

            // Инициализация самолетов и других данных
            if (!context.Aircrafts.Any())
            {
                // Добавляем самолеты
                var aircrafts = new Aircraft[]
                {
                    new Aircraft { Name = "Boeing 737", Category = "Пассажирский", SeatCount = 150 },
                    new Aircraft { Name = "Airbus A320", Category = "Пассажирский", SeatCount = 180 },
                    new Aircraft { Name = "Boeing 777", Category = "Пассажирский", SeatCount = 300 },
                    new Aircraft { Name = "Sukhoi Superjet 100", Category = "Региональный", SeatCount = 90 }
                };

                foreach (var aircraft in aircrafts)
                {
                    context.Aircrafts.Add(aircraft);
                }
                context.SaveChanges();

                // Добавляем рейсы
                var flights = new Flight[]
                {
                    new Flight 
                    { 
                        FlightNumber = "SU1234", 
                        AircraftId = aircrafts[0].Id, 
                        DepartureTime = DateTime.Now.AddDays(1),
                        AvailableSeats = aircrafts[0].SeatCount,
                        Price = 15000
                    },
                    new Flight 
                    { 
                        FlightNumber = "SU5678", 
                        AircraftId = aircrafts[1].Id, 
                        DepartureTime = DateTime.Now.AddDays(2),
                        AvailableSeats = aircrafts[1].SeatCount,
                        Price = 20000
                    },
                    new Flight 
                    { 
                        FlightNumber = "SU9012", 
                        AircraftId = aircrafts[2].Id, 
                        DepartureTime = DateTime.Now.AddDays(3),
                        AvailableSeats = aircrafts[2].SeatCount,
                        Price = 35000
                    }
                };

                foreach (var flight in flights)
                {
                    context.Flights.Add(flight);
                }
                context.SaveChanges();

                // Добавляем посадки
                var landings = new Landing[]
                {
                    new Landing { Location = "Москва", Time = DateTime.Now.AddDays(1).AddHours(2), FlightId = flights[0].Id },
                    new Landing { Location = "Санкт-Петербург", Time = DateTime.Now.AddDays(2).AddHours(3), FlightId = flights[1].Id },
                    new Landing { Location = "Новосибирск", Time = DateTime.Now.AddDays(3).AddHours(4), FlightId = flights[2].Id }
                };

                foreach (var landing in landings)
                {
                    context.Landings.Add(landing);
                }
                context.SaveChanges();

                // Добавляем билеты (продаем несколько билетов)
                var tickets = new Ticket[]
                {
                    new Ticket 
                    { 
                        CashboxNumber = "К1", 
                        FlightId = flights[0].Id, 
                        Date = DateTime.Today, 
                        Time = DateTime.Now.TimeOfDay
                    },
                    new Ticket 
                    { 
                        CashboxNumber = "К2", 
                        FlightId = flights[1].Id, 
                        Date = DateTime.Today, 
                        Time = DateTime.Now.TimeOfDay
                    }
                };

                foreach (var ticket in tickets)
                {
                    context.Tickets.Add(ticket);
                    
                    // Уменьшаем количество свободных мест на рейсе
                    var flight = context.Flights.Find(ticket.FlightId);
                    if (flight != null && flight.AvailableSeats > 0)
                    {
                        flight.AvailableSeats--;
                    }
                }
                context.SaveChanges();
            }
        }

        // Вспомогательный метод для хеширования пароля
        private static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
} 