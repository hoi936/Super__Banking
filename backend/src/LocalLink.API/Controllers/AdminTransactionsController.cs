using LocalLink.Application.Admin.DTOs;
using LocalLink.Application.Admin.Interfaces;
using LocalLink.Application.Common.Models;
using LocalLink.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocalLink.API.Controllers;

[ApiController]
[Route("api/v1/admin/transactions")]
[Authorize(Roles = "STAFF,ADMIN")]
public class AdminTransactionsController : ControllerBase
{
    private readonly IAdminTransactionService _adminTransactionService;

    public AdminTransactionsController(IAdminTransactionService adminTransactionService)
    {
        _adminTransactionService = adminTransactionService;
    }

    /// <summary>
    /// Get paginated and searchable list of transactions (Requires STAFF or ADMIN role)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AdminTransactionListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] TransactionType? type = null,
        [FromQuery] TransactionStatus? status = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? accountNumber = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _adminTransactionService.GetTransactionsAsync(
            page, pageSize, search, type, status, fromDate, toDate, accountNumber, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get comprehensive transaction detail (Requires STAFF or ADMIN role)
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AdminTransactionDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTransactionDetail(Guid id, CancellationToken cancellationToken)
    {
        var detail = await _adminTransactionService.GetTransactionDetailAsync(id, cancellationToken);
        return Ok(detail);
    }
}
