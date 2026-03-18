using System.Security.Claims;
using GuideGo_Service.Dtos.Cart;
using GuideGo_Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PRMGuideGo.Controllers;

/// <summary>
/// API giỏ hàng cho luồng chọn tour và thêm vào giỏ.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Tourist,Admin")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    /// <summary>
    /// Lấy giỏ hàng của người dùng hiện tại.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMyCart()
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(new { statusCode = StatusCodes.Status401Unauthorized, message = "Token không hợp lệ." });
        }

        var cart = await _cartService.GetMyCartAsync(userId);
        return Ok(cart);
    }

    /// <summary>
    /// Thêm tour vào giỏ hàng theo lịch khởi hành.
    /// </summary>
    [HttpPost("items")]
    public async Task<IActionResult> AddItem([FromBody] AddToCartRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errorMessage = ModelState.Values
                .SelectMany(value => value.Errors)
                .Select(error => error.ErrorMessage)
                .FirstOrDefault() ?? "Dữ liệu không hợp lệ.";

            return BadRequest(new { statusCode = StatusCodes.Status400BadRequest, message = errorMessage });
        }

        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(new { statusCode = StatusCodes.Status401Unauthorized, message = "Token không hợp lệ." });
        }

        var result = await _cartService.AddToCartAsync(userId, request);
        if (!result.Success)
        {
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
