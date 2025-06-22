using System.Net.Http.Json;
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

        public async Task<int> GetCount()
        {
            var detections = await GetAllAsync();
            return detections.Count();
        }

        public async Task<IEnumerable<TrashDetection>> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<TrashDetection>>("api/trashdetections")
                   ?? Enumerable.Empty<TrashDetection>();
        }

        public async Task<IEnumerable<TrashDetection>> GetFilteredAsync(
            DateTime? from, DateTime? to, List<string>? trashTypes, bool? isHoliday, bool? isBredaEvent)
        {
            var query = new List<string>();
            if (from.HasValue) query.Add($"from={from.Value:O}");
            if (to.HasValue) query.Add($"to={to.Value:O}");
            if (trashTypes != null && trashTypes.Any()) query.Add($"types={string.Join(",", trashTypes)}");
            if (isHoliday.HasValue) query.Add($"isHoliday={isHoliday.Value}");
            if (isBredaEvent.HasValue) query.Add($"isBredaEvent={isBredaEvent.Value}");

            var url = "api/trashdetections";
            if (query.Count > 0)
                url += "?" + string.Join("&", query);

            return await _httpClient.GetFromJsonAsync<IEnumerable<TrashDetection>>(url)
                   ?? Enumerable.Empty<TrashDetection>();
        }

        public async Task<IEnumerable<string>> GetAvailableTrashTypesAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<string>>("api/trashdetections/types")
                   ?? Enumerable.Empty<string>();
        }
    }
}