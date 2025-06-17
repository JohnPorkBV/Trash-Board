using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TrashBoard.Models
{

    public class TrashDetectionInput
    {
        [Required]
        [JsonPropertyName("Humidity")]
        public float Humidity { get; set; }

        [Required]
        [JsonPropertyName("ConfidenceScore")]
        public float ConfidenceScore { get; set; }

        [Required]
        [JsonPropertyName("Hour")]
        public int Hour { get; set; }

        [Required]
        [JsonPropertyName("Precipitation")]
        public float Precipitation { get; set; }

        [Required]
        [JsonPropertyName("Temp")]
        public float Temp { get; set; }

        [Required]
        [JsonPropertyName("Windforce")]
        public float Windforce { get; set; }

        [JsonPropertyName("IsHoliday")]
        public bool IsHoliday { get; set; }
    }



}
