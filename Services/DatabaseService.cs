using Microsoft.Data.Sqlite;
using AquaParser.Models; // заменить "AquaParser" на название своего проекта

namespace AquaParser.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public string ConnectionString => _connectionString;

        public DatabaseService(string connectionString)
        {
            _connectionString = connectionString;
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
        }

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
                WHERE Date = $date AND Type = $type
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
                WHERE Date = $date
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

      
        public async Task UpdateFromApiAsync()
        {
            using var client = new HttpClient();

            try
            {
                // Получаем данные с API
                var response = await client.GetStringAsync("https://apigateway.nordciti.ru/v1/aqua/CurrentLoad");
                Console.WriteLine($"✅ Данные получены с API: {response}");

                // Парсим JSON и обновляем БД
                await ParseAndSaveApiData(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка API: {ex.Message}");
            }
        }

        private async Task ParseAndSaveApiData(string jsonData)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            // Очищаем старые данные
            var clearCmd = connection.CreateCommand();
            clearCmd.CommandText = "DELETE FROM WaterparkOccupancy WHERE Date = date('now')";
            await clearCmd.ExecuteNonQueryAsync();

            // TODO: Парсим JSON и сохраняем в БД
            var insertCmd = connection.CreateCommand();
            insertCmd.CommandText = @"
        INSERT INTO WaterparkOccupancy (Date, TimeSlot, CurrentVisitors, MaxCapacity)
        VALUES (date('now'), '10:00-12:00', 50, 100)
    ";
            await insertCmd.ExecuteNonQueryAsync();
        }
      

    } 
}
