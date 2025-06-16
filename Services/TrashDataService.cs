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
        private readonly IBredaEventService _bredaEventService;

        public TrashDataService(
            TrashboardDbContext context,
            IHolidayService holidayService,
            IBredaEventService bredaEventService)
        {
            _context = context;
            _holidayService = holidayService;
            _bredaEventService = bredaEventService;
        }



        public async Task<IEnumerable<TrashDetection>> GetAllAsync()
        {
            return await _context.TrashDetections.ToListAsync();
        }

        public async Task<IEnumerable<TrashDetection>> GetFilteredAsync(
            DateTime? from,
            DateTime? to,
            List<string>? trashTypes,
            bool? isHoliday,
            bool? isBredaEvent)
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

            if (isBredaEvent.HasValue)
                query = query.Where(t => t.IsBredaEvent == isBredaEvent.Value);

            query = query.OrderByDescending(t => t.Timestamp);

            return await query.ToListAsync();
        }

        public async Task<TrashDetection?> GetByIdAsync(int id)
        {
            return await _context.TrashDetections.FindAsync(id);
        }

        public async Task AddAsync(TrashDetection detection)
        {
            var holiday= await _holidayService.IsHolidayAsync(detection.Timestamp);
            if (holiday != null)
            {
                detection.IsHoliday = true;
                detection.HolidayName = holiday.LocalName;
            }
            var bredaEvent = await _bredaEventService.HasBredaEventAsync(detection.Timestamp);
            if (bredaEvent != null)
            {
                detection.IsBredaEvent = true;
                detection.BredaEventName = bredaEvent.Name;
            }

            _context.TrashDetections.Add(detection);
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
            var holiday = await _holidayService.IsHolidayAsync(detection.Timestamp);

            if (holiday != null)
            {
                detection.IsHoliday = true;
                detection.HolidayName = holiday.LocalName;
            }
            else
            {
                detection.IsHoliday = false;
                detection.HolidayName = null;
            }

            _context.TrashDetections.Update(detection);
            await _context.SaveChangesAsync();

            return detection;
        }
        public async Task<TrashDetection> UpdateBredaEventInfoForAsync(TrashDetection detection)
        {
            var bredaEvent = await _bredaEventService.HasBredaEventAsync(detection.Timestamp);

            if (bredaEvent != null)
            {
                detection.IsBredaEvent = true;
                detection.BredaEventName = bredaEvent.Name;
            }
            else
            {
                detection.IsBredaEvent = false;
                detection.BredaEventName = null;
            }

            _context.TrashDetections.Update(detection);
            await _context.SaveChangesAsync();

            return detection;
        }



    }
}
