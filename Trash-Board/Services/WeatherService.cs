using System.Text.Json;
using TrashBoard.Models;

namespace TrashBoard.Services
{
    public class WeatherService : IWeatherService
    {
        private const double Latitude = 51.5719;
        private const double Longitude = 4.7683;
        private const string TimeZone = "Europe%2FAmsterdam";
        private readonly HttpClient _httpClient;

        public WeatherService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<WeatherData?> GetWeatherForTimestampAsync(DateTime timestamp)
        {
            var date = timestamp.ToString("yyyy-MM-dd");
            var isoHour = timestamp.ToString("yyyy-MM-ddTHH:00");

            var archiveUrl = $"https://archive-api.open-meteo.com/v1/archive?latitude=51.5719&longitude=4.7683" +
                             $"&start_date={date}&end_date={date}" +
                             $"&hourly=temperature_2m,precipitation,wind_speed_10m,relative_humidity_2m&timezone=Europe/Amsterdam";

            var forecastUrl = $"https://api.open-meteo.com/v1/forecast?latitude=51.5719&longitude=4.7683" +
                              $"&hourly=temperature_2m,precipitation,wind_speed_10m,relative_humidity_2m&timezone=Europe/Amsterdam";

            var archiveData = await GetWeatherDataFromUrl(archiveUrl, isoHour);
            if (archiveData != null)
                return archiveData;

            // If archive is missing or doesn't contain that hour, fallback to forecast
            return await GetWeatherDataFromUrl(forecastUrl, isoHour);
        }

        private async Task<WeatherData?> GetWeatherDataFromUrl(string url, string isoHour)
        {
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("hourly", out var hourly))
                return null;

            var times = hourly.GetProperty("time").EnumerateArray().ToList();
            var temps = hourly.GetProperty("temperature_2m").EnumerateArray().ToList();
            var precs = hourly.GetProperty("precipitation").EnumerateArray().ToList();
            var winds = hourly.GetProperty("wind_speed_10m").EnumerateArray().ToList();
            var hums = hourly.GetProperty("relative_humidity_2m").EnumerateArray().ToList();

            int index = times.FindIndex(t => t.GetString() == isoHour);
            if (index == -1 || index >= temps.Count)
                return null;

            float? GetFloat(JsonElement e) => e.ValueKind == JsonValueKind.Number ? e.GetSingle() : null;
            int? GetInt(JsonElement e) => e.ValueKind == JsonValueKind.Number ? e.GetInt32() : null;

            var temperature = GetFloat(temps[index]);
            var precipitation = GetFloat(precs[index]);
            var windSpeed = GetFloat(winds[index]);
            var humidity = GetInt(hums[index]);

            if (temperature == null || precipitation == null || windSpeed == null || humidity == null)
                return null;

            return new WeatherData
            {
                Timestamp = DateTime.Parse(isoHour),
                Temp = temperature.Value,
                Precipitation = precipitation.Value,
                Windforce = windSpeed.Value,
                Humidity = humidity.Value
            };
        }

    }

}
