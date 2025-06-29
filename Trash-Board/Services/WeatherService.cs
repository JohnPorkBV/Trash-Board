﻿using System.Text.Json;
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
                              $"&timezone={TimeZone}";

            var archiveUrl = $"https://archive-api.open-meteo.com/v1/archive?latitude={Latitude}&longitude={Longitude}" +
                             $"&start_date={startDate}&end_date={endDate}" +
                             $"&hourly=temperature_2m,precipitation,wind_speed_10m,relative_humidity_2m" +
                             $"&timezone={TimeZone}";

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


        public async Task<WeatherData?> GetWeatherForTimestampAsync(DateTime timestamp)
        {
            var date = timestamp.ToString("yyyy-MM-dd");
            var isoHour = "";
            if (timestamp.Hour == 0 && timestamp.Minute == 0)
                isoHour = timestamp.ToString("yyyy-MM-ddT12:00");
            else
                isoHour = timestamp.ToString("yyyy-MM-ddTHH:00");


            var forecastUrl = $"https://api.open-meteo.com/v1/forecast?latitude={Latitude}&longitude={Longitude}" +
                              $"&hourly=temperature_2m,precipitation,wind_speed_10m,relative_humidity_2m" +
                              $"&timezone={TimeZone}";

            var archiveUrl = $"https://archive-api.open-meteo.com/v1/archive?latitude={Latitude}&longitude={Longitude}" +
                             $"&start_date={date}&end_date={date}" +
                             $"&hourly=temperature_2m,precipitation,wind_speed_10m,relative_humidity_2m" +
                             $"&timezone={TimeZone}";

            var forecastData = await GetWeatherDataFromUrl(forecastUrl, isoHour);

            if (forecastData != null)
                return forecastData;

            return await GetWeatherDataFromUrl(archiveUrl, isoHour);
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
