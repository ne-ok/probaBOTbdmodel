namespace AquaParser.Models // заменить "AquaParser" на название своего проекта
{
    public class RestaurantReservation
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string TimeSlot { get; set; } = string.Empty;
        public int AvailableTables { get; set; }
        public int TotalTables { get; set; }

        public int OccupiedTables => TotalTables - AvailableTables;
        public double OccupancyPercent => TotalTables > 0 ? (OccupiedTables * 100.0) / TotalTables : 0;
    }
}
