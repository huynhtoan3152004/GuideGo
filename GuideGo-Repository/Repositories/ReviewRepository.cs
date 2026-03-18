using GuideGo_Repository.Data;
using GuideGo_Repository.Entities;
using GuideGo_Repository.Enums;
using GuideGo_Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GuideGo_Repository.Repositories;

public class ReviewRepository : GenericRepository<Review>, IReviewRepository
{
    public ReviewRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Review>> GetByUserIdAsync(Guid userId)
    {
        return await _dbSet
            .Where(review => review.UserId == userId)
            .OrderByDescending(review => review.CreatedAt)
            .ToListAsync();
    }

    public async Task<Review?> GetByIdAndUserIdAsync(Guid id, Guid userId)
    {
        return await _dbSet.FirstOrDefaultAsync(review => review.Id == id && review.UserId == userId);
    }

    public async Task<bool> TourExistsWithGuideAsync(Guid tourId)
    {
        return await _context.Tours.AnyAsync(tour => tour.Id == tourId && tour.GuideId != null);
    }

    public async Task RecalculateGuideAverageRatingByTourIdAsync(Guid tourId)
    {
        var guideId = await _context.Tours
            .Where(tour => tour.Id == tourId)
            .Select(tour => tour.GuideId)
            .FirstOrDefaultAsync();

        if (guideId is null)
        {
            return;
        }

        var averageRating = await (from review in _context.Reviews
                                   join tour in _context.Tours on review.TourId equals tour.Id
                                   where tour.GuideId == guideId
                                   select (double?)review.Rating)
            .AverageAsync();

        var guide = await _context.Guides.FirstOrDefaultAsync(entity => entity.Id == guideId.Value);
        if (guide is null)
        {
            return;
        }

        guide.Rating = averageRating.HasValue
            ? Math.Round((decimal)averageRating.Value, 1, MidpointRounding.AwayFromZero)
            : 0;
    }

    public async Task<bool> UserHasCompletedBookingForTourAsync(Guid userId, Guid tourId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return await (from booking in _context.Bookings
                      join schedule in _context.TourSchedules on booking.ScheduleId equals schedule.Id
                      where booking.UserId == userId
                            && schedule.TourId == tourId
                            && booking.Status == BookingStatus.Confirmed
                            && schedule.EndDate < today
                      select booking)
            .AnyAsync();
    }

    public async Task<bool> UserAlreadyReviewedTourAsync(Guid userId, Guid tourId)
    {
        return await _dbSet.AnyAsync(review => review.UserId == userId && review.TourId == tourId);
    }
}
