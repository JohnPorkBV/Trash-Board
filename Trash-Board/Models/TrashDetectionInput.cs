using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TrashBoard.Models
{

    public class TrashDetectionInput
    {
        [Required]
        [JsonPropertyName("Humidity")]
        public float Humidity { get; set; } = 55;

        [Required]
        [JsonPropertyName("ConfidenceScore")]
        public float ConfidenceScore { get; set; } = 0.95f;

        [Required]
        [JsonPropertyName("Hour")]
        public int Hour { get; set; } = 14;

        [Required]
        [JsonPropertyName("Precipitation")]
        public float Precipitation { get; set; } = 0;

        [Required]
        [JsonPropertyName("Temp")]
        public float Temp { get; set; } = 22;

        [Required]
        [JsonPropertyName("Windforce")]
        public float Windforce { get; set; } = 5;

        [JsonPropertyName("IsHoliday")]
        public bool IsHoliday { get; set; } = false;
    }



}
