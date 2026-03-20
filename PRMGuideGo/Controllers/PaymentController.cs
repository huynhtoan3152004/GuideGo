using GuideGo_Repository.DTOs;
using GuideGo_Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace PRMGuideGo.Controllers;

/// <summary>
/// API quản lý thanh toán, tích hợp cổng thanh toán VNPay.
/// </summary>
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

    /// <summary>
    /// Tạo URL thanh toán VNPay từ danh sách bookingId.
    /// </summary>
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

    /// <summary>
    /// Callback VNPay redirect người dùng về sau khi thanh toán.
    /// </summary>
    [HttpGet("vnpay/return")]
    public async Task<IActionResult> VnPayReturn()
    {
        var result = await _vnPayService.ProcessReturnAsync(Request.Query);
        return Ok(result);
    }

    /// <summary>
    /// IPN endpoint để VNPay gọi server-to-server xác nhận kết quả thanh toán.
    /// </summary>
    [HttpGet("vnpay/ipn")]
    public async Task<IActionResult> VnPayIpn()
    {
        var result = await _vnPayService.ProcessIpnAsync(Request.Query);
        // VNPay expects specific JSON response to acknowledge receipt
        if (result.Success)
            return Ok(new { RspCode = "00", Message = "Confirm Success" });

        return Ok(new { RspCode = "99", Message = result.Message });
    }

    /// <summary>
    /// Tạo bản ghi thanh toán thủ công cho một booking.
    /// </summary>
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

    /// <summary>
    /// Xác nhận thanh toán thành công theo paymentId.
    /// </summary>
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

    /// <summary>
    /// Đánh dấu thanh toán thất bại theo paymentId.
    /// </summary>
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

    /// <summary>
    /// Lấy thông tin thanh toán theo bookingId.
    /// </summary>
    [HttpGet("booking/{bookingId:guid}")]
    public async Task<IActionResult> GetByBookingId(Guid bookingId)
    {
        var payment = await _paymentService.GetByBookingIdAsync(bookingId);
        if (payment is null)
            return NotFound(new { message = "Payment not found." });
        return Ok(payment);
    }
}
