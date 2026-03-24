using System.Security.Claims;
using GuideGo_Repository.DTOs;
using GuideGo_Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PRMGuideGo.Controllers;

/// <summary>
/// API quản lý đặt tour (booking).
/// </summary>
[ApiController]
[Route("api/bookings")]
[Authorize]
public class BookingController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    /// <summary>
    /// Tạo booking mới từ giỏ hàng. Yêu cầu đăng nhập.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateBooking([FromBody] BookingCreateDto dto)
    {
        var userRole = User.FindFirstValue("role") ?? "Tourist";
        if (!Guid.TryParse(User.FindFirstValue("id"), out var userId))
            return Unauthorized(new { message = "Invalid token." });
        try
        {
            var results = await _bookingService.CreateBookingAsync(dto, userId, userRole);
            return Ok(results);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Lấy danh sách booking của một người dùng theo userId.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetUserBookings([FromQuery] Guid userId)
    {
        var bookings = await _bookingService.GetUserBookingsAsync(userId);
        return Ok(bookings);
    }

    /// <summary>
    /// Lấy chi tiết một booking theo id.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetBookingById(Guid id)
    {
        var booking = await _bookingService.GetBookingByIdAsync(id);
        if (booking is null)
            return NotFound(new { message = "Booking not found." });
        return Ok(booking);
    }

    /// <summary>
    /// Hủy một booking theo id. Không thể hủy booking đã hoàn thành.
    /// </summary>
    [HttpPut("{id:guid}/cancel")]
    public async Task<IActionResult> CancelBooking(Guid id)
    {
        try
        {
            var success = await _bookingService.CancelBookingAsync(id);
            if (!success)
                return NotFound(new { message = "Booking not found." });
            return Ok(new { message = "Booking cancelled successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Đánh dấu booking hoàn thành sau khi tour kết thúc.
    /// Chỉ Guide được gán cho tour hoặc Admin được phép.
    /// Điều kiện: booking.Status == Confirmed và schedule.EndDate &lt;= hôm nay.
    /// </summary>
    [HttpPatch("{id:guid}/complete")]
    [Authorize(Roles = "Guide,Admin")]
    public async Task<IActionResult> CompleteBooking(Guid id)
    {
        if (!Guid.TryParse(User.FindFirstValue("id"), out var actorId))
            return Unauthorized(new { message = "Token không hợp lệ." });

        var isAdmin = User.IsInRole("Admin") ||
                      string.Equals(User.FindFirstValue("role"), "Admin", StringComparison.OrdinalIgnoreCase);
        try
        {
            await _bookingService.CompleteBookingAsync(id, actorId, isAdmin);
            return Ok(new { message = "Booking đã được đánh dấu hoàn thành." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
