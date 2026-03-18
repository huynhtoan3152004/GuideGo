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
        var bookings = await _bookingRepo.Query()
            .Where(b => dto.BookingIds.Contains(b.Id) && b.PaymentId == null)
            .ToListAsync();

        if (bookings.Count != dto.BookingIds.Count)
            throw new KeyNotFoundException("One or more bookings not found or already have a payment.");

        if (bookings.Any(b => b.Status != BookingStatus.Pending))
            throw new InvalidOperationException("Only pending bookings can be paid.");

        var payment = new Payment
        {
            Amount        = bookings.Sum(b => b.TotalPrice),
            PaymentMethod = dto.PaymentMethod,
            Status        = PaymentStatus.Pending
        };

        await _paymentRepo.AddAsync(payment);
        await _paymentRepo.SaveChangesAsync();

        foreach (var booking in bookings)
            booking.PaymentId = payment.Id;

        await _bookingRepo.SaveChangesAsync();

        return MapToResponse(payment, bookings);
    }

    public async Task<PaymentResponseDto> ConfirmPaymentAsync(Guid paymentId)
    {
        var payment = await _paymentRepo.Query()
            .Include(p => p.Bookings)
            .FirstOrDefaultAsync(p => p.Id == paymentId)
            ?? throw new KeyNotFoundException("Payment not found.");

        if (payment.Status != PaymentStatus.Pending)
            throw new InvalidOperationException("Only pending payments can be confirmed.");

        payment.Status = PaymentStatus.Completed;
        payment.PaidAt = DateTime.UtcNow;

        foreach (var booking in payment.Bookings)
            booking.Status = BookingStatus.Confirmed;

        await _paymentRepo.SaveChangesAsync();

        return MapToResponse(payment, payment.Bookings.ToList());
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

        return MapToResponse(payment, []);
    }

    public async Task<PaymentResponseDto?> GetByBookingIdAsync(Guid bookingId)
    {
        var booking = await _bookingRepo.Query()
            .Include(b => b.Payment)
                .ThenInclude(p => p!.Bookings)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking?.Payment is null) return null;

        return MapToResponse(booking.Payment, booking.Payment.Bookings.ToList());
    }

    public async Task<PaymentResponseDto?> GetByPaymentIdAsync(Guid paymentId)
    {
        var payment = await _paymentRepo.Query()
            .Include(p => p.Bookings)
            .FirstOrDefaultAsync(p => p.Id == paymentId);

        return payment is null ? null : MapToResponse(payment, payment.Bookings.ToList());
    }

    private static PaymentResponseDto MapToResponse(Payment payment, List<Booking> bookings) => new()
    {
        Id            = payment.Id,
        BookingIds    = bookings.Select(b => b.Id).ToList(),
        Amount        = payment.Amount,
        PaymentMethod = payment.PaymentMethod,
        Status        = payment.Status.ToString(),
        PaidAt        = payment.PaidAt
    };
}
