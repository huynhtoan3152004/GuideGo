using System.Security.Claims;
using GuideGo_Service.Dtos.Review;
using GuideGo_Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PRMGuideGo.Controllers;

/// <summary>
/// API quản lý đánh giá hướng dẫn viên.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReviewController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    /// <summary>
    /// Lấy danh sách reviews. Hỗ trợ filter theo tour_id hoặc guide_id.
    /// Không truyền gì → trả về tất cả reviews công khai.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(
        [FromQuery(Name = "tour_id")] Guid? tourId,
        [FromQuery(Name = "guide_id")] Guid? guideId)
    {
        if (tourId.HasValue)
        {
            var byTour = await _reviewService.GetByTourIdAsync(tourId.Value);
            return Ok(byTour);
        }

        if (guideId.HasValue)
        {
            var byGuide = await _reviewService.GetByGuideIdAsync(guideId.Value);
            return Ok(byGuide);
        }

        var reviews = await _reviewService.GetAllAsync();
        return Ok(reviews);
    }

    /// <summary>
    /// Lấy danh sách reviews do chính tourist đang đăng nhập viết.
    /// </summary>
    [HttpGet("my-reviews")]
    [Authorize(Roles = "Tourist")]
    public async Task<IActionResult> GetMyReviews()
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized(new { statusCode = StatusCodes.Status401Unauthorized, message = "Invalid token." });

        var reviews = await _reviewService.GetMyReviewsAsync(userId);
        return Ok(reviews);
    }

    /// <summary>
    /// API lấy chi tiết 1 review theo id công khai, không cần đăng nhập.
    /// </summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id)
    {
        var review = await _reviewService.GetByIdAsync(id);
        if (review is null)
        {
            return NotFound(new { statusCode = StatusCodes.Status404NotFound, message = "Review not found." });
        }

        return Ok(review);
    }

    /// <summary>
    /// API tạo review chỉ Tourist được dùng và chỉ khi đã hoàn thành tour đã đặt.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Tourist")]
    public async Task<IActionResult> Create([FromBody] CreateReviewRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errorMessage = ModelState.Values
                .SelectMany(value => value.Errors)
                .Select(error => error.ErrorMessage)
                .FirstOrDefault() ?? "Validation failed.";

            return BadRequest(new { statusCode = StatusCodes.Status400BadRequest, message = errorMessage });
        }

        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(new { statusCode = StatusCodes.Status401Unauthorized, message = "Invalid token." });
        }

        var result = await _reviewService.CreateAsync(request, userId);
        if (!result.Success)
        {
            return BadRequest(new { statusCode = StatusCodes.Status400BadRequest, message = result.Message });
        }

        return Ok(new { statusCode = StatusCodes.Status200OK, message = result.Message });
    }

    /// <summary>
    /// API cập nhật review chỉ Admin hoặc người tạo review được dùng.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateReviewRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errorMessage = ModelState.Values
                .SelectMany(value => value.Errors)
                .Select(error => error.ErrorMessage)
                .FirstOrDefault() ?? "Validation failed.";

            return BadRequest(new { statusCode = StatusCodes.Status400BadRequest, message = errorMessage });
        }

        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(new { statusCode = StatusCodes.Status401Unauthorized, message = "Invalid token." });
        }

        var isAdmin = User.IsInRole("Admin") || string.Equals(User.FindFirstValue("role"), "Admin", StringComparison.OrdinalIgnoreCase);
        var result = await _reviewService.UpdateAsync(id, request, userId, isAdmin);
        if (!result.Success)
        {
            if (result.Message.Contains("not authorized", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    statusCode = StatusCodes.Status403Forbidden,
                    message = result.Message
                });
            }

            if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new
                {
                    statusCode = StatusCodes.Status404NotFound,
                    message = result.Message
                });
            }

            return BadRequest(new { statusCode = StatusCodes.Status400BadRequest, message = result.Message });
        }

        return Ok(new { statusCode = StatusCodes.Status200OK, message = result.Message });
    }

    /// <summary>
    /// API xóa review chỉ Admin hoặc người tạo review được dùng.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(new { statusCode = StatusCodes.Status401Unauthorized, message = "Invalid token." });
        }

        var isAdmin = User.IsInRole("Admin") || string.Equals(User.FindFirstValue("role"), "Admin", StringComparison.OrdinalIgnoreCase);
        var result = await _reviewService.DeleteAsync(id, userId, isAdmin);
        if (!result.Success)
        {
            if (result.Message.Contains("not authorized", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    statusCode = StatusCodes.Status403Forbidden,
                    message = result.Message
                });
            }

            if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new
                {
                    statusCode = StatusCodes.Status404NotFound,
                    message = result.Message
                });
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
