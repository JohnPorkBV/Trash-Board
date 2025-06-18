using TrashBoard.Models;

namespace TrashBoard.Services
{
    public interface IWeatherService
    {
        Task<Dictionary<string, WeatherData>> GetWeatherDataForRangeAsync(DateTime start, DateTime end);
        //Task<WeatherData?> GetWeatherForTimestampAsync(DateTime timestamp);
    }

}