using TrashBoard.Models;

namespace TrashBoard.Services
{
    public interface IHolidayService
    {
        Task<HolidayData?> IsHolidayAsync(DateTime date);
        Task<List<HolidayData>> GetHolidaysForYearAsync(int year);
    }
}
