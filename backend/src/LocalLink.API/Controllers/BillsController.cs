using LocalLink.Application.Auth.Interfaces;
using LocalLink.Application.Bills.DTOs;
using LocalLink.Application.Bills.Interfaces;
using LocalLink.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocalLink.API.Controllers;

[ApiController]
[Route("api/v1/bills")]
[Authorize(Roles = "CUSTOMER")]
public class BillsController : ControllerBase
{
    private readonly IBillService _billService;
    private readonly ICurrentUserService _currentUserService;

    public BillsController(IBillService billService, ICurrentUserService currentUserService)
    {
        _billService = billService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Retrieves a paginated list of utility bills for the current customer.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<BillListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBills(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] string? type = null,
        [FromQuery] DateOnly? fromDueDate = null,
        [FromQuery] DateOnly? toDueDate = null,
        CancellationToken cancellationToken = default)
    {
        if (_currentUserService.UserId == null)
        {
            return Unauthorized();
        }

        var result = await _billService.GetBillsAsync(
            _currentUserService.UserId.Value,
            page,
            pageSize,
            status,
            type,
            fromDueDate,
            toDueDate,
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Retrieves details of a specific bill owned by the current customer.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BillDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBillDetail(Guid id, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            return Unauthorized();
        }

        var result = await _billService.GetBillDetailAsync(id, _currentUserService.UserId.Value, cancellationToken);
        return Ok(result);
    }
}
