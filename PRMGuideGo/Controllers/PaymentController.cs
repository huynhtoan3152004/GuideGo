using GuideGo_Repository.DTOs;
using GuideGo_Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace PRMGuideGo.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
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
