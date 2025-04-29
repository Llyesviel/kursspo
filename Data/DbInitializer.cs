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
                
                // Добавляем еще одного обычного пользователя
                var regularUser = new User
                {
                    Username = "user",
                    Email = "user@example.com",
                    PasswordHash = HashPassword("user123"),
                    Role = "User"
                };
                
                context.Users.Add(user);
                context.Users.Add(regularUser);
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
                        TicketNumber = GenerateTicketNumber(),
                        CashboxNumber = "1", 
                        FlightId = flights[0].Id, 
                        Date = DateTime.Today, 
                        Time = DateTime.Now.TimeOfDay,
                        SeatNumber = "12A",
                        PassengerName = "Иванов Иван Иванович",
                        DocumentNumber = "4509 123456",
                        ContactPhone = "+7 (900) 123-45-67",
                        ContactEmail = "ivanov@example.com",
                        PurchaseSource = "Offline",
                        Status = "Paid"
                    },
                    new Ticket 
                    { 
                        TicketNumber = GenerateTicketNumber(),
                        CashboxNumber = "2", 
                        FlightId = flights[1].Id, 
                        Date = DateTime.Today, 
                        Time = DateTime.Now.TimeOfDay,
                        SeatNumber = "15B",
                        PassengerName = "Петров Петр Петрович",
                        DocumentNumber = "4510 789012",
                        ContactPhone = "+7 (900) 765-43-21",
                        ContactEmail = "petrov@example.com",
                        PurchaseSource = "Offline",
                        Status = "Paid"
                    },
                    new Ticket 
                    { 
                        TicketNumber = GenerateTicketNumber(),
                        CashboxNumber = "0", 
                        FlightId = flights[0].Id, 
                        Date = DateTime.Today, 
                        Time = DateTime.Now.TimeOfDay,
                        SeatNumber = "14C",
                        PassengerName = "Сидоров Сидор Сидорович",
                        DocumentNumber = "4511 246810",
                        ContactPhone = "+7 (900) 111-22-33",
                        ContactEmail = "sidorov@example.com",
                        PurchaseSource = "Online",
                        Status = "Paid",
                        UserId = 2 // ID обычного пользователя
                    },
                    new Ticket 
                    { 
                        TicketNumber = GenerateTicketNumber(),
                        CashboxNumber = "0", 
                        FlightId = flights[2].Id, 
                        Date = DateTime.Today, 
                        Time = DateTime.Now.TimeOfDay,
                        SeatNumber = "21A",
                        PassengerName = "Александров Александр Александрович",
                        DocumentNumber = "4512 135790",
                        ContactPhone = "+7 (900) 444-55-66",
                        ContactEmail = "alex@example.com",
                        PurchaseSource = "Online",
                        Status = "Booked",
                        UserId = 2 // ID обычного пользователя
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
        
        // Метод для генерации уникального номера билета
        private static string GenerateTicketNumber()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            
            string part1 = new string(Enumerable.Repeat(chars, 4)
                .Select(s => s[random.Next(s.Length)]).ToArray());
                
            string part2 = new string(Enumerable.Repeat(chars, 4)
                .Select(s => s[random.Next(s.Length)]).ToArray());
                
            string part3 = new string(Enumerable.Repeat(chars, 4)
                .Select(s => s[random.Next(s.Length)]).ToArray());
                
            return $"{part1}-{part2}-{part3}";
        }
    }
} 