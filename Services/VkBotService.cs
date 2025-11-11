using VkNet;
using VkNet.Model;
using VkBotDb.Models;

namespace VkBotDb.Services
{
    public class VkBotService
    {
        private readonly VkApi _vkApi;
        private readonly DatabaseService _dbService;
        private readonly Dictionary<long, string> _userStates;

        public VkBotService(DatabaseService dbService)
        {
            _dbService = dbService;
            _vkApi = new VkApi();
            _userStates = new Dictionary<long, string>();
        }

        public void Initialize(string groupToken)
        {
            _vkApi.Authorize(new ApiAuthParams { AccessToken = groupToken });
        }

        public string ProcessMessage(string message, long userId)
        {
            message = message.ToLower().Trim();

            // Сброс состояния при команде "меню"
            if (message == "меню" || message == "start" || message == "начать")
            {
                _userStates[userId] = "menu";
                return GetMainMenu();
            }

            // Получаем текущее состояние пользователя
            if (!_userStates.ContainsKey(userId))
            {
                _userStates[userId] = "menu";
            }

            var state = _userStates[userId];

            return state switch
            {
                "menu" => HandleMainMenu(message, userId),
                "waiting_ticket_date" => HandleTicketsRequest(message, userId),
                "waiting_restaurant_date" => HandleRestaurantRequest(message, userId),
                "waiting_waterpark_date" => HandleWaterparkRequest(message, userId),
                "waiting_tariff_category" => HandleTariffsRequest(message, userId),
                _ => GetMainMenu()
            };
        }

        private string HandleMainMenu(string message, long userId)
        {
            switch (message)
            {
                case "1":
                case "билеты":
                    _userStates[userId] = "waiting_ticket_date";
                    return "🎫 **Проверка онлайн-билетов**\n\n📅 Введите дату в формате ДД.ММ.ГГГГ:\nНапример: 25.12.2024";

                case "2":
                case "ресторан":
                case "бронь ресторана":
                    _userStates[userId] = "waiting_restaurant_date";
                    return "🍽️ **Бронь столиков в ресторане**\n\n📅 Введите дату в формате ДД.ММ.ГГГГ:\nНапример: 25.12.2024";

                case "3":
                case "аквапарк":
                case "загруженность":
                    _userStates[userId] = "waiting_waterpark_date";
                    return "🏊‍♂️ **Загруженность аквапарка**\n\n📅 Введите дату в формате ДД.ММ.ГГГГ:\nНапример: 25.12.2024";

                case "4":
                case "тарифы":
                case "цены":
                    _userStates[userId] = "waiting_tariff_category";
                    return "💰 **Тарифы и цены**\n\nВыберите категорию:\n\n1️⃣ - Аквапарк\n2️⃣ - Ресторан\n3️⃣ - Общие тарифы\n4️⃣ - Все тарифы";

                case "5":
                case "помощь":
                case "help":
                    return GetHelpMessage();

                default:
                    return GetMainMenu();
            }
        }

        private string HandleTicketsRequest(string message, long userId)
        {
            var date = ExtractDateFromMessage(message);
            if (date == DateTime.MinValue)
            {
                return "❌ Неверный формат даты. Введите дату в формате ДД.ММ.ГГГГ:\nНапример: 25.12.2024";
            }

            var tickets = _dbService.GetTicketsByDate(date, "online");
            _userStates[userId] = "menu";

            if (tickets.Count == 0)
            {
                return $"❌ На {date:dd.MM.yyyy} нет доступных онлайн-билетов";
            }

            return FormatTicketsResponse(tickets, date);
        }

        private string HandleRestaurantRequest(string message, long userId)
        {
            var date = ExtractDateFromMessage(message);
            if (date == DateTime.MinValue)
            {
                return "❌ Неверный формат даты. Введите дату в формате ДД.ММ.ГГГГ:\nНапример: 25.12.2024";
            }

            var reservations = _dbService.GetRestaurantAvailability(date);
            _userStates[userId] = "menu";

            if (reservations.Count == 0)
            {
                return $"❌ На {date:dd.MM.yyyy} нет свободных столиков";
            }

            return FormatRestaurantResponse(reservations, date);
        }

        private string HandleWaterparkRequest(string message, long userId)
        {
            var date = ExtractDateFromMessage(message);
            if (date == DateTime.MinValue)
            {
                return "❌ Неверный формат даты. Введите дату в формате ДД.ММ.ГГГГ:\nНапример: 25.12.2024";
            }

            var occupancy = _dbService.GetWaterparkOccupancy(date);
            _userStates[userId] = "menu";

            if (occupancy.Count == 0)
            {
                return $"❌ На {date:dd.MM.yyyy} нет данных по аквапарку";
            }

            return FormatWaterparkResponse(occupancy, date);
        }

        private string HandleTariffsRequest(string message, long userId)
        {
            var tariffs = message switch
            {
                "1" or "аквапарк" => _dbService.GetTariffs("waterpark"),
                "2" or "ресторан" => _dbService.GetTariffs("restaurant"),
                "3" or "общие" => _dbService.GetTariffs("general"),
                "4" or "все" => _dbService.GetTariffs(),
                _ => new List<Tariff>()
            };

            _userStates[userId] = "menu";

            if (tariffs.Count == 0)
            {
                return "❌ Тарифы не найдены";
            }

            return FormatTariffsResponse(tariffs);
        }

        private string GetMainMenu()
        {
            return @"🎪 **Добро пожаловать в VkBotDb!**

Выберите опцию:

🎫 1 - Онлайн-билеты
🍽️ 2 - Бронь ресторана  
🏊‍♂️ 3 - Загруженность аквапарка
💰 4 - Тарифы и цены
❓ 5 - Помощь

Просто введите цифру или название опции!";
        }

        private string GetHelpMessage()
        {
            return @"❓ **Помощь по боту VkBotDb**

**Основные команды:**
• 'Меню' - главное меню
• '1' или 'Билеты' - проверка онлайн-билетов
• '2' или 'Ресторан' - бронь столиков
• '3' или 'Аквапарк' - загруженность аквапарка
• '4' или 'Тарифы' - цены и тарифы
• '5' или 'Помощь' - эта справка

**Формат дат:** ДД.ММ.ГГГГ (25.12.2024)";
        }

        // Форматирование ответов (реализации аналогичные предыдущим, но адаптированные под новые модели)
        private string FormatTicketsResponse(List<Ticket> tickets, DateTime date)
        {
            var response = new System.Text.StringBuilder();
            response.AppendLine($"🎫 **Доступные онлайн-билеты на {date:dd.MM.yyyy}**");
            response.AppendLine();

            foreach (var ticket in tickets)
            {
                response.AppendLine($"🎭 **{ticket.EventName}**");
                response.AppendLine($"✅ Доступно: {ticket.AvailableTickets} билетов");
                response.AppendLine($"💰 Цена: {ticket.Price} ₽");
                response.AppendLine();
            }

            response.AppendLine("💡 Для возврата в меню напишите 'меню'");
            return response.ToString();
        }

        private string FormatRestaurantResponse(List<RestaurantReservation> reservations, DateTime date)
        {
            var response = new System.Text.StringBuilder();
            response.AppendLine($"🍽️ **Свободные столики на {date:dd.MM.yyyy}**");
            response.AppendLine();

            foreach (var res in reservations)
            {
                response.AppendLine($"🕒 **{res.TimeSlot}**");
                response.AppendLine($"📊 Свободно: {res.AvailableTables} из {res.TotalTables} столиков");
                response.AppendLine($"📈 Загруженность: {res.OccupancyPercent:0}%");
                response.AppendLine();
            }

            response.AppendLine("💡 Для бронирования звоните: +7 (XXX) XXX-XX-XX");
            response.AppendLine("🔙 Для возврата в меню напишите 'меню'");

            return response.ToString();
        }

        private string FormatWaterparkResponse(List<WaterparkOccupancy> occupancy, DateTime date)
        {
            var response = new System.Text.StringBuilder();
            response.AppendLine($"🏊‍♂️ **Загруженность аквапарка на {date:dd.MM.yyyy}**");
            response.AppendLine();

            foreach (var occ in occupancy)
            {
                var emoji = occ.OccupancyLevel switch
                {
                    "Низкая" => "🟢",
                    "Средняя" => "🟡",
                    "Высокая" => "🟠",
                    "Полная" => "🔴",
                    _ => "⚪"
                };

                response.AppendLine($"🕒 **{occ.TimeSlot}** {emoji}");
                response.AppendLine($"👥 Посетителей: {occ.CurrentVisitors}/{occ.MaxCapacity}");
                response.AppendLine($"📊 Загруженность: {occ.OccupancyPercent:0}% ({occ.OccupancyLevel})");
                response.AppendLine();
            }

            response.AppendLine("💡 Лучшее время для посещения - утренние часы!");
            response.AppendLine("🔙 Для возврата в меню напишите 'меню'");

            return response.ToString();
        }

        private string FormatTariffsResponse(List<Tariff> tariffs)
        {
            var response = new System.Text.StringBuilder();
            response.AppendLine("💰 **Тарифы и цены**");
            response.AppendLine();

            string currentCategory = "";
            foreach (var tariff in tariffs)
            {
                if (tariff.Category != currentCategory)
                {
                    currentCategory = tariff.Category;
                    response.AppendLine($"\n**{GetCategoryName(currentCategory)}**");
                }

                response.AppendLine($"💎 **{tariff.Name}** - {tariff.Price} ₽");
                response.AppendLine($"⏱️ {tariff.Duration} • {tariff.Description}");
                response.AppendLine();
            }

            response.AppendLine("🔙 Для возврата в меню напишите 'меню'");
            return response.ToString();
        }

        private string GetCategoryName(string category)
        {
            return category switch
            {
                "waterpark" => "🏊‍♂️ АКВАПАРК",
                "restaurant" => "🍽️ РЕСТОРАН",
                "general" => "🎪 ОБЩИЕ ТАРИФЫ",
                _ => category
            };
        }

        private DateTime ExtractDateFromMessage(string message)
        {
            // Реализация парсинга даты (как в предыдущем примере)
            var patterns = new[] {
                "(\\d{1,2}\\.\\d{1,2}\\.\\d{4})",
                "(\\d{1,2}\\.\\d{1,2})",
                "(\\d{1,2}/\\d{1,2}/\\d{4})"
            };

            foreach (var pattern in patterns)
            {
                var match = System.Text.RegularExpressions.Regex.Match(message, pattern);
                if (match.Success)
                {
                    if (DateTime.TryParse(match.Groups[1].Value, out var date))
                    {
                        if (date.Year == DateTime.Now.Year && !match.Groups[1].Value.Contains(DateTime.Now.Year.ToString()))
                        {
                            date = new DateTime(DateTime.Now.Year, date.Month, date.Day);
                        }
                        return date;
                    }
                }
            }

            return DateTime.MinValue;
        }
    }
}