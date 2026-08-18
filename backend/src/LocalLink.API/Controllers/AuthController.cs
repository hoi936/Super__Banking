using LocalLink.Application.Auth.DTOs;
using LocalLink.Application.Auth.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocalLink.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUserService;

    public AuthController(IAuthService authService, ICurrentUserService currentUserService)
    {
        _authService = authService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Authenticates a user and returns an access token and refresh token.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var response = await _authService.LoginAsync(request, _currentUserService.IpAddress, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Rotates a refresh token and generates a new access and refresh token pair.
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var response = await _authService.RefreshTokenAsync(request, _currentUserService.IpAddress, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Logs out the currently authenticated user and revokes the active refresh token.
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest? request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Unauthorized();
        }

        await _authService.LogoutAsync(request ?? new LogoutRequest(), _currentUserService.UserId.Value, _currentUserService.IpAddress, cancellationToken);
        return Ok(new { message = "Logged out successfully." });
    }

    /// <summary>
    /// Returns the profile and roles of the currently authenticated user.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(CurrentUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Unauthorized();
        }

        var currentUser = await _authService.GetCurrentUserAsync(_currentUserService.UserId.Value, cancellationToken);
        return Ok(currentUser);
    }

    /// <summary>
    /// Development verification endpoint requiring CUSTOMER role.
    /// </summary>
    [HttpGet("test/customer")]
    [Authorize(Roles = "CUSTOMER")]
    public IActionResult TestCustomerAccess()
    {
        return Ok(new
        {
            message = "Authorized as CUSTOMER role.",
            email = _currentUserService.Email,
            roles = _currentUserService.Roles
        });
    }

    /// <summary>
    /// Development verification endpoint requiring STAFF role.
    /// </summary>
    [HttpGet("test/staff")]
    [Authorize(Roles = "STAFF")]
    public IActionResult TestStaffAccess()
    {
        return Ok(new
        {
            message = "Authorized as STAFF role.",
            email = _currentUserService.Email,
            roles = _currentUserService.Roles
        });
    }

    /// <summary>
    /// Development verification endpoint requiring ADMIN role.
    /// </summary>
    [HttpGet("test/admin")]
    [Authorize(Roles = "ADMIN")]
    public IActionResult TestAdminAccess()
    {
        return Ok(new
        {
            message = "Authorized as ADMIN role.",
            email = _currentUserService.Email,
            roles = _currentUserService.Roles
        });
    }
}
