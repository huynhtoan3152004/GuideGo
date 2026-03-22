using GuideGo_Repository.Entities;

namespace GuideGo_Repository.Interfaces;

public interface ITourScheduleRepository
{
    Task<TourSchedule?> GetByIdAsync(Guid id);
    Task<IEnumerable<TourSchedule>> GetByTourIdAsync(Guid tourId);
    Task<TourSchedule> AddAsync(TourSchedule schedule);
    Task UpdateAsync(TourSchedule schedule);
    Task DeleteAsync(TourSchedule schedule);
    Task<bool> ExistsActiveTourAsync(Guid tourId);
    Task<bool> IsTourOwnedByGuideAsync(Guid tourId, Guid guideId);
    Task<Guid?> GetGuideIdByUserIdAsync(Guid userId);
    Task<bool> HasActiveBookingsAsync(Guid scheduleId);
}
