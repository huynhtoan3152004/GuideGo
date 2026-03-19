using GuideGo_Repository.DTOs;
using GuideGo_Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace PRMGuideGo.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly IVnPayService _vnPayService;

    public PaymentController(IPaymentService paymentService, IVnPayService vnPayService)
    {
        _paymentService = paymentService;
        _vnPayService   = vnPayService;
    }

    // ─── VNPay ───────────────────────────────────────────────────────────────

    // POST /api/payments/vnpay/create-url
    [HttpPost("vnpay/create-url")]
    public async Task<IActionResult> CreateVnPayUrl([FromBody] VnPayCreateUrlDto dto)
    {
        try
        {
            var remoteIp = HttpContext.Connection.RemoteIpAddress;
            // Convert IPv6 loopback (::1) to IPv4 (127.0.0.1) — VNPay does not support IPv6
            if (remoteIp != null && remoteIp.IsIPv6LinkLocal || remoteIp?.ToString() == "::1")
                remoteIp = System.Net.IPAddress.Parse("127.0.0.1");
            var ipAddress = remoteIp?.ToString() ?? "127.0.0.1";
            var paymentUrl = await _vnPayService.CreatePaymentUrlAsync(dto.BookingIds, ipAddress);
            return Ok(new { paymentUrl });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET /api/payments/vnpay/return  ← VNPay redirects user here after payment
    [HttpGet("vnpay/return")]
    public async Task<IActionResult> VnPayReturn()
    {
        var result = await _vnPayService.ProcessReturnAsync(Request.Query);
        return Ok(result);
    }

    // GET /api/payments/vnpay/ipn  ← VNPay calls this server-to-server
    [HttpGet("vnpay/ipn")]
    public async Task<IActionResult> VnPayIpn()
    {
        var result = await _vnPayService.ProcessIpnAsync(Request.Query);
        // VNPay expects specific JSON response to acknowledge receipt
        if (result.Success)
            return Ok(new { RspCode = "00", Message = "Confirm Success" });

        return Ok(new { RspCode = "99", Message = result.Message });
    }

    // POST /api/payments
    [HttpPost]
    public async Task<IActionResult> CreatePayment([FromBody] PaymentCreateDto dto)
    {
        try
        {
            var result = await _paymentService.CreatePaymentAsync(dto);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // PUT /api/payments/{id}/confirm
    [HttpPut("{id:guid}/confirm")]
    public async Task<IActionResult> ConfirmPayment(Guid id)
    {
        try
        {
            var result = await _paymentService.ConfirmPaymentAsync(id);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // PUT /api/payments/{id}/fail
    [HttpPut("{id:guid}/fail")]
    public async Task<IActionResult> FailPayment(Guid id)
    {
        try
        {
            var result = await _paymentService.FailPaymentAsync(id);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET /api/payments/booking/{bookingId}
    [HttpGet("booking/{bookingId:guid}")]
    public async Task<IActionResult> GetByBookingId(Guid bookingId)
    {
        var payment = await _paymentService.GetByBookingIdAsync(bookingId);
        if (payment is null)
            return NotFound(new { message = "Payment not found." });
        return Ok(payment);
    }
}
