using Microsoft.Data.Sqlite;
using VkBotDb.Models;

namespace VkBotDb.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                -- Таблица билетов
                CREATE TABLE IF NOT EXISTS Tickets (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    EventName TEXT NOT NULL,
                    Date TEXT NOT NULL,
                    TotalTickets INTEGER NOT NULL,
                    SoldTickets INTEGER DEFAULT 0,
                    Price DECIMAL(10,2) NOT NULL,
                    Type TEXT NOT NULL DEFAULT 'online'
                );

                -- Таблица брони ресторана
                CREATE TABLE IF NOT EXISTS RestaurantReservations (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Date TEXT NOT NULL,
                    TimeSlot TEXT NOT NULL,
                    AvailableTables INTEGER NOT NULL,
                    TotalTables INTEGER NOT NULL
                );

                -- Таблица загруженности аквапарка
                CREATE TABLE IF NOT EXISTS WaterparkOccupancy (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Date TEXT NOT NULL,
                    TimeSlot TEXT NOT NULL,
                    CurrentVisitors INTEGER NOT NULL,
                    MaxCapacity INTEGER NOT NULL
                );

                -- Таблица тарифов
                CREATE TABLE IF NOT EXISTS Tariffs (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Description TEXT NOT NULL,
                    Price DECIMAL(10,2) NOT NULL,
                    Duration TEXT NOT NULL,
                    Category TEXT NOT NULL
                );
            ";
            command.ExecuteNonQuery();

            AddSampleData(connection);
        }

        private void AddSampleData(SqliteConnection connection)
        {
            var checkCommand = new SqliteCommand("SELECT COUNT(*) FROM Tickets", connection);
            var count = (long)checkCommand.ExecuteScalar();

            if (count == 0)
            {
                // Тестовые билеты в аквапарк
                var insertTickets = new SqliteCommand(@"
                    INSERT INTO Tickets (EventName, Date, TotalTickets, SoldTickets, Price, Type) 
                    VALUES 
                    ('Билет в аквапарк - Утренний сеанс', '2025-12-25', 100, 25, 1500.00, 'online'),
                    ('Билет в аквапарк - Дневной сеанс', '2025-12-25', 80, 40, 1800.00, 'online'),
                    ('Билет в аквапарк - Вечерний сеанс', '2025-12-25', 60, 15, 2000.00, 'online'),
                    ('Билет в аквапарк - Утренний сеанс', '2025-12-26', 100, 10, 1500.00, 'online'),
                    ('Билет в аквапарк - Дневной сеанс', '2025-12-26', 80, 20, 1800.00, 'online'),
                    ('Билет в аквапарк - Вечерний сеанс', '2025-12-26', 60, 5, 2000.00, 'online'),
                    ('Билет в аквапарк - Утренний сеанс', '2025-12-27', 100, 35, 1500.00, 'online'),
                    ('Билет в аквапарк - Дневной сеанс', '2025-12-27', 80, 45, 1800.00, 'online'),
                    ('Билет в аквапарк - Вечерний сеанс', '2025-12-27', 60, 25, 2000.00, 'online'),
                    ('Детский билет в аквапарк', '2025-12-25', 50, 10, 800.00, 'online'),
                    ('Детский билет в аквапарк', '2025-12-26', 50, 5, 800.00, 'online'),
                    ('Детский билет в аквапарк', '2025-12-27', 50, 15, 800.00, 'online'),
                    ('Семейный билет (2+2)', '2025-12-25', 30, 5, 4500.00, 'online'),
                    ('Семейный билет (2+2)', '2025-12-26', 30, 2, 4500.00, 'online'),
                    ('Семейный билет (2+2)', '2025-12-27', 30, 8, 4500.00, 'online'),
                    ('VIP билет - Все включено', '2025-12-25', 20, 12, 5000.00, 'online'),
                    ('VIP билет - Все включено', '2025-12-26', 20, 8, 5000.00, 'online'),
                    ('VIP билет - Все включено', '2025-12-27', 20, 15, 5000.00, 'online'),
                    ('Групповой билет (10+ человек)', '2025-12-25', 10, 3, 12000.00, 'online'),
                    ('Групповой билет (10+ человек)', '2025-12-26', 10, 1, 12000.00, 'online'),
                    ('Групповой билет (10+ человек)', '2025-12-27', 10, 6, 12000.00, 'online')
                ", connection);
                insertTickets.ExecuteNonQuery();

                // Бронь ресторана
                var insertRestaurant = new SqliteCommand(@"
                    INSERT INTO RestaurantReservations (Date, TimeSlot, AvailableTables, TotalTables) 
                    VALUES 
                    ('2025-12-25', '10:00-12:00', 5, 15),
                    ('2025-12-25', '12:00-14:00', 2, 15),
                    ('2025-12-25', '14:00-16:00', 8, 15),
                    ('2025-12-25', '16:00-18:00', 4, 15),
                    ('2025-12-25', '18:00-20:00', 3, 15),
                    ('2025-12-25', '20:00-22:00', 1, 15),
                    ('2025-12-26', '10:00-12:00', 12, 15),
                    ('2025-12-26', '12:00-14:00', 9, 15),
                    ('2025-12-26', '14:00-16:00', 11, 15),
                    ('2025-12-26', '16:00-18:00', 8, 15),
                    ('2025-12-26', '18:00-20:00', 6, 15),
                    ('2025-12-26', '20:00-22:00', 4, 15),
                    ('2025-12-27', '10:00-12:00', 10, 15),
                    ('2025-12-27', '12:00-14:00', 7, 15),
                    ('2025-12-27', '14:00-16:00', 5, 15),
                    ('2025-12-27', '16:00-18:00', 9, 15),
                    ('2025-12-27', '18:00-20:00', 3, 15),
                    ('2025-12-27', '20:00-22:00', 2, 15)
                ", connection);
                insertRestaurant.ExecuteNonQuery();

                // Аквапарк - загруженность
                var insertWaterpark = new SqliteCommand(@"
                    INSERT INTO WaterparkOccupancy (Date, TimeSlot, CurrentVisitors, MaxCapacity) 
                    VALUES 
                    ('2025-12-25', '08:00-10:00', 25, 100),
                    ('2025-12-25', '10:00-12:00', 45, 100),
                    ('2025-12-25', '12:00-14:00', 80, 100),
                    ('2025-12-25', '14:00-16:00', 95, 100),
                    ('2025-12-25', '16:00-18:00', 75, 100),
                    ('2025-12-25', '18:00-20:00', 40, 100),
                    ('2025-12-26', '08:00-10:00', 15, 100),
                    ('2025-12-26', '10:00-12:00', 30, 100),
                    ('2025-12-26', '12:00-14:00', 60, 100),
                    ('2025-12-26', '14:00-16:00', 85, 100),
                    ('2025-12-26', '16:00-18:00', 70, 100),
                    ('2025-12-26', '18:00-20:00', 35, 100),
                    ('2025-12-27', '08:00-10:00', 20, 100),
                    ('2025-12-27', '10:00-12:00', 50, 100),
                    ('2025-12-27', '12:00-14:00', 90, 100),
                    ('2025-12-27', '14:00-16:00', 98, 100),
                    ('2025-12-27', '16:00-18:00', 80, 100),
                    ('2025-12-27', '18:00-20:00', 45, 100)
                ", connection);
                insertWaterpark.ExecuteNonQuery();

                // Тарифы
                var insertTariffs = new SqliteCommand(@"
                    INSERT INTO Tariffs (Name, Description, Price, Duration, Category) 
                    VALUES 
                    ('Стандарт', 'Вход в аквапарк на 2 часа', 1500.00, '2 часа', 'waterpark'),
                    ('Премиум', 'Вход в аквапарк на 4 часа + напиток', 2500.00, '4 часа', 'waterpark'),
                    ('Детский', 'Вход для детей до 12 лет на 2 часа', 800.00, '2 часа', 'waterpark'),
                    ('Детский с обедом', 'Вход для детей + детское меню', 1200.00, '2 часа', 'waterpark'),
                    ('VIP', 'Все включено + отдельная кабинка', 5000.00, '6 часов', 'waterpark'),
                    ('Семейный', '2 взрослых + 2 ребенка', 4500.00, '4 часа', 'waterpark'),
                    ('Групповой', 'На компанию от 10 человек', 12000.00, '4 часа', 'waterpark'),
                    ('Бизнес-ланч', 'Комплексный обед с напитком', 650.00, '1 час', 'restaurant'),
                    ('Бизнес-ланч Премиум', 'Расширенное меню + десерт', 950.00, '1.5 часа', 'restaurant'),
                    ('Романтический ужин', 'Ужин на двоих с вином', 3500.00, '2 часа', 'restaurant'),
                    ('Семейный ужин', 'Ужин для семьи из 4 человек', 5000.00, '2 часа', 'restaurant'),
                    ('Детский праздник', 'Организация дня рождения', 8000.00, '3 часа', 'restaurant'),
                    ('Общий абонемент', 'Посещение всех зон комплекса', 5000.00, '1 день', 'general'),
                    ('Абонемент на месяц', 'Неограниченное посещение', 15000.00, '30 дней', 'general'),
                    ('Корпоративный', 'Для сотрудников компаний', 8000.00, '1 день', 'general'),
                    ('Сезонный пропуск', 'На весь летний сезон', 25000.00, '90 дней', 'general')
                ", connection);
                insertTariffs.ExecuteNonQuery();

        // Методы для билетов
        public List<Ticket> GetTicketsByDate(DateTime date, string type = "online")
        {
            var tickets = new List<Ticket>();

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, EventName, Date, TotalTickets, SoldTickets, Price, Type
                FROM Tickets 
                WHERE Date = $date AND Type = $type AND (TotalTickets - SoldTickets) > 0
            ";
            command.Parameters.AddWithValue("$date", date.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("$type", type);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                tickets.Add(new Ticket
                {
                    Id = reader.GetInt32(0),
                    EventName = reader.GetString(1),
                    Date = DateTime.Parse(reader.GetString(2)),
                    TotalTickets = reader.GetInt32(3),
                    SoldTickets = reader.GetInt32(4),
                    Price = reader.GetDecimal(5),
                    Type = reader.GetString(6)
                });
            }

            return tickets;
        }

        // Методы для ресторана
        public List<RestaurantReservation> GetRestaurantAvailability(DateTime date)
        {
            var reservations = new List<RestaurantReservation>();

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, Date, TimeSlot, AvailableTables, TotalTables
                FROM RestaurantReservations 
                WHERE Date = $date AND AvailableTables > 0
                ORDER BY TimeSlot
            ";
            command.Parameters.AddWithValue("$date", date.ToString("yyyy-MM-dd"));

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                reservations.Add(new RestaurantReservation
                {
                    Id = reader.GetInt32(0),
                    Date = DateTime.Parse(reader.GetString(1)),
                    TimeSlot = reader.GetString(2),
                    AvailableTables = reader.GetInt32(3),
                    TotalTables = reader.GetInt32(4)
                });
            }

            return reservations;
        }

        // Методы для аквапарка
        public List<WaterparkOccupancy> GetWaterparkOccupancy(DateTime date)
        {
            var occupancy = new List<WaterparkOccupancy>();

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, Date, TimeSlot, CurrentVisitors, MaxCapacity
                FROM WaterparkOccupancy 
                WHERE Date = $date
                ORDER BY TimeSlot
            ";
            command.Parameters.AddWithValue("$date", date.ToString("yyyy-MM-dd"));

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                occupancy.Add(new WaterparkOccupancy
                {
                    Id = reader.GetInt32(0),
                    Date = DateTime.Parse(reader.GetString(1)),
                    TimeSlot = reader.GetString(2),
                    CurrentVisitors = reader.GetInt32(3),
                    MaxCapacity = reader.GetInt32(4)
                });
            }

            return occupancy;
        }

        // Методы для тарифов
        public List<Tariff> GetTariffs(string category = "")
        {
            var tariffs = new List<Tariff>();

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();

            if (string.IsNullOrEmpty(category))
            {
                command.CommandText = "SELECT Id, Name, Description, Price, Duration, Category FROM Tariffs ORDER BY Category, Price";
            }
            else
            {
                command.CommandText = "SELECT Id, Name, Description, Price, Duration, Category FROM Tariffs WHERE Category = $category ORDER BY Price";
                command.Parameters.AddWithValue("$category", category);
            }

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                tariffs.Add(new Tariff
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Description = reader.GetString(2),
                    Price = reader.GetDecimal(3),
                    Duration = reader.GetString(4),
                    Category = reader.GetString(5)
                });
            }

            return tariffs;
        }
    }
}
