using Microsoft.EntityFrameworkCore;
using Airport.Models;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Airport.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // Проверяем, что база данных существует
            context.Database.EnsureCreated();

            // Если в базе есть данные, выходим
            if (context.Users.Any())
            {
                return;
            }

            Console.WriteLine("Начинаем инициализацию базы данных...");

            // Добавляем пользователей
            var users = new List<User>
            {
                new User { 
                    Username = "admin",
                    Email = "admin@example.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin"), 
                    Role = "Admin", 
                    CreatedAt = DateTime.Now 
                },
                new User { 
                    Username = "user",
                    Email = "user@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("user"), 
                    Role = "User", 
                    CreatedAt = DateTime.Now 
                },
            };
            
            context.Users.AddRange(users);
                context.SaveChanges();
            Console.WriteLine("Пользователи добавлены.");

            // Добавляем самолеты
            var aircrafts = new List<Aircraft>
            {
                new Aircraft { Name = "Boeing 737", SeatCount = 180, Category = "Пассажирский" },
                new Aircraft { Name = "Airbus A320", SeatCount = 150, Category = "Пассажирский" },
                new Aircraft { Name = "Sukhoi Superjet 100", SeatCount = 100, Category = "Региональный" },
                new Aircraft { Name = "Boeing 777", SeatCount = 300, Category = "Пассажирский дальнемагистральный" },
                new Aircraft { Name = "Airbus A380", SeatCount = 500, Category = "Пассажирский дальнемагистральный" },
                new Aircraft { Name = "Embraer E190", SeatCount = 114, Category = "Региональный" },
                new Aircraft { Name = "Airbus A321", SeatCount = 200, Category = "Пассажирский" },
                new Aircraft { Name = "Boeing 787", SeatCount = 250, Category = "Пассажирский дальнемагистральный" },
                new Aircraft { Name = "ATR 72", SeatCount = 70, Category = "Региональный" },
                new Aircraft { Name = "Boeing 747", SeatCount = 400, Category = "Пассажирский дальнемагистральный" }
            };
            
            context.Aircrafts.AddRange(aircrafts);
                context.SaveChanges();
            Console.WriteLine("Самолеты добавлены.");

            // Создаем список рейсов, посадок, вылетов и билетов
            var flights = new List<Flight>();
            var departures = new List<Departure>();
            var landings = new List<Landing>();
            var tickets = new List<Ticket>();

                var random = new Random();
            string[] airlines = { "SU", "S7", "U6", "DP", "UT" };
            var today = DateTime.Today;

            // Список статусов рейсов
            string[] statuses = { "Активен", "Задержан", "Отменен", "Прибыл", "Вылетел" };

            int flightCounter = 1;

            // Функция для создания рейсов между городами
            void CreateFlights(string departureCity, string arrivalCity, int count, decimal minPrice, decimal maxPrice, int startHour = 6, int endHour = 22)
            {
                for (int i = 0; i < count; i++)
                {
                    // Выбираем случайный самолет
                    var aircraft = aircrafts[random.Next(aircrafts.Count)];
                    
                    // Генерируем номер рейса
                    string airline = airlines[random.Next(airlines.Length)];
                    string flightNumber = $"{airline}{random.Next(1000, 9999)}";
                    
                    // Случайное время вылета
                    var departureHour = random.Next(startHour, endHour);
                    var departureMinute = random.Next(0, 4) * 15; // 0, 15, 30, 45
                    var departureTime = today.AddDays(random.Next(1, 30))
                        .AddHours(departureHour)
                        .AddMinutes(departureMinute);
                    
                    // Определяем время в пути в зависимости от направления
                    int flightDurationHours;
                    if ((departureCity == "Москва" && arrivalCity == "Санкт-Петербург") ||
                        (departureCity == "Санкт-Петербург" && arrivalCity == "Москва"))
                    {
                        flightDurationHours = 1; // Короткие рейсы
                    }
                    else if (departureCity == "Москва" && 
                           (arrivalCity == "Казань" || arrivalCity == "Краснодар" || 
                            arrivalCity == "Калининград" || arrivalCity == "Сочи"))
                    {
                        flightDurationHours = 2; // Средние рейсы
                    }
                    else
                    {
                        flightDurationHours = 4; // Длинные рейсы (например, до Новосибирска или Екатеринбурга)
                    }
                    
                    // Случайная цена в заданном диапазоне
                    decimal price = random.Next((int)minPrice, (int)maxPrice + 1);
                    
                    // Случайное количество доступных мест (немного меньше, чем общее количество мест)
                    int soldSeats = random.Next(0, (int)(aircraft.SeatCount * 0.8));
                    int availableSeats = aircraft.SeatCount - soldSeats;
                    
                    // Случайный статус рейса (преимущественно активные)
                    string status = random.Next(100) < 80 ? "Активен" : statuses[random.Next(1, statuses.Length)];

                    // Создаем рейс
                    var flight = new Flight
                    { 
                        FlightNumber = flightNumber, 
                        AircraftId = aircraft.Id, 
                        DepartureTime = departureTime,
                        AvailableSeats = availableSeats,
                        Price = price,
                        Status = status
                    };
                    
                    flights.Add(flight);
                    
                    // Счетчик для отслеживания общего количества рейсов
                    flightCounter++;
                }
            }

            // Создаем рейсы по заданным направлениям
            CreateFlights("Москва", "Санкт-Петербург", 10, 3000, 5000);
            CreateFlights("Санкт-Петербург", "Москва", 10, 3000, 5000);
            CreateFlights("Москва", "Казань", 7, 3000, 5000);
            CreateFlights("Москва", "Новосибирск", 7, 8000, 12000);
            CreateFlights("Москва", "Екатеринбург", 7, 5000, 7000);
            CreateFlights("Москва", "Сочи", 7, 5000, 8000);
            CreateFlights("Москва", "Краснодар", 7, 4000, 6000);
            CreateFlights("Москва", "Калининград", 7, 4500, 6500);
            CreateFlights("Санкт-Петербург", "Казань", 3, 4000, 6000);
            CreateFlights("Санкт-Петербург", "Сочи", 3, 6000, 9000);
            CreateFlights("Казань", "Москва", 3, 3000, 5000);
            CreateFlights("Новосибирск", "Москва", 3, 8000, 12000);
            
            Console.WriteLine($"Создано {flights.Count} рейсов");
            
            // Сохраняем рейсы в базу данных
            context.Flights.AddRange(flights);
            context.SaveChanges();
            
            // Создаем посадки и вылеты для каждого рейса
            int departureIndex = 0;
            int landingIndex = 0;
            
            // Для каждого рейса создаем соответствующие записи посадки и вылета
                foreach (var flight in flights)
            {
                // Определяем города прилета и вылета в зависимости от индекса рейса
                string departureCity;
                string arrivalCity;
                
                if (departureIndex < 10) // Первые 10 рейсов: Москва - Санкт-Петербург
                {
                    departureCity = "Москва";
                    arrivalCity = "Санкт-Петербург";
                }
                else if (departureIndex < 20) // Следующие 10 рейсов: Санкт-Петербург - Москва
                {
                    departureCity = "Санкт-Петербург";
                    arrivalCity = "Москва";
                }
                else if (departureIndex < 27) // Москва - Казань
                {
                    departureCity = "Москва";
                    arrivalCity = "Казань";
                }
                else if (departureIndex < 34) // Москва - Новосибирск
                {
                    departureCity = "Москва";
                    arrivalCity = "Новосибирск";
                }
                else if (departureIndex < 41) // Москва - Екатеринбург
                {
                    departureCity = "Москва";
                    arrivalCity = "Екатеринбург";
                }
                else if (departureIndex < 48) // Москва - Сочи
                {
                    departureCity = "Москва";
                    arrivalCity = "Сочи";
                }
                else if (departureIndex < 55) // Москва - Краснодар
                {
                    departureCity = "Москва";
                    arrivalCity = "Краснодар";
                }
                else if (departureIndex < 62) // Москва - Калининград
                {
                    departureCity = "Москва";
                    arrivalCity = "Калининград";
                }
                else if (departureIndex < 65) // Санкт-Петербург - Казань
                {
                    departureCity = "Санкт-Петербург";
                    arrivalCity = "Казань";
                }
                else if (departureIndex < 68) // Санкт-Петербург - Сочи
                {
                    departureCity = "Санкт-Петербург";
                    arrivalCity = "Сочи";
                }
                else if (departureIndex < 71) // Казань - Москва
                {
                    departureCity = "Казань";
                    arrivalCity = "Москва";
                }
                else // Новосибирск - Москва
                {
                    departureCity = "Новосибирск";
                    arrivalCity = "Москва";
                }
                
                // Определяем время полета в зависимости от маршрута
                int flightDurationHours;
                if ((departureCity == "Москва" && arrivalCity == "Санкт-Петербург") ||
                    (departureCity == "Санкт-Петербург" && arrivalCity == "Москва"))
                {
                    flightDurationHours = 1; // Короткие рейсы
                }
                else if (departureCity == "Москва" && 
                       (arrivalCity == "Казань" || arrivalCity == "Краснодар" || 
                        arrivalCity == "Калининград" || arrivalCity == "Сочи"))
                {
                    flightDurationHours = 2; // Средние рейсы
                }
                else
                {
                    flightDurationHours = 4; // Длинные рейсы
                }
                
                // Создаем вылет
                var departure = new Departure
                {
                    Location = departureCity,
                    Time = flight.DepartureTime,
                    FlightId = flight.Id
                };
                departures.Add(departure);
                
                // Создаем посадку (время посадки = время вылета + продолжительность полета)
                var landing = new Landing
                {
                    Location = arrivalCity,
                    Time = flight.DepartureTime.AddHours(flightDurationHours),
                    FlightId = flight.Id
                };
                landings.Add(landing);
                
                // Создаем несколько билетов для рейса
                int ticketsToCreate = random.Next(1, 10); // От 1 до 10 билетов на рейс
                
                for (int t = 0; t < ticketsToCreate && flight.AvailableSeats < 180; t++)
                {
                    // Генерируем номер места (ряд от 1 до 30, место от A до F)
                    int row = random.Next(1, 31);
                    char seat = (char)('A' + random.Next(0, 6)); // A-F
                    string seatNumber = $"{row}{seat}";
                    
                    // Генерируем уникальный номер билета
                    string ticketNumber = $"{GenerateRandomString(4)}-{GenerateRandomString(4)}-{GenerateRandomString(4)}";
                    
                    // Создаем билет
                    var ticket = new Ticket
                    {
                        TicketNumber = ticketNumber,
                        CashboxNumber = random.Next(1, 6).ToString(),
                            FlightId = flight.Id, 
                        PassengerName = GenerateRandomName(),
                        DocumentNumber = $"{random.Next(1000, 9999)} {random.Next(100000, 999999)}",
                            SeatNumber = seatNumber,
                        Date = today.AddDays(-random.Next(1, 15)), // Билет куплен от 1 до 15 дней назад
                        Time = TimeSpan.FromHours(random.Next(9, 18)).Add(TimeSpan.FromMinutes(random.Next(0, 60))),
                        PurchaseSource = random.Next(2) == 0 ? "Online" : "Offline",
                        Status = "Paid"
                    };
                    
                    tickets.Add(ticket);
                }
                
                departureIndex++;
                landingIndex++;
            }
            
            // Сохраняем посадки, вылеты и билеты в базу данных
            context.Departures.AddRange(departures);
            context.Landings.AddRange(landings);
            context.Tickets.AddRange(tickets);
            context.SaveChanges();
            
            Console.WriteLine($"Добавлено {departures.Count} вылетов, {landings.Count} посадок и {tickets.Count} билетов.");

            Console.WriteLine("Инициализация базы данных завершена успешно.");
        }
        
        // Вспомогательный метод для генерации случайной строки заданной длины
        private static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        
        // Вспомогательный метод для генерации случайного имени
        private static string GenerateRandomName()
        {
            string[] firstNames = { "Иван", "Петр", "Алексей", "Сергей", "Михаил", "Андрей", "Дмитрий", "Артем", "Максим", "Николай" };
            string[] lastNames = { "Иванов", "Петров", "Сидоров", "Смирнов", "Кузнецов", "Соколов", "Попов", "Лебедев", "Козлов", "Новиков" };
            
            var random = new Random();
            return $"{lastNames[random.Next(lastNames.Length)]} {firstNames[random.Next(firstNames.Length)]}";
        }
    }
} 