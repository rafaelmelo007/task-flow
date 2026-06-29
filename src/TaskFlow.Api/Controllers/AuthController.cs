using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TaskFlow.Application.Auth.Dtos;
using TaskFlow.Application.Auth.Services;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Models;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICurrentUser _currentUser;

    public AuthController(IAuthService authService, ICurrentUser currentUser)
    {
        _authService = authService;
        _currentUser = currentUser;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting("AuthPolicy")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var result = await _authService.RegisterAsync(request, ct);
        return result.ErrorType switch
        {
            ResultErrorType.Validation => ValidationProblem(
                new ValidationProblemDetails(new Dictionary<string, string[]>(StringComparer.Ordinal) { [""] = new[] { result.Error! } })),
            ResultErrorType.Conflict => Conflict(new { error = result.Error }),
            _ when !result.IsSuccess => Problem(result.Error),
            _ => CreatedAtAction(nameof(Me), null, result.Value)
        };
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("AuthPolicy")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await _authService.LoginAsync(request, ct);
        return result.ErrorType switch
        {
            ResultErrorType.Validation => ValidationProblem(
                new ValidationProblemDetails(new Dictionary<string, string[]>(StringComparer.Ordinal) { [""] = new[] { result.Error! } })),
            ResultErrorType.Unauthorized => Unauthorized(new { error = result.Error }),
            _ when !result.IsSuccess => Problem(result.Error),
            _ => Ok(result.Value)
        };
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var userId = _currentUser.UserId;
        if (!userId.HasValue)
            return Unauthorized();

        var result = await _authService.GetCurrentUserAsync(userId.Value, ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound();
    }
}
