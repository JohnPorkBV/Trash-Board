using Microsoft.EntityFrameworkCore;
using TrashBoard.Data;
using TrashBoard.Models;

namespace TrashBoard.Services
{
    public class TrashDataService : ITrashDataService
    {
        private readonly IDbContextFactory<TrashboardDbContext> _contextFactory;
        private readonly IHolidayService _holidayService;
        private readonly IBredaEventService _bredaEventService;

        public TrashDataService(
            IDbContextFactory<TrashboardDbContext> contextFactory,
            IHolidayService holidayService,
            IBredaEventService bredaEventService)
        {
            _contextFactory = contextFactory;
            _holidayService = holidayService;
            _bredaEventService = bredaEventService;
        }

        public async Task<IEnumerable<TrashDetection>> GetAllAsync()
        {
            await using var context = _contextFactory.CreateDbContext();
            return await context.TrashDetections.AsNoTracking().ToListAsync();
        }

        public async Task<IEnumerable<TrashDetection>> GetFilteredAsync(
            DateTime? from,
            DateTime? to,
            List<string>? trashTypes,
            bool? isHoliday,
            bool? isBredaEvent)
        {
            await using var context = _contextFactory.CreateDbContext();

            var query = context.TrashDetections
                .AsNoTracking()
                .AsQueryable();

            if (from.HasValue)
                query = query.Where(t => t.Timestamp >= from.Value);

            if (to.HasValue)
                query = query.Where(t => t.Timestamp <= to.Value);

            if (trashTypes is { Count: > 0 })
                query = query.Where(t => trashTypes.Contains(t.DetectedObject));

            if (isHoliday.HasValue)
                query = query.Where(t => t.IsHoliday == isHoliday.Value);

            if (isBredaEvent.HasValue)
                query = query.Where(t => t.IsBredaEvent == isBredaEvent.Value);

            return await query
                .OrderByDescending(t => t.Timestamp)
                .ToListAsync();
        }


        public async Task<TrashDetection?> GetByIdAsync(int id)
        {
            await using var context = _contextFactory.CreateDbContext();
            return await context.TrashDetections.FindAsync(id);
        }

        public async Task AddAsync(TrashDetection detection)
        {
            await using var context = _contextFactory.CreateDbContext();

            var holiday = await _holidayService.IsHolidayAsync(detection.Timestamp);
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

            context.TrashDetections.Add(detection);
            await context.SaveChangesAsync();
        }

        public async Task<IEnumerable<string>> GetAvailableTrashTypesAsync()
        {
            await using var context = _contextFactory.CreateDbContext();

            return await context.TrashDetections
                .Where(t => !string.IsNullOrEmpty(t.DetectedObject))
                .Select(t => t.DetectedObject!)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();
        }

        public async Task<TrashDetection> UpdateHolidayInfoForAsync(TrashDetection detection)
        {
            await using var context = _contextFactory.CreateDbContext();

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

            context.TrashDetections.Update(detection);
            await context.SaveChangesAsync();

            return detection;
        }

        public async Task<TrashDetection> UpdateBredaEventInfoForAsync(TrashDetection detection)
        {
            await using var context = _contextFactory.CreateDbContext();

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

            context.TrashDetections.Update(detection);
            await context.SaveChangesAsync();

            return detection;
        }
    }
}
