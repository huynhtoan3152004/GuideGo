using GuideGo_Repository.Entities;

namespace GuideGo_Repository.Interfaces;

public interface ITourRepository
{
    Task<IEnumerable<Tour>> GetAllActiveToursAsync();
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
    Task<Tour> AddTourAsync(Tour tour);
    Task UpdateTourAsync(Tour tour);
    Task<bool> ExistsLocationAsync(Guid locationId);
    Task<bool> ExistsGuideAsync(Guid guideId);
    Task<bool> ExistsActiveTourAsync(Guid tourId);
    Task<Guid?> GetGuideIdByUserIdAsync(Guid userId);
    Task<bool> IsTourOwnedByGuideAsync(Guid tourId, Guid guideId);
    Task<bool> HasActiveBookingsAsync(Guid tourId);
    Task AddTourImageAsync(TourImage tourImage);
    Task<int> SaveChangesAsync();
}