using GuideGo_Repository.Data;
using GuideGo_Repository.DTOs;
using GuideGo_Repository.Entities;
using GuideGo_Repository.Enums;
using GuideGo_Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GuideGo_Service.Services;

public class BookingService : IBookingService
{
    private readonly AppDbContext _context;

    public BookingService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<BookingResponseDto>> CreateBookingAsync(BookingCreateDto dto)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
                .ThenInclude(i => i.Schedule)
                    .ThenInclude(s => s.Tour)
            .FirstOrDefaultAsync(c => c.Id == dto.CartId)
            ?? throw new KeyNotFoundException("Cart not found.");

        if (!cart.Items.Any())
            throw new InvalidOperationException("Cart is empty.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Validate all items before creating any booking
        foreach (var item in cart.Items)
        {
            if (item.Schedule.StartDate <= today)
                throw new InvalidOperationException(
                    $"Tour '{item.Schedule.Tour.Title}' has already started or passed.");

            if (item.Schedule.AvailableSlots < item.PeopleCount)
                throw new InvalidOperationException(
                    $"Not enough slots for '{item.Schedule.Tour.Title}'. Available: {item.Schedule.AvailableSlots}.");
        }

        var bookings = new List<Booking>();

        foreach (var item in cart.Items)
        {
            var totalPrice = item.Schedule.Tour.PricePerPerson * item.PeopleCount;

            var booking = new Booking
            {
                UserId = cart.UserId,
                ScheduleId = item.ScheduleId,
                PeopleCount = item.PeopleCount,
                TotalPrice = totalPrice,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            item.Schedule.AvailableSlots -= item.PeopleCount;
            bookings.Add(booking);
        }

        await _context.Bookings.AddRangeAsync(bookings);
        await _context.SaveChangesAsync();

        return bookings.Zip(cart.Items, (b, i) => MapToResponse(b, i.Schedule)).ToList();
    }

    public async Task<IEnumerable<BookingResponseDto>> GetUserBookingsAsync(Guid userId)
    {
        var bookings = await _context.Bookings
            .Include(b => b.Schedule)
                .ThenInclude(s => s.Tour)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        return bookings.Select(b => MapToResponse(b, b.Schedule));
    }

    public async Task<BookingResponseDto?> GetBookingByIdAsync(Guid bookingId)
    {
        var booking = await _context.Bookings
            .Include(b => b.Schedule)
                .ThenInclude(s => s.Tour)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        return booking is null ? null : MapToResponse(booking, booking.Schedule);
    }

    public async Task<bool> CancelBookingAsync(Guid bookingId)
    {
        var booking = await _context.Bookings
            .Include(b => b.Schedule)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking is null)
            return false;

        if (booking.Status == BookingStatus.Cancelled)
            throw new InvalidOperationException("Booking is already cancelled.");

        booking.Status = BookingStatus.Cancelled;
        booking.Schedule.AvailableSlots += booking.PeopleCount;

        await _context.SaveChangesAsync();
        return true;
    }

    private static BookingResponseDto MapToResponse(Booking booking, TourSchedule schedule) => new()
    {
        Id = booking.Id,
        UserId = booking.UserId,
        ScheduleId = booking.ScheduleId,
        TourTitle = schedule.Tour.Title,
        StartDate = schedule.StartDate,
        EndDate = schedule.EndDate,
        PeopleCount = booking.PeopleCount,
        TotalPrice = booking.TotalPrice,
        Status = booking.Status.ToString(),
        CreatedAt = booking.CreatedAt
    };
}
