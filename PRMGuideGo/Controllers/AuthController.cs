using GuideGo_Service.Dtos.Auth;
using GuideGo_Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace PRMGuideGo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
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

        var result = await _authService.RegisterAsync(request);

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

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new
            {
                token = (string?)null
            });
        }

        var result = await _authService.LoginAsync(request);

        if (!result.Success)
        {
            return BadRequest(new
            {
                token = (string?)null
            });
        }

        return Ok(new
        {
            token = result.Token
        });
    }
}
