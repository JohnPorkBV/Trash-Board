using TrashBoard.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TrashBoard.Services
{
    public interface ITrashDataService
    {
        Task<int> GetCount();
        Task<IEnumerable<TrashDetection>> GetAllAsync();

        Task<IEnumerable<TrashDetection>> GetFilteredAsync(
            DateTime? from,
            DateTime? to,
            List<string>? trashTypes,
            bool? isHoliday, bool? isBredaEvent);

        Task<TrashDetection?> GetByIdAsync(int id);

        Task AddAsync(TrashDetection detection);
        Task<IEnumerable<string>> GetAvailableTrashTypesAsync();
        IAsyncEnumerable<string> UpdateAllHolidayWithProgressAsync();
        IAsyncEnumerable<string> UpdateAllBredaEventWithProgressAsync();
        IAsyncEnumerable<string> UpdateAllWeatherInfoWithProgressAsync();
        IAsyncEnumerable<string> ImportFromApiWithProgressAsync(IApiTrashDataService apiTrashDataService);
        Task<int> ResetDetectionDataAsync();
        Task<int> DeleteAllDetectionsAsync();
        Task ImportFromApiAsync(IApiTrashDataService apiTrashDataService);
    }
}
