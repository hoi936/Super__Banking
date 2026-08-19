using LocalLink.Application.Auth.Interfaces;
using LocalLink.Application.Common.Exceptions;
using LocalLink.Application.Common.Models;
using LocalLink.Application.Loans.DTOs;
using LocalLink.Application.Loans.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocalLink.API.Controllers;

[ApiController]
[Route("api/v1/admin/loans")]
[Authorize(Roles = "STAFF,ADMIN")]
public class AdminLoansController : ControllerBase
{
    private readonly IAdminLoanService _adminLoanService;
    private readonly ICurrentUserService _currentUserService;

    public AdminLoansController(IAdminLoanService adminLoanService, ICurrentUserService currentUserService)
    {
        _adminLoanService = adminLoanService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<LoanApplicationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetApplications(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery] string? keyword = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _adminLoanService.GetApplicationsAsync(page, pageSize, status, keyword, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(LoanApplicationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetApplication(Guid id, CancellationToken cancellationToken)
    {
        var result = await _adminLoanService.GetApplicationAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(typeof(LoanApplicationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Approve(
        Guid id,
        [FromBody] ApproveLoanApplicationRequest request,
        CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        var result = await _adminLoanService.ApproveAsync(
            id,
            request,
            _currentUserService.UserId.Value,
            _currentUserService.IpAddress,
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(typeof(LoanApplicationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reject(
        Guid id,
        [FromBody] RejectLoanApplicationRequest request,
        CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        var result = await _adminLoanService.RejectAsync(
            id,
            request,
            _currentUserService.UserId.Value,
            _currentUserService.IpAddress,
            cancellationToken);

        return Ok(result);
    }
}
