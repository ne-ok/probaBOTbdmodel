namespace AquaParser.Models // заменить "AquaParser" на название своего проекта
{
    public class WaterparkOccupancy
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string TimeSlot { get; set; } = string.Empty;
        public int CurrentVisitors { get; set; }
        public int MaxCapacity { get; set; }

        public int AvailableSpots => MaxCapacity - CurrentVisitors;
        public double OccupancyPercent => MaxCapacity > 0 ? (CurrentVisitors * 100.0) / MaxCapacity : 0;
        public string OccupancyLevel => OccupancyPercent switch
        {
            < 30 => "Низкая",
            < 70 => "Средняя",
            < 90 => "Высокая",
            _ => "Полная"
        };
    }
}
