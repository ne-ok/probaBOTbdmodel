namespace AquaParser.Models // заменить "AquaParser" на название своего проекта
{
	public class Tariff
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public decimal Price { get; set; }
		public string Duration { get; set; } = string.Empty;
		public string Category { get; set; } = string.Empty; // "waterpark", "restaurant", "general"
	}
}
