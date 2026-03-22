using GuideGo_Repository.Entities;

namespace GuideGo_Repository.Interfaces;

public interface ITourRepository
{
    Task<IEnumerable<Tour>> GetAllActiveToursAsync();
    Task<IEnumerable<Tour>> GetMyRequestedToursAsync(Guid userId);
    Task<(IEnumerable<Tour> Items, int TotalItems)> SearchActiveToursAsync(
        string? keyword,
        string? city,
        Guid? locationId,
        string? guideLanguage,
        bool verifiedGuideOnly,
        decimal? minPrice,
        decimal? maxPrice,
        DateOnly? startDate,
        DateOnly? endDate,
        string? sortBy,
        int page,
        int pageSize);
    Task<Tour?> GetTourByIdAsync(Guid id);
    Task<Tour?> GetTourForOperationsAsync(Guid id);
    Task<Tour> AddTourAsync(Tour tour);
    Task UpdateTourAsync(Tour tour);
    Task<bool> ExistsLocationAsync(Guid locationId);
    Task<bool> ExistsGuideAsync(Guid guideId);
    Task<bool> ExistsUserAsync(Guid userId);
    Task<bool> ExistsActiveTourAsync(Guid tourId);
    Task<Guid?> GetGuideIdByUserIdAsync(Guid userId);
    Task<bool> IsTourOwnedByGuideAsync(Guid tourId, Guid guideId);
    Task<bool> IsTourRequestedByUserAsync(Guid tourId, Guid userId);
    Task<bool> IsGuideAssignedToTourAsync(Guid tourId, Guid guideId);
    Task<bool> HasActiveBookingsAsync(Guid tourId);
    Task AddTourImageAsync(TourImage tourImage);
    Task<IEnumerable<Guide>> FindSuitableGuidesAsync(Guid locationId, string? language, bool verifiedOnly, int limit);
    Task<TourSchedule?> GetScheduleByIdAsync(Guid scheduleId);
    Task<TourSchedule?> GetScheduleByTourIdAsync(Guid tourId, Guid scheduleId);
    Task AddTourScheduleAsync(TourSchedule schedule);
    Task UpdateTourScheduleAsync(TourSchedule schedule);
    Task<int> SaveChangesAsync();
}