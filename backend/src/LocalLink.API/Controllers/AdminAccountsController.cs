using LocalLink.Application.Accounts.DTOs;
using LocalLink.Application.Accounts.Interfaces;
using LocalLink.Application.Auth.Interfaces;
using LocalLink.Application.Common.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocalLink.API.Controllers;

[ApiController]
[Route("api/v1/admin/accounts")]
[Authorize(Roles = "ADMIN")]
public class AdminAccountsController : ControllerBase
{
    private readonly IAdminAccountService _adminAccountService;
    private readonly ICurrentUserService _currentUserService;

    public AdminAccountsController(
        IAdminAccountService adminAccountService,
        ICurrentUserService currentUserService)
    {
        _adminAccountService = adminAccountService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Update bank account status (e.g. LOCKED or ACTIVE). Requires ADMIN role.
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(AccountSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAccountStatus(
        Guid id,
        [FromBody] UpdateAccountStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        var result = await _adminAccountService.UpdateAccountStatusAsync(
            id,
            request,
            _currentUserService.UserId.Value,
            _currentUserService.IpAddress,
            cancellationToken);

        return Ok(result);
    }
}
