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
            bool? isHoliday);  // <-- nieuwe parameter

        Task<TrashDetection?> GetByIdAsync(int id);

        Task AddAsync(TrashDetection detection);

        Task UpdateWeatherInfoAsync(int id, float temperature, string condition, float humidity, float precipitation, DateTime date);

        Task<IEnumerable<string>> GetAvailableTrashTypesAsync();

        Task<TrashDetection> UpdateHolidayInfoForAsync(TrashDetection detection);
    }
}
