using LocalLink.Application.Auth.Interfaces;
using LocalLink.Application.Common.Models;
using LocalLink.Application.Transfers.DTOs;
using LocalLink.Application.Transfers.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocalLink.API.Controllers;

[ApiController]
[Route("api/v1/transfers")]
[Authorize(Roles = "CUSTOMER")]
public class TransfersController : ControllerBase
{
    private readonly ITransferService _transferService;
    private readonly ICurrentUserService _currentUserService;

    public TransfersController(ITransferService transferService, ICurrentUserService currentUserService)
    {
        _transferService = transferService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Executes an internal bank transfer with idempotency protection and atomic financial integrity.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TransferReceiptDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(TransferReceiptDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Transfer(
        [FromBody] CreateTransferRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            return Unauthorized();
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _transferService.TransferAsync(
            request,
            idempotencyKey,
            _currentUserService.UserId.Value,
            ipAddress,
            cancellationToken);

        return CreatedAtAction(nameof(GetTransferDetail), new { id = result.TransferId }, result);
    }

    /// <summary>
    /// Retrieves a paginated list of internal transfers for the authenticated customer.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TransferListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyTransfers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] Guid? accountId = null,
        CancellationToken cancellationToken = default)
    {
        if (_currentUserService.UserId == null)
        {
            return Unauthorized();
        }

        var result = await _transferService.GetMyTransfersAsync(
            _currentUserService.UserId.Value,
            page,
            pageSize,
            status,
            fromDate,
            toDate,
            accountId,
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Retrieves details of a specific transfer receipt owned by the authenticated customer.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TransferReceiptDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTransferDetail(Guid id, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            return Unauthorized();
        }

        var result = await _transferService.GetTransferDetailAsync(id, _currentUserService.UserId.Value, cancellationToken);
        return Ok(result);
    }
}
