namespace AquaParser.Models   // заменить "AquaParser" на название своего проекта
{
    public class Ticket
    {
        public int Id { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int TotalTickets { get; set; }
        public int SoldTickets { get; set; }
        public decimal Price { get; set; }
        public string Type { get; set; } = string.Empty;

        public int AvailableTickets => TotalTickets - SoldTickets;
    }
}
