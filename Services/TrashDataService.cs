using Microsoft.EntityFrameworkCore;
using TrashBoard.Data;
using TrashBoard.Models;
using TrashBoard.Services;

namespace TrashBoard.Services
{
    public class TrashDataService : ITrashDataService
    {
        private readonly TrashboardDbContext _context;
        private readonly IHolidayService _holidayService;

        public TrashDataService(TrashboardDbContext context, IHolidayService holidayService)
        {
            _context = context;
            _holidayService = holidayService;
        }


        public async Task<IEnumerable<TrashDetection>> GetAllAsync()
        {
            return await _context.TrashDetections.ToListAsync();
        }

        public async Task<IEnumerable<TrashDetection>> GetFilteredAsync(
    DateTime? from,
    DateTime? to,
    List<string>? trashTypes,
    bool? isHoliday)
        {
            var query = _context.TrashDetections.AsQueryable();

            if (from.HasValue)
                query = query.Where(t => t.Timestamp >= from.Value);

            if (to.HasValue)
                query = query.Where(t => t.Timestamp <= to.Value);

            if (trashTypes != null && trashTypes.Any())
                query = query.Where(t => trashTypes.Contains(t.DetectedObject));

            if (isHoliday.HasValue)
                query = query.Where(t => t.IsHoliday == isHoliday.Value);

            query = query.OrderByDescending(t => t.Timestamp);

            return await query.ToListAsync();
        }

        public async Task<TrashDetection?> GetByIdAsync(int id)
        {
            return await _context.TrashDetections.FindAsync(id);
        }

        public async Task AddAsync(TrashDetection detection)
        {
            // Check for holiday
            var holidays = await _holidayService.GetHolidaysForYearAsync(detection.Timestamp.Year);
            var holiday = holidays.FirstOrDefault(h => h.Date.Date == detection.Timestamp.Date);

            if (holiday != null)
            {
                detection.IsHoliday = true;
                detection.HolidayName = holiday.LocalName; // or holiday.Name if you prefer English
            }
            else
            {
                detection.IsHoliday = false;
                detection.HolidayName = null;
            }

            _context.TrashDetections.Add(detection);
            await _context.SaveChangesAsync();
        }


        public async Task UpdateWeatherInfoAsync(int id, float temperature, string condition, float humidity, float precipitation, DateTime date)
        {
            var detection = await _context.TrashDetections.FindAsync(id);
            if (detection == null) return;

            detection.Temp = temperature;
            detection.Humidity = humidity;
            detection.Precipitation = precipitation;
            detection.Date = date;

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<string>> GetAvailableTrashTypesAsync()
        {
            return await _context.TrashDetections
                .Select(td => td.DetectedObject)
                .Where(type => type != null && type != "")
                .Distinct()
                .OrderBy(type => type)
                .ToListAsync();
        }
        public async Task<TrashDetection> UpdateHolidayInfoForAsync(TrashDetection detection)
        {
            var holidays = await _holidayService.GetHolidaysForYearAsync(detection.Timestamp.Year);
            var holiday = holidays.FirstOrDefault(h => h.Date.Date == detection.Timestamp.Date);

            detection.IsHoliday = holiday != null;
            detection.HolidayName = holiday?.LocalName;

            _context.TrashDetections.Update(detection);
            await _context.SaveChangesAsync();

            return detection;
        }



    }
}
