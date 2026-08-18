using LocalLink.Application.Auth.Interfaces;
using LocalLink.Application.Common.Models;
using LocalLink.Application.Transactions.DTOs;
using LocalLink.Application.Transactions.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocalLink.API.Controllers;

[ApiController]
[Route("api/v1/transactions")]
[Authorize(Roles = "CUSTOMER")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;
    private readonly ICurrentUserService _currentUserService;

    public TransactionsController(ITransactionService transactionService, ICurrentUserService currentUserService)
    {
        _transactionService = transactionService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Retrieves a paginated list of transaction ledger records for the authenticated customer.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TransactionListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyTransactions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? accountId = null,
        [FromQuery] string? type = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        if (_currentUserService.UserId == null)
        {
            return Unauthorized();
        }

        var result = await _transactionService.GetMyTransactionsAsync(
            _currentUserService.UserId.Value,
            page,
            pageSize,
            accountId,
            type,
            fromDate,
            toDate,
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Retrieves details of a specific transaction ledger record owned by the authenticated customer.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TransactionDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTransactionDetail(Guid id, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            return Unauthorized();
        }

        var result = await _transactionService.GetTransactionDetailAsync(id, _currentUserService.UserId.Value, cancellationToken);
        return Ok(result);
    }
}
