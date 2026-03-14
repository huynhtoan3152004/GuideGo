using GuideGo_Repository.Entities;

namespace GuideGo_Repository.Interfaces;

public interface ITourRepository
{
    Task<IEnumerable<Tour>> GetAllActiveToursAsync();
    Task<Tour?> GetTourByIdAsync(Guid id);
    Task<Tour> AddTourAsync(Tour tour);
    Task UpdateTourAsync(Tour tour);
    Task<bool> ExistsLocationAsync(Guid locationId);
    Task<bool> ExistsGuideAsync(Guid guideId);
    Task<Guid?> GetGuideIdByUserIdAsync(Guid userId);
    Task<bool> IsTourOwnedByGuideAsync(Guid tourId, Guid guideId);
    Task<bool> HasActiveBookingsAsync(Guid tourId);
    Task<int> SaveChangesAsync();
}