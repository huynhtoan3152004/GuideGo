using System.Security.Claims;
using GuideGo_Service.Dtos.Review;
using GuideGo_Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PRMGuideGo.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Tourist")]
public class ReviewController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(new { statusCode = StatusCodes.Status401Unauthorized, message = "Invalid token." });
        }

        var reviews = await _reviewService.GetMyReviewsAsync(userId);
        return Ok(reviews);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(new { statusCode = StatusCodes.Status401Unauthorized, message = "Invalid token." });
        }

        var review = await _reviewService.GetMyReviewByIdAsync(id, userId);
        if (review is null)
        {
            return NotFound(new { statusCode = StatusCodes.Status404NotFound, message = "Review not found." });
        }

        return Ok(review);
    }

    [HttpPost]
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

        var result = await _reviewService.UpdateAsync(id, request, userId);
        if (!result.Success)
        {
            return NotFound(new { statusCode = StatusCodes.Status404NotFound, message = result.Message });
        }

        return Ok(new { statusCode = StatusCodes.Status200OK, message = result.Message });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(new { statusCode = StatusCodes.Status401Unauthorized, message = "Invalid token." });
        }

        var result = await _reviewService.DeleteAsync(id, userId);
        if (!result.Success)
        {
            return NotFound(new { statusCode = StatusCodes.Status404NotFound, message = result.Message });
        }

        return Ok(new { statusCode = StatusCodes.Status200OK, message = result.Message });
    }

    private bool TryGetCurrentUserId(out Guid userId)
    {
        var userIdClaim = User.FindFirstValue("id");
        return Guid.TryParse(userIdClaim, out userId);
    }
}
