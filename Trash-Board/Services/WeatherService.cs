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
        public async Task<Dictionary<string, WeatherData>> GetWeatherDataForRangeAsync(DateTime start, DateTime end)
        {
            var startDate = start.Date.ToString("yyyy-MM-dd");
            var endDate = end.Date.ToString("yyyy-MM-dd");

            var forecastUrl = $"https://api.open-meteo.com/v1/forecast?latitude={Latitude}&longitude={Longitude}" +
                              $"&hourly=temperature_2m,precipitation,wind_speed_10m,relative_humidity_2m" +
                              $"&timezone=Europe/Amsterdam";

            var archiveUrl = $"https://archive-api.open-meteo.com/v1/archive?latitude={Latitude}&longitude={Longitude}" +
                             $"&start_date={startDate}&end_date={endDate}" +
                             $"&hourly=temperature_2m,precipitation,wind_speed_10m,relative_humidity_2m" +
                             $"&timezone=Europe/Amsterdam";

            // First try prediction data (forecast)
            var forecastData = await GetWeatherMapFromUrl(forecastUrl);

            // Then get archive data (fallback)
            var archiveData = await GetWeatherMapFromUrl(archiveUrl);

            // Merge: forecast has priority, fill gaps from archive
            foreach (var (hour, data) in archiveData)
            {
                if (!forecastData.ContainsKey(hour))
                {
                    forecastData[hour] = data;
                }
            }

            return forecastData;
        }


        
        private async Task<Dictionary<string, WeatherData>> GetWeatherMapFromUrl(string url)
        {
            var weatherByHour = new Dictionary<string, WeatherData>();

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return weatherByHour;

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("hourly", out var hourly))
                return weatherByHour;

            var times = hourly.GetProperty("time").EnumerateArray().ToList();
            var temps = hourly.GetProperty("temperature_2m").EnumerateArray().ToList();
            var precs = hourly.GetProperty("precipitation").EnumerateArray().ToList();
            var winds = hourly.GetProperty("wind_speed_10m").EnumerateArray().ToList();
            var hums = hourly.GetProperty("relative_humidity_2m").EnumerateArray().ToList();

            for (int i = 0; i < times.Count; i++)
            {
                var isoHour = times[i].GetString();
                if (isoHour == null || i >= temps.Count || i >= precs.Count || i >= winds.Count || i >= hums.Count)
                    continue;

                if (temps[i].ValueKind != JsonValueKind.Number ||
                    precs[i].ValueKind != JsonValueKind.Number ||
                    winds[i].ValueKind != JsonValueKind.Number ||
                    hums[i].ValueKind != JsonValueKind.Number)
                    continue;

                weatherByHour[isoHour] = new WeatherData
                {
                    Timestamp = DateTime.Parse(isoHour),
                    Temp = temps[i].GetSingle(),
                    Precipitation = precs[i].GetSingle(),
                    Windforce = winds[i].GetSingle(),
                    Humidity = hums[i].GetInt32()
                };
            }

            return weatherByHour;
        }

       
    }

}
