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
                    new Aircraft { Name = "Boeing 737-800", Category = "Пассажирский", SeatCount = 189 },
                    new Aircraft { Name = "Airbus A320neo", Category = "Пассажирский", SeatCount = 180 },
                    new Aircraft { Name = "Boeing 777-300ER", Category = "Пассажирский дальнемагистральный", SeatCount = 402 },
                    new Aircraft { Name = "Sukhoi Superjet 100", Category = "Региональный", SeatCount = 98 },
                    new Aircraft { Name = "Airbus A350-900", Category = "Пассажирский дальнемагистральный", SeatCount = 325 },
                    new Aircraft { Name = "Boeing 787-9 Dreamliner", Category = "Пассажирский дальнемагистральный", SeatCount = 290 },
                    new Aircraft { Name = "Airbus A330-300", Category = "Пассажирский дальнемагистральный", SeatCount = 293 },
                    new Aircraft { Name = "Embraer E190", Category = "Региональный", SeatCount = 114 },
                    new Aircraft { Name = "Boeing 747-8", Category = "Пассажирский дальнемагистральный", SeatCount = 467 },
                    new Aircraft { Name = "MC-21-300", Category = "Пассажирский", SeatCount = 211 }
                };

                foreach (var aircraft in aircrafts)
                {
                    context.Aircrafts.Add(aircraft);
                }
                context.SaveChanges();

                // Список городов для рейсов
                string[] cities = {
                    "Москва", "Санкт-Петербург", "Сочи", "Екатеринбург", "Новосибирск",
                    "Краснодар", "Казань", "Самара", "Ростов-на-Дону", "Уфа",
                    "Красноярск", "Пермь", "Воронеж", "Волгоград", "Минеральные Воды",
                    "Тюмень", "Кемерово", "Мурманск", "Калининград", "Анапа",
                    "Стамбул", "Дубай", "Париж", "Рим", "Барселона",
                    "Берлин", "Лондон", "Нью-Йорк", "Токио", "Пекин"
                };

                // Случайная генерация рейсов
                var random = new Random();
                var flights = new List<Flight>();
                var airlines = new string[] { "SU", "S7", "U6", "DP", "UT", "N4", "5N", "EO", "TK", "EK" };
                
                for (int i = 0; i < 60; i++)
                {
                    var aircraft = aircrafts[random.Next(aircrafts.Length)];
                    var airline = airlines[random.Next(airlines.Length)];
                    var flightNumber = $"{airline}{random.Next(1000, 9999)}";
                    
                    // Случайно определяем, сколько мест уже занято
                    int occupiedSeats = random.Next(0, (int)(aircraft.SeatCount * 0.9));
                    var availableSeats = aircraft.SeatCount - occupiedSeats;
                    
                    // Случайная цена от 5000 до 150000
                    var basePrice = random.Next(5000, 150000);
                    // Округление цены до "красивых" значений (кратно 500)
                    var price = (decimal)(Math.Round(basePrice / 500.0, 0) * 500);
                    
                    // Случайная дата в ближайшие 30 дней
                    var departureTime = DateTime.Now.AddDays(random.Next(1, 30))
                        .AddHours(random.Next(0, 24))
                        .AddMinutes(random.Next(0, 4) * 15); // Округляем до 0, 15, 30, 45 минут
                    
                    flights.Add(new Flight 
                    { 
                        FlightNumber = flightNumber, 
                        AircraftId = aircraft.Id, 
                        DepartureTime = departureTime,
                        AvailableSeats = availableSeats,
                        Price = price
                    });
                }

                foreach (var flight in flights)
                {
                    context.Flights.Add(flight);
                }
                context.SaveChanges();

                // Добавляем посадки и вылеты для всех рейсов
                var landings = new List<Landing>();
                var departures = new List<Departure>();
                
                foreach (var flight in flights)
                {
                    // Выбираем случайный город отправления
                    string departureCity = cities[random.Next(cities.Length)];
                    
                    // Выбираем случайный город назначения, но убедимся, что он отличается от города отправления
                    string arrivalCity;
                    do 
                    {
                        arrivalCity = cities[random.Next(cities.Length)];
                    } while (arrivalCity == departureCity);
                    
                    // Добавляем вылет
                    departures.Add(new Departure 
                    { 
                        Location = departureCity, 
                        Time = flight.DepartureTime, 
                        FlightId = flight.Id 
                    });
                    
                    // Добавляем посадку (примерно через 1-8 часов после вылета)
                    var flightDuration = TimeSpan.FromHours(random.Next(1, 9));
                    landings.Add(new Landing 
                    { 
                        Location = arrivalCity, 
                        Time = flight.DepartureTime.Add(flightDuration), 
                        FlightId = flight.Id 
                    });
                }

                foreach (var departure in departures)
                {
                    context.Departures.Add(departure);
                }
                
                foreach (var landing in landings)
                {
                    context.Landings.Add(landing);
                }
                context.SaveChanges();

                // Добавляем билеты (продаем несколько билетов)
                var tickets = new List<Ticket>();
                
                // Для каждого полета создаем от 0 до 20 билетов
                foreach (var flight in flights)
                {
                    int ticketsToCreate = random.Next(0, 21);
                    
                    // Множество для отслеживания занятых мест
                    var occupiedSeats = new HashSet<string>();
                    
                    for (int i = 0; i < ticketsToCreate && flight.AvailableSeats > 0; i++)
                    {
                        // Генерация номера места (ряд от 1 до 50, место от A до F)
                        string seatNumber;
                        do
                        {
                            int row = random.Next(1, 51);
                            char letter = (char)('A' + random.Next(0, 6)); // A-F
                            seatNumber = $"{row}{letter}";
                        } while (occupiedSeats.Contains(seatNumber));
                        
                        occupiedSeats.Add(seatNumber);
                        
                        // Генерация случайных данных для пассажира
                        string[] firstNames = { "Иван", "Петр", "Александр", "Сергей", "Дмитрий", "Алексей", "Михаил", "Николай", "Андрей", "Владимир" };
                        string[] lastNames = { "Иванов", "Петров", "Сидоров", "Смирнов", "Кузнецов", "Попов", "Васильев", "Соколов", "Михайлов", "Новиков" };
                        string[] patronymics = { "Иванович", "Петрович", "Александрович", "Сергеевич", "Дмитриевич", "Алексеевич", "Михайлович", "Николаевич", "Андреевич", "Владимирович" };
                        
                        string passengerName = $"{lastNames[random.Next(lastNames.Length)]} {firstNames[random.Next(firstNames.Length)]} {patronymics[random.Next(patronymics.Length)]}";
                        string docNumber = $"{random.Next(1000, 9999)} {random.Next(100000, 999999)}";
                        string contactPhone = $"+7 ({random.Next(900, 999)}) {random.Next(100, 999)}-{random.Next(10, 99)}-{random.Next(10, 99)}";
                        string contactEmail = $"{lastNames[random.Next(lastNames.Length)].ToLower()}{random.Next(10, 99)}@{new string[] { "gmail.com", "mail.ru", "yandex.ru", "outlook.com" }[random.Next(4)]}";
                        
                        // Определяем источник покупки (20% онлайн, 80% оффлайн)
                        string purchaseSource = random.Next(0, 100) < 20 ? "Online" : "Offline";
                        string cashboxNumber = purchaseSource == "Online" ? "0" : random.Next(1, 10).ToString();
                        
                        // Определяем статус билета (80% оплачены, 15% забронированы, 5% отменены)
                        int statusRandom = random.Next(0, 100);
                        string status;
                        if (statusRandom < 80) status = "Paid";
                        else if (statusRandom < 95) status = "Booked";
                        else status = "Cancelled";
                        
                        // Определяем, принадлежит ли билет пользователю
                        int? userId = purchaseSource == "Online" ? 2 : null;
                        
                        tickets.Add(new Ticket 
                        { 
                            TicketNumber = GenerateTicketNumber(),
                            CashboxNumber = cashboxNumber, 
                            FlightId = flight.Id, 
                            Date = DateTime.Today.AddDays(-random.Next(0, 14)), // Билет мог быть куплен в течение последних двух недель
                            Time = DateTime.Now.AddHours(-random.Next(0, 24)).TimeOfDay,
                            SeatNumber = seatNumber,
                            PassengerName = passengerName,
                            DocumentNumber = docNumber,
                            ContactPhone = contactPhone,
                            ContactEmail = contactEmail,
                            PurchaseSource = purchaseSource,
                            Status = status,
                            UserId = userId
                        });
                        
                        // Уменьшаем количество свободных мест на рейсе (только для оплаченных и забронированных билетов)
                        if (status != "Cancelled")
                        {
                            flight.AvailableSeats--;
                        }
                    }
                }
                
                foreach (var ticket in tickets)
                {
                    context.Tickets.Add(ticket);
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