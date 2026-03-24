using GuideGo_Repository.DTOs;
using GuideGo_Repository.Entities;
using GuideGo_Repository.Enums;
using GuideGo_Repository.Repositories.Interfaces;
using GuideGo_Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GuideGo_Service.Services;

public class BookingService : IBookingService
{
    private readonly IGenericRepository<Booking> _bookingRepo;
    private readonly IGenericRepository<Cart> _cartRepo;
    private readonly IGenericRepository<Guide> _guideRepo;

    public BookingService(
        IGenericRepository<Booking> bookingRepo,
        IGenericRepository<Cart> cartRepo,
        IGenericRepository<Guide> guideRepo)
    {
        _bookingRepo = bookingRepo;
        _cartRepo    = cartRepo;
        _guideRepo   = guideRepo;
    }

    public async Task<IEnumerable<BookingResponseDto>> CreateBookingAsync(BookingCreateDto dto, Guid userId, string userRole)
    {
        var cart = await _cartRepo.Query()
            .Include(c => c.Items)
                .ThenInclude(i => i.Schedule)
                    .ThenInclude(s => s.Tour)
            .FirstOrDefaultAsync(c => c.Id == dto.CartId)
            ?? throw new KeyNotFoundException("Cart not found.");

        if (cart.UserId != userId)
            throw new UnauthorizedAccessException("You are not allowed to book from this cart.");

        if (!cart.Items.Any())
            throw new InvalidOperationException("Cart is empty.");

        var isCompany = userRole == "Company";
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        foreach (var item in cart.Items)
        {
            if (item.Schedule.StartDate <= today)
                throw new InvalidOperationException(
                    $"Lịch khởi hành của tour '{item.Schedule.Tour.Title}' đã hết hạn đặt chỗ. Vui lòng đặt trước ít nhất 1 ngày.");

            if (item.Schedule.AvailableSlots < item.PeopleCount)
                throw new InvalidOperationException(
                    $"Not enough slots for '{item.Schedule.Tour.Title}'. Available: {item.Schedule.AvailableSlots}.");
        }

        var bookings = new List<Booking>();

        foreach (var item in cart.Items)
        {
            var unitPrice = isCompany
                && item.Schedule.Tour.GroupPricePerPerson.HasValue
                && (!item.Schedule.Tour.MinGroupSize.HasValue || item.PeopleCount >= item.Schedule.Tour.MinGroupSize)
                    ? item.Schedule.Tour.GroupPricePerPerson.Value
                    : item.Schedule.Tour.PricePerPerson;

            var booking = new Booking
            {
                UserId      = cart.UserId,
                ScheduleId  = item.ScheduleId,
                PeopleCount = item.PeopleCount,
                TotalPrice  = unitPrice * item.PeopleCount,
                Status      = BookingStatus.Pending,
                CreatedAt   = DateTime.UtcNow
            };

            item.Schedule.AvailableSlots -= item.PeopleCount;
            bookings.Add(booking);
        }

        await _bookingRepo.AddRangeAsync(bookings);
        await _bookingRepo.SaveChangesAsync();

        return bookings.Zip(cart.Items, (b, i) => MapToResponse(b, i.Schedule)).ToList();
    }

    public async Task<IEnumerable<BookingResponseDto>> GetUserBookingsAsync(Guid userId)
    {
        var bookings = await _bookingRepo.Query()
            .Include(b => b.Schedule)
                .ThenInclude(s => s.Tour)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        return bookings.Select(b => MapToResponse(b, b.Schedule));
    }

    public async Task<BookingResponseDto?> GetBookingByIdAsync(Guid bookingId)
    {
        var booking = await _bookingRepo.Query()
            .Include(b => b.Schedule)
                .ThenInclude(s => s.Tour)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        return booking is null ? null : MapToResponse(booking, booking.Schedule);
    }

    public async Task<bool> CancelBookingAsync(Guid bookingId)
    {
        var booking = await _bookingRepo.Query()
            .Include(b => b.Schedule)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking is null) return false;

        if (booking.Status == BookingStatus.Cancelled)
            throw new InvalidOperationException("Booking is already cancelled.");

        if (booking.Status == BookingStatus.Completed)
            throw new InvalidOperationException("Không thể hủy booking đã hoàn thành.");

        booking.Status = BookingStatus.Cancelled;
        booking.Schedule.AvailableSlots += booking.PeopleCount;

        await _bookingRepo.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CompleteBookingAsync(Guid bookingId, Guid actorId, bool isAdmin)
    {
        var booking = await _bookingRepo.Query()
            .Include(b => b.Schedule)
                .ThenInclude(s => s.Tour)
            .FirstOrDefaultAsync(b => b.Id == bookingId)
            ?? throw new KeyNotFoundException("Không tìm thấy booking.");

        if (booking.Status != BookingStatus.Confirmed)
            throw new InvalidOperationException("Chỉ có thể hoàn thành booking đang ở trạng thái Confirmed.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (booking.Schedule.EndDate > today)
            throw new InvalidOperationException("Tour chưa kết thúc, không thể đánh dấu hoàn thành.");

        if (!isAdmin)
        {
            var guide = await _guideRepo.Query()
                .FirstOrDefaultAsync(g => g.UserId == actorId);

            if (guide is null || booking.Schedule.Tour.GuideId != guide.Id)
                throw new UnauthorizedAccessException("Bạn không có quyền hoàn thành booking này.");
        }

        booking.Status = BookingStatus.Completed;
        await _bookingRepo.SaveChangesAsync();
        return true;
    }

    private static BookingResponseDto MapToResponse(Booking booking, TourSchedule schedule) => new()
    {
        Id          = booking.Id,
        UserId      = booking.UserId,
        ScheduleId  = booking.ScheduleId,
        TourTitle   = schedule.Tour.Title,
        StartDate   = schedule.StartDate,
        EndDate     = schedule.EndDate,
        PeopleCount = booking.PeopleCount,
        TotalPrice  = booking.TotalPrice,
        Status      = booking.Status.ToString(),
        CreatedAt   = booking.CreatedAt
    };
}
