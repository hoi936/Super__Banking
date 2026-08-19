using LocalLink.Application.Auth.Interfaces;
using LocalLink.Application.Common.Exceptions;
using LocalLink.Application.TermDeposits.DTOs;
using LocalLink.Application.TermDeposits.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocalLink.API.Controllers;

[ApiController]
[Route("api/v1/term-deposits")]
public class TermDepositsController : ControllerBase
{
    private readonly ITermDepositService _termDepositService;
    private readonly ICurrentUserService _currentUserService;

    public TermDepositsController(ITermDepositService termDepositService, ICurrentUserService currentUserService)
    {
        _termDepositService = termDepositService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    [Authorize(Roles = "CUSTOMER")]
    [ProducesResponseType(typeof(IReadOnlyList<TermDepositDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyTermDeposits([FromQuery] string? status, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        var result = await _termDepositService.GetMyTermDepositsAsync(
            _currentUserService.UserId.Value,
            status,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "CUSTOMER")]
    [ProducesResponseType(typeof(TermDepositDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyTermDepositDetail(Guid id, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        var result = await _termDepositService.GetMyTermDepositDetailAsync(
            _currentUserService.UserId.Value,
            id,
            cancellationToken);

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "CUSTOMER")]
    [ProducesResponseType(typeof(TermDepositReceiptDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> OpenTermDeposit(
        [FromBody] CreateTermDepositRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _termDepositService.OpenTermDepositAsync(
            request,
            idempotencyKey,
            _currentUserService.UserId.Value,
            ipAddress,
            cancellationToken);

        return CreatedAtAction(nameof(GetMyTermDepositDetail), new { id = result.TermDepositId }, result);
    }

    [HttpPost("mature-due")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> MatureDueTermDeposits(CancellationToken cancellationToken)
    {
        var maturedCount = await _termDepositService.MatureDueTermDepositsAsync(cancellationToken);
        return Ok(new { maturedCount });
    }
}
