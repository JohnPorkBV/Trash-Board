using Trash_Board.Models;

namespace Trash_Board.Services
{
    public interface ITrashDataService
    {
        Task<IEnumerable<TrashDetection>> GetAllAsync();
        Task<IEnumerable<TrashDetection>> GetFilteredAsync(
            DateTime? from = null, DateTime? to = null, string? trashType = null);

        Task<TrashDetection?> GetByIdAsync(int id);
        Task AddAsync(TrashDetection detection);
        Task UpdateWeatherInfoAsync(int id, float temperature, string condition, float humidity, float precipitation, DateTime date);


    }
}
