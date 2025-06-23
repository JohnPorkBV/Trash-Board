using TrashBoard.Models;

namespace TrashBoard.Services
{
    public interface IApiTrashDataService
    {
        Task<List<TrashDetectionApiModel>> GetAllAsync();
        Task<List<TrashDetectionApiModel>> GetSinceAsync(DateTime since);
    }
}
