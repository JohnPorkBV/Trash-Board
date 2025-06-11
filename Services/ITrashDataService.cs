using TrashBoard.Models;

namespace TrashBoard.Services
{
    public interface ITrashDataService
    {
        Task<IEnumerable<TrashDetection>> GetAllAsync();
        Task<IEnumerable<TrashDetection>> GetFilteredAsync(
            DateTime? from = null, DateTime? to = null, string? trashType = null);
        Task<TrashDetection?> GetByIdAsync(int id);
        Task AddAsync(TrashDetection detection);
        Task UpdateWeatherInfoAsync(int id, float temperature, string condition, float humidity, float precipitation, DateTime date);
        Task<TrashDetection> UpdateHolidayInfoForAsync(TrashDetection detection);


    }
}
