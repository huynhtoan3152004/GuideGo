using GuideGo_Repository.Data;
using GuideGo_Repository.Entities;
using GuideGo_Repository.Enums;
using GuideGo_Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GuideGo_Repository.Repositories;

public class TourScheduleRepository : ITourScheduleRepository
{
    private readonly AppDbContext _context;

    public TourScheduleRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TourSchedule?> GetByIdAsync(Guid id)
    {
        return await _context.TourSchedules
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IEnumerable<TourSchedule>> GetByTourIdAsync(Guid tourId)
    {
        return await _context.TourSchedules
            .AsNoTracking()
            .Where(s => s.TourId == tourId)
            .OrderBy(s => s.StartDate)
            .ToListAsync();
    }

    public async Task<TourSchedule> AddAsync(TourSchedule schedule)
    {
        await _context.TourSchedules.AddAsync(schedule);
        await _context.SaveChangesAsync();
        return schedule;
    }

    public async Task UpdateAsync(TourSchedule schedule)
    {
        _context.TourSchedules.Update(schedule);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(TourSchedule schedule)
    {
        _context.TourSchedules.Remove(schedule);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsActiveTourAsync(Guid tourId)
    {
        return await _context.Tours.AnyAsync(t => t.Id == tourId && t.IsActive);
    }

    public async Task<bool> IsTourOwnedByGuideAsync(Guid tourId, Guid guideId)
    {
        return await _context.Tours.AnyAsync(t => t.Id == tourId && t.GuideId == guideId && t.IsActive);
    }

    public async Task<Guid?> GetGuideIdByUserIdAsync(Guid userId)
    {
        return await _context.Guides
            .Where(g => g.UserId == userId)
            .Select(g => (Guid?)g.Id)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> HasActiveBookingsAsync(Guid scheduleId)
    {
        return await _context.Bookings.AnyAsync(b =>
            b.ScheduleId == scheduleId &&
            (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed));
    }
}
