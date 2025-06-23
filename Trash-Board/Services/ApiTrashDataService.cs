using System.Globalization;
using System.Net.Http.Json;
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
        //public async Task<IEnumerable<TrashDetection>> GetAllAsync()
        //{
        //    return await _httpClient.GetFromJsonAsync<IEnumerable<TrashDetection>>("api/trashdetections")
        //           ?? Enumerable.Empty<TrashDetection>();
        //}
        public async Task<List<TrashDetectionApiModel>> GetAllAsync()
        {
            var response = await _httpClient.GetAsync("api/trashdetect");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<TrashDetectionApiModel>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new();
        }

        public async Task<List<TrashDetectionApiModel>> GetSinceAsync(DateTime since)
        {
            var dateOnly = since.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var url = $"api/trashdetect/date?date={dateOnly}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<List<TrashDetectionApiModel>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result?.Where(x => x.Timestamp > since).ToList() ?? new();
        }
    }
}