using TrashBoard.Models;

namespace TrashBoard.Services
{
    public interface IWeatherService
    {
        Task<WeatherData?> GetWeatherForTimestampAsync(DateTime timestamp);
    }

}