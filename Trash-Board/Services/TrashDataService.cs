using Microsoft.EntityFrameworkCore;
using TrashBoard.Data;
using TrashBoard.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TrashBoard.Services
{
    public class TrashDataService : ITrashDataService
    {
        private readonly IDbContextFactory<TrashboardDbContext> _contextFactory;
        private readonly IHolidayService _holidayService;
        private readonly IBredaEventService _bredaEventService;
        private readonly IWeatherService _weatherService;

        public TrashDataService(
            IDbContextFactory<TrashboardDbContext> contextFactory,
            IHolidayService holidayService,
            IBredaEventService bredaEventService,
            IWeatherService weatherService)
        {
            _contextFactory = contextFactory;
            _holidayService = holidayService;
            _bredaEventService = bredaEventService;
            _weatherService = weatherService;
        }

        public async Task<int> GetCount()
        {
            await using var context = _contextFactory.CreateDbContext();
            return  context.TrashDetections
            .Count();

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
        public async IAsyncEnumerable<string> UpdateAllHolidayWithProgressAsync()
        {
            await using var context = _contextFactory.CreateDbContext();

            var allDetections = await context.TrashDetections.ToListAsync();
            if (allDetections.Count == 0)
            {
                yield return "No trash detections found.";
                yield break;
            }

            yield return $"Updating {allDetections.Count} items...";

            var years = allDetections
                .Select(d => d.Timestamp.Year)
                .Distinct()
                .OrderBy(y => y)
                .ToList();

            var holidayLookup = new Dictionary<DateTime, string>();

            foreach (var year in years)
            {
                yield return $"Fetching holiday data for {year}...";
                var holidays = await _holidayService.GetHolidaysForYearAsync(year);

                foreach (var holiday in holidays)
                {
                    var date = holiday.Date.Date;
                    if (!holidayLookup.ContainsKey(date))
                        holidayLookup[date] = holiday.LocalName;
                }
            }

            int updated = 0;
            for (int i = 0; i < allDetections.Count; i++)
            {
                var detection = allDetections[i];
                var detectionDate = detection.Timestamp.Date;

                if (holidayLookup.TryGetValue(detectionDate, out var name))
                {
                    detection.IsHoliday = true;
                    detection.HolidayName = name;
                }
                else
                {
                    detection.IsHoliday = false;
                    detection.HolidayName = null;
                }

                context.Entry(detection).State = EntityState.Modified;
                updated++;

                if (updated % 25 == 0 || updated == allDetections.Count)
                {
                    yield return $"Updated {updated}/{allDetections.Count} items...";
                }
            }

            int changes = context.ChangeTracker.Entries()
               .Count(e => e.State == EntityState.Modified);
            yield return $"Saving {changes} modified entries...";

            await context.SaveChangesAsync();

            yield return "All Holiday data updated!";
        }

        public async IAsyncEnumerable<string> UpdateAllBredaEventWithProgressAsync()
        {
            await using var context = _contextFactory.CreateDbContext();

            var allDetections = await context.TrashDetections.ToListAsync();
            if (allDetections.Count == 0)
            {
                yield return "No trash detections found.";
                yield break;
            }

            yield return $"Updating {allDetections.Count} items...";

            // Gather distinct years from detections
            var years = allDetections
                .Select(d => d.Timestamp.Year)
                .Distinct()
                .OrderBy(y => y)
                .ToList();

            // Fetch all Breda events per year
            var allEvents = new List<BredaEvent>();
            foreach (var year in years)
            {
                yield return $"Fetching Breda events for {year}...";
                var events = await _bredaEventService.GetBredaEventsAsync(year);
                allEvents.AddRange(events);
            }

            int updated = 0;
            foreach (var detection in allDetections)
            {
                var detectionDate = detection.Timestamp.Date;

                var matchingEvent = allEvents.FirstOrDefault(e =>
                    e.StartDate <= detectionDate && detectionDate <= e.EndDate);

                if (matchingEvent != null)
                {
                    detection.IsBredaEvent = true;
                    detection.BredaEventName = matchingEvent.Name;
                }
                else
                {
                    detection.IsBredaEvent = false;
                    detection.BredaEventName = null;
                }

                context.Entry(detection).State = EntityState.Modified;

                updated++;
                if (updated % 25 == 0 || updated == allDetections.Count)
                {
                    yield return $"Updated {updated}/{allDetections.Count} items...";
                }
            }

            int changes = context.ChangeTracker.Entries()
                .Count(e => e.State == EntityState.Modified);
            yield return $"Saving {changes} modified entries...";

            await context.SaveChangesAsync();

            yield return "All Breda event data updated!";
        }

        
        public async IAsyncEnumerable<string> UpdateAllWeatherInfoWithProgressAsync()
        {
            await using var context = _contextFactory.CreateDbContext();

            var allDetections = await context.TrashDetections.ToListAsync();
            if (allDetections.Count == 0)
            {
                yield return "No trash detections found.";
                yield break;
            }
            yield return $"Updating {allDetections.Count} items";

            var earliest = allDetections.Min(t => t.Timestamp);
            var latest = allDetections.Max(t => t.Timestamp);

            yield return $"Fetching weather data from {earliest} to {latest}...";
            var weatherDataByHour = await _weatherService.GetWeatherDataForRangeAsync(earliest, latest);

            int updated = 0;
            for (int i = 0; i < allDetections.Count; i++)
            {
                var detection = allDetections[i];
                var isoHour = detection.Timestamp.ToString("yyyy-MM-ddTHH:00");

                if (weatherDataByHour.TryGetValue(isoHour, out var weather))
                {
                    detection.Temp = weather.Temp;
                    detection.Precipitation = weather.Precipitation;
                    detection.Windforce = weather.Windforce;
                    detection.Humidity = weather.Humidity;

                    context.Entry(detection).State = EntityState.Modified;
                }


                updated++;

                // Yield progress every 25 updates or at end
                if (updated % 25 == 0 || i == allDetections.Count - 1)
                {
                    yield return $"Updated {updated}/{allDetections.Count} items...";
                }
            }
            int count = context.ChangeTracker.Entries()
               .Count(e => e.State == EntityState.Modified);
            yield return $"Saving {count} modified entries...";

            await context.SaveChangesAsync();

            yield return "All weather data updated!";
        }
        public async Task<int> ResetDetectionDataAsync()
        {
            await using var context = _contextFactory.CreateDbContext();

            var detections = await context.TrashDetections.ToListAsync();

            foreach (var detection in detections)
            {
                // Only reset fields other than DetectedObject and Timestamp
                detection.ConfidenceScore = "0";

                detection.Date = default;
                detection.Hour = 0;

                detection.Temp = 0;
                detection.Humidity = 0;
                detection.Precipitation = 0;
                detection.Windforce = 0;

                detection.IsHoliday = false;
                detection.HolidayName = null;

                detection.IsBredaEvent = false;
                detection.BredaEventName = null;

                context.Entry(detection).State = EntityState.Modified;
            }

            var changes = await context.SaveChangesAsync();
            return changes;
        }


    }
}
