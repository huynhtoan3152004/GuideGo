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
            .Include(tour => tour.Images)
            .Include(tour => tour.Guide)
                .ThenInclude(guide => guide!.User)
            .Include(tour => tour.Schedules)
            .OrderByDescending(tour => tour.CreatedAt)
            .ToListAsync();
    }

    public async Task<(IEnumerable<Tour> Items, int TotalItems)> SearchActiveToursAsync(
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
        int pageSize)
    {
        var query = _dbSet
            .AsNoTracking()
            .Where(tour => tour.IsActive)
            .Include(tour => tour.Location)
            .Include(tour => tour.Images)
            .Include(tour => tour.Guide)
                .ThenInclude(guide => guide!.User)
            .Include(tour => tour.Schedules)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var normalizedKeyword = keyword.Trim().ToLower();
            query = query.Where(tour =>
                tour.Title.ToLower().Contains(normalizedKeyword) ||
                (tour.Description != null && tour.Description.ToLower().Contains(normalizedKeyword)));
        }

        if (!string.IsNullOrWhiteSpace(city))
        {
            var normalizedCity = city.Trim().ToLower();
            query = query.Where(tour => tour.Location.City != null && tour.Location.City.ToLower().Contains(normalizedCity));
        }

        if (locationId.HasValue)
        {
            query = query.Where(tour => tour.LocationId == locationId.Value);
        }

        if (!string.IsNullOrWhiteSpace(guideLanguage))
        {
            var normalizedLanguage = guideLanguage.Trim().ToLower();
            query = query.Where(tour =>
                tour.Guide != null &&
                tour.Guide.Languages.Any(language => language.ToLower() == normalizedLanguage));
        }

        if (verifiedGuideOnly)
        {
            query = query.Where(tour => tour.Guide != null && tour.Guide.IsVerified);
        }

        if (minPrice.HasValue)
        {
            query = query.Where(tour => tour.PricePerPerson >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(tour => tour.PricePerPerson <= maxPrice.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(tour => tour.Schedules.Any(schedule => schedule.StartDate >= startDate.Value));
        }

        if (endDate.HasValue)
        {
            query = query.Where(tour => tour.Schedules.Any(schedule => schedule.EndDate <= endDate.Value));
        }

        query = (sortBy ?? "created_at").Trim().ToLower() switch
        {
            "price_asc" => query.OrderBy(tour => tour.PricePerPerson),
            "price_desc" => query.OrderByDescending(tour => tour.PricePerPerson),
            "rating_desc" => query.OrderByDescending(tour => tour.Rating),
            _ => query.OrderByDescending(tour => tour.CreatedAt)
        };

        var totalItems = await query.CountAsync();
        var skip = (page - 1) * pageSize;

        var items = await query
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalItems);
    }

    public async Task<Tour?> GetTourByIdAsync(Guid id)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(tour => tour.Location)
            .Include(tour => tour.Images)
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

    public async Task<bool> ExistsActiveTourAsync(Guid tourId)
    {
        return await _dbSet.AnyAsync(tour => tour.Id == tourId && tour.IsActive);
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

    public async Task AddTourImageAsync(TourImage tourImage)
    {
        await _context.TourImages.AddAsync(tourImage);
        await _context.SaveChangesAsync();
    }

}