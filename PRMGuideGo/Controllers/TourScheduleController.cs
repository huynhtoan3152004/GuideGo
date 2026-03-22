using System.Security.Claims;
using GuideGo_Service.Dtos.Tour;
using GuideGo_Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PRMGuideGo.Controllers;

/// <summary>
/// API quản lý lịch tour (tour schedules).
/// </summary>
[ApiController]
[Route("api/tour-schedules")]
public class TourScheduleController : ControllerBase
{
    private readonly ITourScheduleService _scheduleService;

    public TourScheduleController(ITourScheduleService scheduleService)
    {
        _scheduleService = scheduleService;
    }

    /// <summary>
    /// Lấy danh sách tất cả lịch của một tour.
    /// </summary>
    /// <param name="tourId">Id tour.</param>
    [HttpGet("tour/{tourId:guid}")]
    public async Task<IActionResult> GetByTourId(Guid tourId)
    {
        var schedules = await _scheduleService.GetByTourIdAsync(tourId);
        return Ok(schedules);
    }

    /// <summary>
    /// Lấy chi tiết một lịch tour theo id.
    /// </summary>
    /// <param name="id">Id lịch tour.</param>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var schedule = await _scheduleService.GetByIdAsync(id);
        if (schedule is null)
        {
            return NotFound(new { statusCode = StatusCodes.Status404NotFound, message = "Không tìm thấy lịch tour." });
        }

        return Ok(schedule);
    }

    /// <summary>
    /// Tạo mới lịch tour. Chỉ Guide (owner) hoặc Admin được phép.
    /// </summary>
    /// <param name="request">Thông tin lịch tour cần tạo.</param>
    [HttpPost]
    [Authorize(Roles = "Guide,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateTourScheduleRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errorMessage = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault() ?? "Dữ liệu không hợp lệ.";

            return BadRequest(new { statusCode = StatusCodes.Status400BadRequest, message = errorMessage });
        }

        if (!TryGetCurrentUserId(out var actorId))
        {
            return Unauthorized(new { statusCode = StatusCodes.Status401Unauthorized, message = "Token không hợp lệ." });
        }

        var isAdmin = User.IsInRole("Admin") ||
                      string.Equals(User.FindFirstValue("role"), "Admin", StringComparison.OrdinalIgnoreCase);

        var result = await _scheduleService.CreateAsync(request, actorId, isAdmin);
        if (!result.Success)
        {
            if (result.Message.Contains("quyền", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { statusCode = StatusCodes.Status403Forbidden, message = result.Message });
            }

            if (result.Message.Contains("không tìm thấy", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { statusCode = StatusCodes.Status404NotFound, message = result.Message });
            }

            return BadRequest(new { statusCode = StatusCodes.Status400BadRequest, message = result.Message });
        }

        return Ok(new
        {
            statusCode = StatusCodes.Status200OK,
            message = result.Message,
            data = result.Data
        });
    }

    /// <summary>
    /// Cập nhật lịch tour theo id. Chỉ Guide (owner) hoặc Admin được phép.
    /// </summary>
    /// <param name="id">Id lịch tour.</param>
    /// <param name="request">Thông tin cập nhật lịch tour.</param>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Guide,Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTourScheduleRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errorMessage = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault() ?? "Dữ liệu không hợp lệ.";

            return BadRequest(new { statusCode = StatusCodes.Status400BadRequest, message = errorMessage });
        }

        if (!TryGetCurrentUserId(out var actorId))
        {
            return Unauthorized(new { statusCode = StatusCodes.Status401Unauthorized, message = "Token không hợp lệ." });
        }

        var isAdmin = User.IsInRole("Admin") ||
                      string.Equals(User.FindFirstValue("role"), "Admin", StringComparison.OrdinalIgnoreCase);

        var result = await _scheduleService.UpdateAsync(id, request, actorId, isAdmin);
        if (!result.Success)
        {
            if (result.Message.Contains("quyền", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { statusCode = StatusCodes.Status403Forbidden, message = result.Message });
            }

            if (result.Message.Contains("không tìm thấy", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { statusCode = StatusCodes.Status404NotFound, message = result.Message });
            }

            return BadRequest(new { statusCode = StatusCodes.Status400BadRequest, message = result.Message });
        }

        return Ok(new { statusCode = StatusCodes.Status200OK, message = result.Message });
    }

    /// <summary>
    /// Xóa lịch tour theo id. Chỉ Guide (owner) hoặc Admin được phép.
    /// Không thể xóa nếu đang có đơn đặt chỗ hoạt động.
    /// </summary>
    /// <param name="id">Id lịch tour.</param>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Guide,Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!TryGetCurrentUserId(out var actorId))
        {
            return Unauthorized(new { statusCode = StatusCodes.Status401Unauthorized, message = "Token không hợp lệ." });
        }

        var isAdmin = User.IsInRole("Admin") ||
                      string.Equals(User.FindFirstValue("role"), "Admin", StringComparison.OrdinalIgnoreCase);

        var result = await _scheduleService.DeleteAsync(id, actorId, isAdmin);
        if (!result.Success)
        {
            if (result.Message.Contains("quyền", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { statusCode = StatusCodes.Status403Forbidden, message = result.Message });
            }

            if (result.Message.Contains("không tìm thấy", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { statusCode = StatusCodes.Status404NotFound, message = result.Message });
            }

            return BadRequest(new { statusCode = StatusCodes.Status400BadRequest, message = result.Message });
        }

        return Ok(new { statusCode = StatusCodes.Status200OK, message = result.Message });
    }

    private bool TryGetCurrentUserId(out Guid userId)
    {
        var userIdClaim = User.FindFirstValue("id");
        return Guid.TryParse(userIdClaim, out userId);
    }
}
