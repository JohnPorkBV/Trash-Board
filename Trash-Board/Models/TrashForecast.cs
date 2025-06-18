namespace TrashBoard.Models
{
    public class TrashForecastResult
    {
        public string date { get; set; } = "";
        public int predicted_trash_count { get; set; }
        public string predicted_object { get; set; } = "";
    }
}
