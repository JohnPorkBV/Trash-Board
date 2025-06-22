using TrashBoard.Models;

namespace TrashBoard.Services
{
    public interface IApiTrashDataService
    {
        Task<int> GetCount();
        Task<IEnumerable<TrashDetection>> GetAllAsync();
        Task<IEnumerable<TrashDetection>> GetFilteredAsync(
            DateTime? from, DateTime? to, List<string>? trashTypes, bool? isHoliday, bool? isBredaEvent);
        Task<IEnumerable<string>> GetAvailableTrashTypesAsync();
    }
}
