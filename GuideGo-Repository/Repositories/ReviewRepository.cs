using GuideGo_Repository.Data;
using GuideGo_Repository.Entities;
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
}
