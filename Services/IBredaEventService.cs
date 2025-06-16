using TrashBoard.Models;

namespace TrashBoard.Services
{
    public interface IBredaEventService
    {
        Task<BredaEvent?> HasBredaEventAsync(DateTime date);
        Task<List<BredaEvent>> GetBredaEventsAsync(int year);

    }
}