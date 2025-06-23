using TrashBoard.Models;

namespace TrashBoard.Services
{
    public interface IApiTrashDataService
    {
        Task<List<TrashDetection>> GetAllAsync();
        Task<List<TrashDetection>> GetSinceAsync(DateTime since);
    }
}
