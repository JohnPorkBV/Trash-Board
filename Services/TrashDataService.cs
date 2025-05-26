using Microsoft.EntityFrameworkCore;
using TrashBoard.Data;
using TrashBoard.Models;
using TrashBoard.Services;

namespace TrashBoard.Services
{
    public class TrashDataService : ITrashDataService
    {
        private readonly TrashboardDbContext _context;

        public TrashDataService(TrashboardDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TrashDetection>> GetAllAsync()
        {
            return await _context.TrashDetections.ToListAsync();
        }

        public async Task<IEnumerable<TrashDetection>> GetFilteredAsync(DateTime? from, DateTime? to, string? trashType)
        {
            var query = _context.TrashDetections.AsQueryable();

            if (from.HasValue)
                query = query.Where(t => t.Timestamp >= from.Value);

            if (to.HasValue)
                query = query.Where(t => t.Timestamp <= to.Value);

            if (!string.IsNullOrEmpty(trashType))
                query = query.Where(t => t.DetectedObject == trashType);

            return await query.ToListAsync();
        }

        public async Task<TrashDetection?> GetByIdAsync(int id)
        {
            return await _context.TrashDetections.FindAsync(id);
        }

        public async Task AddAsync(TrashDetection detection)
        {
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
    }

}
