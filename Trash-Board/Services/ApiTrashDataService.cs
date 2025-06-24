using System.Globalization;
using System.Text.Json;
using TrashBoard.Models;

namespace TrashBoard.Services
{
    public class ApiTrashDataService : IApiTrashDataService
    {
        private readonly HttpClient _httpClient;

        public ApiTrashDataService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        private TrashDetection MapToTrashDetection(TrashDetectionApiModel apiItem)
        {
            return new TrashDetection
            {
                DetectedObject = apiItem.Label,
                ConfidenceScore = apiItem.Confidence.ToString("F2", CultureInfo.InvariantCulture),
                Timestamp = apiItem.Timestamp,
                Date = apiItem.Timestamp.Date,
                Hour = apiItem.Timestamp.Hour,
                Temp = (float)apiItem.Temperature,
                Humidity = (float)apiItem.Humidity,
                Windforce = (float)apiItem.Wind,

                // Default for enrichment later
                Precipitation = 0f,
                IsHoliday = false,
                HolidayName = null,
                IsBredaEvent = false,
                BredaEventName = null
            };
        }

        public async Task<List<TrashDetection>> GetAllAsync()
        {
            var response = await _httpClient.GetAsync("api/trashdetect");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var rawList = JsonSerializer.Deserialize<List<TrashDetectionApiModel>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new();

            return rawList.Select(MapToTrashDetection).ToList();
        }

        public async Task<List<TrashDetection>> GetSinceAsync(DateTime since)
        {
            var dateOnly = since.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var url = $"api/trashdetect/date?date={dateOnly}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var rawList = JsonSerializer.Deserialize<List<TrashDetectionApiModel>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new();

            return rawList
                .Where(x => x.Timestamp > since)
                .Select(MapToTrashDetection)
                .ToList();
        }
    }
}
