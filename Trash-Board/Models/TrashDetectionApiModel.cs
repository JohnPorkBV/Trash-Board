namespace TrashBoard.Models
{
    public class TrashDetectionApiModel
    {
        public int Id { get; set; }
        public string Label { get; set; } = default!;
        public double Confidence { get; set; }
        public DateTime Timestamp { get; set; }
        public string CameraId { get; set; } = default!;
        public string Location { get; set; } = default!;
        public double Temperature { get; set; }
        public float Humidity { get; set; }
        public double Wind { get; set; }
    }

}
