using LocalLink.Application.Auth.Interfaces;
using LocalLink.Application.Common.Models;
using LocalLink.Application.Napas.DTOs;
using LocalLink.Application.Napas.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocalLink.API.Controllers;

[ApiController]
[Route("api/v1/napas")]
[Authorize(Roles = "CUSTOMER")]
public class NapasController : ControllerBase
{
    private readonly INapasTransferService _napasTransferService;
    private readonly ICurrentUserService _currentUserService;

    public NapasController(INapasTransferService napasTransferService, ICurrentUserService currentUserService)
    {
        _napasTransferService = napasTransferService;
        _currentUserService = currentUserService;
    }

    [HttpGet("banks")]
    [ProducesResponseType(typeof(IReadOnlyList<NapasBankDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBanks(CancellationToken cancellationToken)
    {
        var banks = await _napasTransferService.GetBanksAsync(cancellationToken);
        return Ok(banks);
    }

    [HttpPost("lookup")]
    [ProducesResponseType(typeof(NapasLookupResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Lookup([FromBody] NapasLookupRequest request, CancellationToken cancellationToken)
    {
        var result = await _napasTransferService.LookupAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("transfers")]
    [ProducesResponseType(typeof(NapasTransferReceiptDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Transfer(
        [FromBody] CreateNapasTransferRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            return Unauthorized();
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _napasTransferService.TransferAsync(
            request,
            idempotencyKey,
            _currentUserService.UserId.Value,
            ipAddress,
            cancellationToken);

        return CreatedAtAction(nameof(GetMyTransfers), new { id = result.ExternalTransferId }, result);
    }

    [HttpGet("transfers")]
    [ProducesResponseType(typeof(PagedResult<NapasTransferListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyTransfers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        if (_currentUserService.UserId == null)
        {
            return Unauthorized();
        }

        var result = await _napasTransferService.GetMyTransfersAsync(
            _currentUserService.UserId.Value,
            page,
            pageSize,
            status,
            cancellationToken);

        return Ok(result);
    }
}
