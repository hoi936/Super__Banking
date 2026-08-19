using LocalLink.Application.Auth.Interfaces;
using LocalLink.Application.Common.Exceptions;
using LocalLink.Application.Loans.DTOs;
using LocalLink.Application.Loans.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocalLink.API.Controllers;

[ApiController]
[Route("api/v1/loans")]
[Authorize(Roles = "CUSTOMER")]
public class LoansController : ControllerBase
{
    private readonly ILoanService _loanService;
    private readonly ICurrentUserService _currentUserService;

    public LoansController(ILoanService loanService, ICurrentUserService currentUserService)
    {
        _loanService = loanService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<LoanApplicationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyApplications([FromQuery] string? status, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        var applications = await _loanService.GetMyApplicationsAsync(
            _currentUserService.UserId.Value,
            status,
            cancellationToken);

        return Ok(applications);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(LoanApplicationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyApplication(Guid id, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        var application = await _loanService.GetMyApplicationAsync(
            _currentUserService.UserId.Value,
            id,
            cancellationToken);

        return Ok(application);
    }

    [HttpPost]
    [ProducesResponseType(typeof(LoanApplicationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateApplication(
        [FromBody] CreateLoanApplicationRequest request,
        CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        var application = await _loanService.CreateApplicationAsync(
            _currentUserService.UserId.Value,
            request,
            cancellationToken);

        return CreatedAtAction(nameof(GetMyApplication), new { id = application.Id }, application);
    }
}
