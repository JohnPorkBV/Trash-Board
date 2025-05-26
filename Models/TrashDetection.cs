using System.ComponentModel.DataAnnotations;

namespace Trash_Board.Models
{
    public class TrashDetection
    {
        public int Id { get; set; }

        public string DetectedObject { get; set; } = string.Empty;
        public float ConfidenceScore { get; set; }

        public DateTime Timestamp { get; set; }
        public DateTime Date { get; set; }
        public int Hour { get; set; }

        public float Temp { get; set; }
        public float Humidity { get; set; }
        public float Precipitation { get; set; }
        public float Windforce { get; set; }
    }


}
