using GuideGo_Repository.DTOs;

namespace GuideGo_Service.Interfaces;

public interface IBookingService
{
    Task<IEnumerable<BookingResponseDto>> CreateBookingAsync(BookingCreateDto dto, Guid userId, string userRole);
    Task<IEnumerable<BookingResponseDto>> GetUserBookingsAsync(Guid userId);
    Task<BookingResponseDto?> GetBookingByIdAsync(Guid bookingId);
    Task<bool> CancelBookingAsync(Guid bookingId);
}
