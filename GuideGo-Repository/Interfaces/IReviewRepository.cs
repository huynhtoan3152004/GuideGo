using GuideGo_Repository.Entities;

namespace GuideGo_Repository.Interfaces;

public interface IReviewRepository
{
    Task<IEnumerable<Review>> GetByUserIdAsync(Guid userId);
    Task<Review?> GetByIdAsync(Guid id);
    Task<Review?> GetByIdAndUserIdAsync(Guid id, Guid userId);
    Task<bool> TourExistsWithGuideAsync(Guid tourId);
    Task<bool> UserHasCompletedBookingForTourAsync(Guid userId, Guid tourId);
    Task<bool> UserAlreadyReviewedTourAsync(Guid userId, Guid tourId);
    Task RecalculateGuideAverageRatingByTourIdAsync(Guid tourId);
    Task AddAsync(Review review);
    void Update(Review review);
    void Remove(Review review);
    Task<int> SaveChangesAsync();
}
