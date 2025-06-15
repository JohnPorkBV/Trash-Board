using System.Net.Http.Json;
using System.Text.Json.Serialization;
using TrashBoard.Models;

namespace TrashBoard.Services
{
    public class AiPredictionService
    {
        private readonly HttpClient _http;

        public AiPredictionService(HttpClient http)
        {
            _http = http;
        }

        public async Task<string?> PredictAsync(TrashDetectionInput input)
        {
            var response = await _http.PostAsJsonAsync("http://localhost:8000/predict", input);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error {response.StatusCode}: {error}");
                return null;
            }

            var json = await response.Content.ReadFromJsonAsync<PredictionResult>();
            return json?.Prediction;
        }
    }

    public class PredictionResult
    {
        [JsonPropertyName("Prediction")]
        public string? Prediction { get; set; }
    }


}
