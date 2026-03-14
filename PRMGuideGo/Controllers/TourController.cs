using System.Security.Claims;
using GuideGo_Service.Dtos.Tour;
using GuideGo_Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PRMGuideGo.Controllers;

/// <summary>
/// API quản lý tour và thông tin chi tiết tour.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TourController : ControllerBase
{
    private readonly ITourService _tourService;

    public TourController(ITourService tourService)
    {
        _tourService = tourService;
    }

    /// <summary>
    /// Lấy danh sách tất cả tour đang hoạt động, kèm Location, Guide và Schedule.
    /// </summary>
    /// <returns>Danh sách tour đang hoạt động.</returns>
    [HttpGet]
    public async Task<IActionResult> GetAllActiveTours()
    {
        var tours = await _tourService.GetAllActiveToursAsync();
        return Ok(tours);
    }

    /// <summary>
    /// Lấy chi tiết một tour theo id.
    /// </summary>
    /// <param name="id">Id tour.</param>
    /// <returns>Thông tin chi tiết tour.</returns>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var tour = await _tourService.GetTourByIdAsync(id);
        if (tour is null)
        {
            return NotFound(new { statusCode = StatusCodes.Status404NotFound, message = "Không tìm thấy tour." });
        }

        return Ok(tour);
    }

    /// <summary>
    /// Tạo mới tour. Chỉ Guide hoặc Admin được phép.
    /// </summary>
    /// <param name="request">Thông tin tạo tour.</param>
    /// <returns>Kết quả tạo tour.</returns>
    [HttpPost]
    [Authorize(Roles = "Guide,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateTourRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errorMessage = ModelState.Values
                .SelectMany(value => value.Errors)
                .Select(error => error.ErrorMessage)
                .FirstOrDefault() ?? "Dữ liệu không hợp lệ.";

            return BadRequest(new { statusCode = StatusCodes.Status400BadRequest, message = errorMessage });
        }

        if (!TryGetCurrentUserId(out var actorId))
        {
            return Unauthorized(new { statusCode = StatusCodes.Status401Unauthorized, message = "Token không hợp lệ." });
        }

        var isAdmin = User.IsInRole("Admin") ||
                      string.Equals(User.FindFirstValue("role"), "Admin", StringComparison.OrdinalIgnoreCase);

        var result = await _tourService.CreateTourAsync(request, actorId, isAdmin);
        if (!result.Success)
        {
            if (result.Message.Contains("permission", StringComparison.OrdinalIgnoreCase) ||
                result.Message.Contains("quyền", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { statusCode = StatusCodes.Status403Forbidden, message = result.Message });
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
    /// Cập nhật tour theo id. Chỉ owner Guide hoặc Admin được phép.
    /// </summary>
    /// <param name="id">Id tour.</param>
    /// <param name="request">Thông tin cập nhật tour.</param>
    /// <returns>Kết quả cập nhật tour.</returns>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Guide,Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTourRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errorMessage = ModelState.Values
                .SelectMany(value => value.Errors)
                .Select(error => error.ErrorMessage)
                .FirstOrDefault() ?? "Dữ liệu không hợp lệ.";

            return BadRequest(new { statusCode = StatusCodes.Status400BadRequest, message = errorMessage });
        }

        if (!TryGetCurrentUserId(out var actorId))
        {
            return Unauthorized(new { statusCode = StatusCodes.Status401Unauthorized, message = "Token không hợp lệ." });
        }

        var isAdmin = User.IsInRole("Admin") ||
                      string.Equals(User.FindFirstValue("role"), "Admin", StringComparison.OrdinalIgnoreCase);

        var result = await _tourService.UpdateTourAsync(id, request, actorId, isAdmin);
        if (!result.Success)
        {
            if (result.Message.Contains("permission", StringComparison.OrdinalIgnoreCase) ||
                result.Message.Contains("quyền", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { statusCode = StatusCodes.Status403Forbidden, message = result.Message });
            }

            if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase) ||
                result.Message.Contains("không tìm thấy", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { statusCode = StatusCodes.Status404NotFound, message = result.Message });
            }

            return BadRequest(new { statusCode = StatusCodes.Status400BadRequest, message = result.Message });
        }

        return Ok(new { statusCode = StatusCodes.Status200OK, message = result.Message });
    }

    /// <summary>
    /// Xóa mềm tour theo id (đặt IsActive = false). Chỉ owner Guide hoặc Admin được phép.
    /// </summary>
    /// <param name="id">Id tour.</param>
    /// <returns>Kết quả xóa tour.</returns>
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

        var result = await _tourService.DeleteTourAsync(id, actorId, isAdmin);
        if (!result.Success)
        {
            if (result.Message.Contains("permission", StringComparison.OrdinalIgnoreCase) ||
                result.Message.Contains("quyền", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { statusCode = StatusCodes.Status403Forbidden, message = result.Message });
            }

            if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase) ||
                result.Message.Contains("không tìm thấy", StringComparison.OrdinalIgnoreCase))
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
