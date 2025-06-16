namespace TrashBoard.Models
{
    public class HolidayData
    {
        public DateTime Date { get; set; }
        public string Name { get; set; } = string.Empty;
        public string LocalName { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
        public bool Fixed { get; set; }
        public bool Global { get; set; }
        public List<string> Counties { get; set; } = new();
        public int? LaunchYear { get; set; }
        public List<string> Types { get; set; } = new();
    }
}
