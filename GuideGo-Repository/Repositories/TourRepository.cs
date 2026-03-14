using GuideGo_Repository.Data;
using GuideGo_Repository.Entities;
using GuideGo_Repository.Enums;
using GuideGo_Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GuideGo_Repository.Repositories;

public class TourRepository : GenericRepository<Tour>, ITourRepository
{
    public TourRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Tour>> GetAllActiveToursAsync()
    {
        return await _dbSet
            .AsNoTracking()
            .Where(tour => tour.IsActive)
            .Include(tour => tour.Location)
            .Include(tour => tour.Guide)
                .ThenInclude(guide => guide!.User)
            .Include(tour => tour.Schedules)
            .OrderByDescending(tour => tour.CreatedAt)
            .ToListAsync();
    }

    public async Task<Tour?> GetTourByIdAsync(Guid id)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(tour => tour.Location)
            .Include(tour => tour.Guide)
                .ThenInclude(guide => guide!.User)
            .Include(tour => tour.Schedules)
            .FirstOrDefaultAsync(tour => tour.Id == id && tour.IsActive);
    }

    public async Task<Tour> AddTourAsync(Tour tour)
    {
        await _dbSet.AddAsync(tour);
        await _context.SaveChangesAsync();
        return tour;
    }

    public async Task UpdateTourAsync(Tour tour)
    {
        _dbSet.Update(tour);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsLocationAsync(Guid locationId)
    {
        return await _context.Locations.AnyAsync(location => location.Id == locationId);
    }

    public async Task<bool> ExistsGuideAsync(Guid guideId)
    {
        return await _context.Guides.AnyAsync(guide => guide.Id == guideId);
    }

    public async Task<Guid?> GetGuideIdByUserIdAsync(Guid userId)
    {
        return await _context.Guides
            .Where(guide => guide.UserId == userId)
            .Select(guide => (Guid?)guide.Id)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> IsTourOwnedByGuideAsync(Guid tourId, Guid guideId)
    {
        return await _dbSet.AnyAsync(tour => tour.Id == tourId && tour.GuideId == guideId && tour.IsActive);
    }

    public async Task<bool> HasActiveBookingsAsync(Guid tourId)
    {
        return await _context.Bookings.AnyAsync(booking =>
            booking.Schedule.TourId == tourId &&
            (booking.Status == BookingStatus.Pending || booking.Status == BookingStatus.Confirmed));
    }

}