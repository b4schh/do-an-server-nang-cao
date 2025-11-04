using FootballField.API.Entities;

namespace FootballField.API.Repositories.Interfaces
{
    public interface IBookingRepository : IGenericRepository<Booking>
    {
        Task<HashSet<(int FieldId, int TimeSlotId)>> GetBookedTimeSlotIdsForComplexAsync(int complexId, DateTime date);
    }
}
