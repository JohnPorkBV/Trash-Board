using TrashBoard.Models;

namespace TrashBoard.Services
{
    public interface IHolidayService
    {
        Task<bool> IsHolidayAsync(DateTime date);
        Task<List<HolidayData>> GetHolidaysForYearAsync(int year);
    }
}
