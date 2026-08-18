using LocalLink.Application.Accounts.DTOs;
using LocalLink.Application.Accounts.Interfaces;
using LocalLink.Application.Auth.Interfaces;
using LocalLink.Application.Common.Exceptions;
using LocalLink.Application.Common.Models;
using LocalLink.Application.Customers.DTOs;
using LocalLink.Application.Customers.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocalLink.API.Controllers;

[ApiController]
[Route("api/v1/admin/customers")]
public class AdminCustomersController : ControllerBase
{
    private readonly IAdminCustomerService _adminCustomerService;
    private readonly IAdminAccountService _adminAccountService;
    private readonly ICurrentUserService _currentUserService;

    public AdminCustomersController(
        IAdminCustomerService adminCustomerService,
        IAdminAccountService adminAccountService,
        ICurrentUserService currentUserService)
    {
        _adminCustomerService = adminCustomerService;
        _adminAccountService = adminAccountService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Get paginated and searchable list of customers (Requires STAFF or ADMIN role)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "STAFF,ADMIN")]
    [ProducesResponseType(typeof(PagedResult<AdminCustomerListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetCustomers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _adminCustomerService.GetCustomersAsync(page, pageSize, search, status, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get comprehensive customer details with roles and accounts summary (Requires STAFF or ADMIN role)
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "STAFF,ADMIN")]
    [ProducesResponseType(typeof(AdminCustomerDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCustomerDetail(Guid id, CancellationToken cancellationToken)
    {
        var detail = await _adminCustomerService.GetCustomerDetailAsync(id, cancellationToken);
        return Ok(detail);
    }

    /// <summary>
    /// Get list of accounts for a specific customer (Requires STAFF or ADMIN role)
    /// </summary>
    [HttpGet("{id:guid}/accounts")]
    [Authorize(Roles = "STAFF,ADMIN")]
    [ProducesResponseType(typeof(IReadOnlyList<AccountSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCustomerAccounts(Guid id, CancellationToken cancellationToken)
    {
        var accounts = await _adminAccountService.GetCustomerAccountsAsync(id, cancellationToken);
        return Ok(accounts);
    }

    /// <summary>
    /// Update customer status (e.g. SUSPENDED or ACTIVE). Requires ADMIN role.
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(typeof(CustomerProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCustomerStatus(
        Guid id, 
        [FromBody] UpdateCustomerStatusRequest request, 
        CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        var result = await _adminCustomerService.UpdateCustomerStatusAsync(
            id, 
            request, 
            _currentUserService.UserId.Value, 
            _currentUserService.IpAddress, 
            cancellationToken);

        return Ok(result);
    }
}
