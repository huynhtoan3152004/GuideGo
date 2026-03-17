using GuideGo_Repository.DTOs;
using GuideGo_Repository.Entities;
using GuideGo_Repository.Enums;
using GuideGo_Repository.Repositories.Interfaces;
using GuideGo_Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GuideGo_Service.Services;

public class PaymentService : IPaymentService
{
    private readonly IGenericRepository<Payment> _paymentRepo;
    private readonly IGenericRepository<Booking> _bookingRepo;

    public PaymentService(
        IGenericRepository<Payment> paymentRepo,
        IGenericRepository<Booking> bookingRepo)
    {
        _paymentRepo = paymentRepo;
        _bookingRepo = bookingRepo;
    }

    public async Task<PaymentResponseDto> CreatePaymentAsync(PaymentCreateDto dto)
    {
        var booking = await _bookingRepo.Query()
            .Include(b => b.Payment)
            .FirstOrDefaultAsync(b => b.Id == dto.BookingId)
            ?? throw new KeyNotFoundException("Booking not found.");

        if (booking.Status != BookingStatus.Pending)
            throw new InvalidOperationException("Only pending bookings can be paid.");

        if (booking.Payment is not null)
            throw new InvalidOperationException("Payment already exists for this booking.");

        var payment = new Payment
        {
            BookingId     = dto.BookingId,
            Amount        = booking.TotalPrice,
            PaymentMethod = dto.PaymentMethod,
            Status        = PaymentStatus.Pending
        };

        await _paymentRepo.AddAsync(payment);
        await _paymentRepo.SaveChangesAsync();

        return MapToResponse(payment);
    }

    public async Task<PaymentResponseDto> ConfirmPaymentAsync(Guid paymentId)
    {
        var payment = await _paymentRepo.Query()
            .Include(p => p.Booking)
            .FirstOrDefaultAsync(p => p.Id == paymentId)
            ?? throw new KeyNotFoundException("Payment not found.");

        if (payment.Status != PaymentStatus.Pending)
            throw new InvalidOperationException("Only pending payments can be confirmed.");

        payment.Status        = PaymentStatus.Completed;
        payment.PaidAt        = DateTime.UtcNow;
        payment.Booking.Status = BookingStatus.Confirmed;

        await _paymentRepo.SaveChangesAsync();

        return MapToResponse(payment);
    }

    public async Task<PaymentResponseDto> FailPaymentAsync(Guid paymentId)
    {
        var payment = await _paymentRepo.Query()
            .FirstOrDefaultAsync(p => p.Id == paymentId)
            ?? throw new KeyNotFoundException("Payment not found.");

        if (payment.Status != PaymentStatus.Pending)
            throw new InvalidOperationException("Only pending payments can be marked as failed.");

        payment.Status = PaymentStatus.Failed;

        await _paymentRepo.SaveChangesAsync();

        return MapToResponse(payment);
    }

    public async Task<PaymentResponseDto?> GetByBookingIdAsync(Guid bookingId)
    {
        var payment = await _paymentRepo.Query()
            .FirstOrDefaultAsync(p => p.BookingId == bookingId);

        return payment is null ? null : MapToResponse(payment);
    }

    private static PaymentResponseDto MapToResponse(Payment payment) => new()
    {
        Id            = payment.Id,
        BookingId     = payment.BookingId,
        Amount        = payment.Amount,
        PaymentMethod = payment.PaymentMethod,
        Status        = payment.Status.ToString(),
        PaidAt        = payment.PaidAt
    };
}
