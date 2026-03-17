using System.Security.Claims;
using GuideGo_Service.Dtos.User;
using GuideGo_Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PRMGuideGo.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userService.GetAllAsync();
        return Ok(users);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user is null)
        {
            return NotFound(new { message = "User not found." });
        }

        return Ok(user);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateUserRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errorMessage = ModelState.Values
                .SelectMany(value => value.Errors)
                .Select(error => error.ErrorMessage)
                .FirstOrDefault() ?? "Validation failed.";

            return BadRequest(new
            {
                statusCode = StatusCodes.Status400BadRequest,
                message = errorMessage
            });
        }

        var result = await _userService.CreateUserAsync(request);
        if (!result.Success)
        {
            return BadRequest(new
            {
                statusCode = StatusCodes.Status400BadRequest,
                message = result.Message
            });
        }

        return Ok(new
        {
            statusCode = StatusCodes.Status200OK,
            message = result.Message
        });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Put(Guid id, [FromBody] UpdateUserRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errorMessage = ModelState.Values
                .SelectMany(value => value.Errors)
                .Select(error => error.ErrorMessage)
                .FirstOrDefault() ?? "Validation failed.";

            return BadRequest(new
            {
                statusCode = StatusCodes.Status400BadRequest,
                message = errorMessage
            });
        }

        var actorIdClaim = User.FindFirstValue("id");
        if (!Guid.TryParse(actorIdClaim, out var actorId))
        {
            return Unauthorized(new
            {
                statusCode = StatusCodes.Status401Unauthorized,
                message = "Invalid token."
            });
        }

        var isAdmin = User.IsInRole("Admin") ||
                      string.Equals(User.FindFirstValue("role"), "Admin", StringComparison.OrdinalIgnoreCase);

        var result = await _userService.UpdateUserAsync(id, request, actorId, isAdmin);

        if (!result.Success)
        {
            if (result.Message.Contains("permission", StringComparison.OrdinalIgnoreCase)
                || result.Message.Contains("Only admin can update role", StringComparison.OrdinalIgnoreCase)
                || result.Message.Contains("cannot update their own role", StringComparison.OrdinalIgnoreCase)
                || result.Message.Contains("cannot change your own role", StringComparison.OrdinalIgnoreCase)
                || result.Message.Contains("cannot update role of an Admin account", StringComparison.OrdinalIgnoreCase)
                || result.Message.Contains("promote another user to Admin role", StringComparison.OrdinalIgnoreCase))
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

            return BadRequest(new
            {
                statusCode = StatusCodes.Status400BadRequest,
                message = result.Message
            });
        }

        return Ok(new
        {
            statusCode = StatusCodes.Status200OK,
            message = result.Message
        });
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _userService.SoftDeleteAsync(id);
        if (!result.Success)
        {
            return NotFound(new { message = result.Message });
        }

        return Ok(new { message = result.Message });
    }
}
