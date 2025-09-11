using System.Text.Json;
using TrashBoard.Models;

namespace TrashBoard.Services
{
    public class HolidayService : IHolidayService
    {
        private readonly HttpClient _httpClient;
        private readonly Dictionary<int, List<HolidayData>> _holidayCache = new();
        private const string CountryCode = "NL";

        public HolidayService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<HolidayData?> IsHolidayAsync(DateTime date)
        {
            var holidays = await GetHolidaysForYearAsync(date.Year);
            return holidays.FirstOrDefault(h => h.Date.Date == date.Date);
        }
        public async Task<List<HolidayData>> GetHolidaysForYearAsync(int year)
        {
            if (_holidayCache.TryGetValue(year, out var cached))
                return cached;

            var url = $"https://date.nager.at/api/v3/PublicHolidays/{year}/{CountryCode}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return new List<HolidayData>();

            var json = await response.Content.ReadAsStringAsync();
            var holidays = JsonSerializer.Deserialize<List<HolidayData>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (holidays != null)
                _holidayCache[year] = holidays;

            return holidays ?? new List<HolidayData>();
        }
    }
}
