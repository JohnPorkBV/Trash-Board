using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TrashBoard.Data;
using TrashBoard.Models;

namespace TrashBoard.Services
{
    public class BredaEventService : IBredaEventService
    {
        private readonly TrashboardDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly Dictionary<int, List<BredaEvent>> _holidayCache = new();
        private const string CountryCode = "NL";

        public BredaEventService(TrashboardDbContext context, HttpClient httpClient)
        {
            _context = context;
            _httpClient = httpClient;
        }

        public async Task<BredaEvent?> HasBredaEventAsync(DateTime date)
        {
            return await _context.BredaEvents
            .FirstOrDefaultAsync(e =>
                e.StartDate.Date <= date.Date &&
                (e.EndDate == null || e.EndDate.Value.Date >= date.Date));
        }



        public async Task<List<BredaEvent>> GetBredaEventsAsync(int year)
        {
            return await _context.BredaEvents
                .Where(e =>
                    e.StartDate.Year == year ||                      // Event starts in the year
                    (e.EndDate.HasValue && e.EndDate.Value.Year == year) ||  // Event ends in the year
                    (e.StartDate.Year < year && e.EndDate.HasValue && e.EndDate.Value.Year > year) // Spans across years
                )
                .OrderBy(e => e.StartDate)
                .ToListAsync();
        }

    }
}
