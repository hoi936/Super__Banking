using LocalLink.Application.Admin.DTOs;
using LocalLink.Application.Admin.Interfaces;
using LocalLink.Application.Auth.Interfaces;
using LocalLink.Application.Common.Exceptions;
using LocalLink.Application.Common.Models;
using LocalLink.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocalLink.API.Controllers;

[ApiController]
[Route("api/v1/admin/users")]
[Authorize(Roles = "ADMIN")]
public class AdminUsersController : ControllerBase
{
    private readonly IAdminUserService _adminUserService;
    private readonly ICurrentUserService _currentUserService;

    public AdminUsersController(IAdminUserService adminUserService, ICurrentUserService currentUserService)
    {
        _adminUserService = adminUserService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Get paginated and searchable list of users (Requires ADMIN role)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AdminUserListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] UserStatus? status = null,
        [FromQuery] string? role = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _adminUserService.GetUsersAsync(page, pageSize, search, status, role, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get user detail (Requires ADMIN role)
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AdminUserDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserDetail(Guid id, CancellationToken cancellationToken)
    {
        var detail = await _adminUserService.GetUserDetailAsync(id, cancellationToken);
        return Ok(detail);
    }

    /// <summary>
    /// Update user status, potentially revoking tokens (Requires ADMIN role)
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(AdminUserDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUserStatus(
        Guid id, 
        [FromBody] UpdateUserStatusRequest request, 
        CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
            throw new UnauthorizedException("User is not authenticated.");

        var result = await _adminUserService.UpdateUserStatusAsync(
            id, request, _currentUserService.UserId.Value, _currentUserService.IpAddress, cancellationToken);
            
        return Ok(result);
    }
}
