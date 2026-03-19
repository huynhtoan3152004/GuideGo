using GuideGo_Repository.DTOs;

namespace GuideGo_Service.Interfaces;

public interface IPaymentService
{
    Task<PaymentResponseDto> CreatePaymentAsync(PaymentCreateDto dto);
    Task<PaymentResponseDto> ConfirmPaymentAsync(Guid paymentId);
    Task<PaymentResponseDto> FailPaymentAsync(Guid paymentId);
    Task<PaymentResponseDto?> GetByBookingIdAsync(Guid bookingId);
    Task<PaymentResponseDto?> GetByPaymentIdAsync(Guid paymentId);
}
