using TrashBoard.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TrashBoard.Services
{
    public interface ITrashDataService
    {
        Task<IEnumerable<TrashDetection>> GetAllAsync();

        Task<IEnumerable<TrashDetection>> GetFilteredAsync(
            DateTime? from,
            DateTime? to,
            List<string>? trashTypes,
            bool? isHoliday, bool? isBredaEvent);

        Task<TrashDetection?> GetByIdAsync(int id);

        Task AddAsync(TrashDetection detection);
        Task<IEnumerable<string>> GetAvailableTrashTypesAsync();

        Task<TrashDetection> UpdateHolidayInfoForAsync(TrashDetection detection);
        Task<TrashDetection> UpdateBredaEventInfoForAsync(TrashDetection detection);
    }
}
