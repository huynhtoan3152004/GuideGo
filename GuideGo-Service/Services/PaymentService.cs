using GuideGo_Repository.Data;
using GuideGo_Repository.DTOs;
using GuideGo_Repository.Entities;
using GuideGo_Repository.Enums;
using GuideGo_Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GuideGo_Service.Services;

public class PaymentService : IPaymentService
{
    private readonly AppDbContext _context;

    public PaymentService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentResponseDto> CreatePaymentAsync(PaymentCreateDto dto)
    {
        var booking = await _context.Bookings
            .Include(b => b.Payment)
            .FirstOrDefaultAsync(b => b.Id == dto.BookingId)
            ?? throw new KeyNotFoundException("Booking not found.");

        if (booking.Status != BookingStatus.Pending)
            throw new InvalidOperationException("Only pending bookings can be paid.");

        if (booking.Payment is not null)
            throw new InvalidOperationException("Payment already exists for this booking.");

        var payment = new Payment
        {
            BookingId = dto.BookingId,
            Amount = booking.TotalPrice,
            PaymentMethod = dto.PaymentMethod,
            Status = PaymentStatus.Pending
        };

        await _context.Payments.AddAsync(payment);
        await _context.SaveChangesAsync();

        return MapToResponse(payment);
    }

    public async Task<PaymentResponseDto> ConfirmPaymentAsync(Guid paymentId)
    {
        var payment = await _context.Payments
            .Include(p => p.Booking)
            .FirstOrDefaultAsync(p => p.Id == paymentId)
            ?? throw new KeyNotFoundException("Payment not found.");

        if (payment.Status != PaymentStatus.Pending)
            throw new InvalidOperationException("Only pending payments can be confirmed.");

        payment.Status = PaymentStatus.Completed;
        payment.PaidAt = DateTime.UtcNow;
        payment.Booking.Status = BookingStatus.Confirmed;

        await _context.SaveChangesAsync();

        return MapToResponse(payment);
    }

    public async Task<PaymentResponseDto> FailPaymentAsync(Guid paymentId)
    {
        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.Id == paymentId)
            ?? throw new KeyNotFoundException("Payment not found.");

        if (payment.Status != PaymentStatus.Pending)
            throw new InvalidOperationException("Only pending payments can be marked as failed.");

        payment.Status = PaymentStatus.Failed;

        await _context.SaveChangesAsync();

        return MapToResponse(payment);
    }

    public async Task<PaymentResponseDto?> GetByBookingIdAsync(Guid bookingId)
    {
        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.BookingId == bookingId);

        return payment is null ? null : MapToResponse(payment);
    }

    private static PaymentResponseDto MapToResponse(Payment payment) => new()
    {
        Id = payment.Id,
        BookingId = payment.BookingId,
        Amount = payment.Amount,
        PaymentMethod = payment.PaymentMethod,
        Status = payment.Status.ToString(),
        PaidAt = payment.PaidAt
    };
}
