using GuideGo_Repository.DTOs;
using GuideGo_Repository.Entities;
using GuideGo_Repository.Enums;
using GuideGo_Repository.Repositories.Interfaces;
using GuideGo_Service.Helpers;
using GuideGo_Service.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace GuideGo_Service.Services;

public class VnPayService : IVnPayService
{
    private readonly IGenericRepository<Payment> _paymentRepo;
    private readonly IGenericRepository<Booking> _bookingRepo;
    private readonly string _baseUrl;
    private readonly string _tmnCode;
    private readonly string _hashSecret;
    private readonly string _returnUrl;

    public VnPayService(
        IGenericRepository<Payment> paymentRepo,
        IGenericRepository<Booking> bookingRepo,
        IConfiguration config)
    {
        _paymentRepo = paymentRepo;
        _bookingRepo = bookingRepo;
        _baseUrl     = config["VnPay:BaseUrl"]    ?? throw new InvalidOperationException("VnPay:BaseUrl missing");
        _tmnCode     = config["VnPay:TmnCode"]    ?? throw new InvalidOperationException("VnPay:TmnCode missing");
        _hashSecret  = config["VnPay:HashSecret"] ?? throw new InvalidOperationException("VnPay:HashSecret missing");
        _returnUrl   = config["VnPay:ReturnUrl"]  ?? throw new InvalidOperationException("VnPay:ReturnUrl missing");
    }

    public async Task<string> CreatePaymentUrlAsync(Guid bookingId, string ipAddress)
    {
        var booking = await _bookingRepo.Query()
            .Include(b => b.Payment)
            .FirstOrDefaultAsync(b => b.Id == bookingId)
            ?? throw new KeyNotFoundException("Booking not found.");

        if (booking.Status != BookingStatus.Pending)
            throw new InvalidOperationException("Only pending bookings can be paid.");

        if (booking.Payment is not null)
            throw new InvalidOperationException("Payment already exists for this booking.");

        var payment = new Payment
        {
            BookingId     = bookingId,
            Amount        = booking.TotalPrice,
            PaymentMethod = "VNPay",
            Status        = PaymentStatus.Pending
        };

        await _paymentRepo.AddAsync(payment);
        await _paymentRepo.SaveChangesAsync();

        var txnRef     = payment.Id.ToString("N");
        var amountVnd  = (long)(payment.Amount * 100);
        var orderInfo  = $"Thanh toan booking {bookingId}";
        var createDate = DateTime.UtcNow.AddHours(7);

        return VnPayHelper.BuildPaymentUrl(
            _baseUrl, _tmnCode, _hashSecret, _returnUrl,
            txnRef, amountVnd, orderInfo, ipAddress, createDate);
    }

    public async Task<VnPayReturnResponseDto> ProcessReturnAsync(IQueryCollection queryParams)
        => await HandleCallbackAsync(queryParams);

    public async Task<VnPayReturnResponseDto> ProcessIpnAsync(IQueryCollection queryParams)
        => await HandleCallbackAsync(queryParams);

    private async Task<VnPayReturnResponseDto> HandleCallbackAsync(IQueryCollection queryParams)
    {
        if (!VnPayHelper.VerifySignature(queryParams, _hashSecret))
            return new VnPayReturnResponseDto { Success = false, Message = "Invalid signature." };

        var responseCode = queryParams["vnp_ResponseCode"].ToString();
        var txnRef       = queryParams["vnp_TxnRef"].ToString();
        var vnpTxnId     = queryParams["vnp_TransactionNo"].ToString();

        if (!Guid.TryParseExact(txnRef, "N", out var paymentId))
            return new VnPayReturnResponseDto { Success = false, Message = "Invalid transaction reference." };

        var payment = await _paymentRepo.Query()
            .Include(p => p.Booking)
            .FirstOrDefaultAsync(p => p.Id == paymentId);

        if (payment is null)
            return new VnPayReturnResponseDto { Success = false, Message = "Payment not found." };

        if (payment.Status != PaymentStatus.Pending)
            return new VnPayReturnResponseDto
            {
                Success            = payment.Status == PaymentStatus.Completed,
                Message            = $"Payment already {payment.Status}.",
                VnPayTransactionId = vnpTxnId,
                PaymentId          = payment.Id
            };

        bool isSuccess = responseCode == "00";

        payment.Status = isSuccess ? PaymentStatus.Completed : PaymentStatus.Failed;

        if (isSuccess)
        {
            payment.PaidAt         = DateTime.UtcNow;
            payment.Booking.Status = BookingStatus.Confirmed;
        }

        await _paymentRepo.SaveChangesAsync();

        return new VnPayReturnResponseDto
        {
            Success            = isSuccess,
            Message            = isSuccess ? "Payment successful." : $"Payment failed. Code: {responseCode}",
            VnPayTransactionId = vnpTxnId,
            PaymentId          = payment.Id
        };
    }
}
