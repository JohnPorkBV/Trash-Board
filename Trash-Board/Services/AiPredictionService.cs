using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using TrashBoard.Models;
using static System.Net.WebRequestMethods;

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
            var response = await _http.PostAsJsonAsync("/predict", input);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error {response.StatusCode}: {error}");
                return null;
            }

            var json = await response.Content.ReadFromJsonAsync<PredictionResult>();
            return json?.Prediction;
        }
        public async Task<TrainingResult?> RetrainModelAsync(TrainingParameters parameters)
        {
            var response = await _http.PostAsJsonAsync("/train", parameters);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            var result = new TrainingResult
            {
                Message = json.GetProperty("message").GetString() ?? "",
                Accuracy = json.GetProperty("accuracy").GetDouble()
            };

            var reportElement = json.GetProperty("report");
            foreach (var property in reportElement.EnumerateObject())
            {
                // Only process class reports (they are objects with "precision", "recall", etc.)
                if (property.Value.ValueKind != JsonValueKind.Object) continue;

                if (!property.Value.TryGetProperty("precision", out var _)) continue;

                result.Report[property.Name] = new ClassificationMetrics
                {
                    Precision = property.Value.GetProperty("precision").GetDouble(),
                    Recall = property.Value.GetProperty("recall").GetDouble(),
                    F1Score = property.Value.GetProperty("f1-score").GetDouble(),
                    Support = property.Value.GetProperty("support").GetDouble()
                };
            }


            return result;
        }

        public async Task<Stream?> GetDecisionTreePngAsync()
        {
            var response = await _http.GetAsync("/decision-tree");
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error fetching PNG: {response.StatusCode} - {error}");
                return null;
            }

            return await response.Content.ReadAsStreamAsync();
        }

    }

    public class TrainingResult
    {
        public string Message { get; set; } = "";
        public double Accuracy { get; set; }
        public Dictionary<string, ClassificationMetrics> Report { get; set; } = new();
    }

    public class ClassificationMetrics
    {
        public double Precision { get; set; }
        public double Recall { get; set; }
        public double F1Score { get; set; }
        public double Support { get; set; }
    }

    public class PredictionResult
    {
        [JsonPropertyName("Prediction")]
        public string? Prediction { get; set; }
    }

    public class TrainingParameters
    {
        [JsonPropertyName("n_estimators")]
        public int NEstimators { get; set; }
        [JsonPropertyName("criterion")]
        public string Criterion { get; set; } = "gini";
        [JsonPropertyName("max_depth")]
        public int MaxDepth { get; set; }
        [JsonPropertyName("min_samples_split")]
        public int MinSamplesSplit { get; set; }
        [JsonPropertyName("min_samples_leaf")]
        public int MinSamplesLeaf { get; set; }
        [JsonPropertyName("max_features")]
        public string MaxFeatures { get; set; } = "sqrt";
    }




}
