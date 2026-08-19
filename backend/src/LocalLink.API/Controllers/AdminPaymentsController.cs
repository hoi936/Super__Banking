using LocalLink.Application.Admin.DTOs;
using LocalLink.Application.Admin.Interfaces;
using LocalLink.Application.Common.Models;
using LocalLink.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocalLink.API.Controllers;

[ApiController]
[Route("api/v1/admin/payments")]
[Authorize(Roles = "STAFF,ADMIN")]
public class AdminPaymentsController : ControllerBase
{
    private readonly IAdminPaymentService _adminPaymentService;

    public AdminPaymentsController(IAdminPaymentService adminPaymentService)
    {
        _adminPaymentService = adminPaymentService;
    }

    /// <summary>
    /// Get paginated and searchable list of payments (Requires STAFF or ADMIN role)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AdminPaymentListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPayments(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] PaymentStatus? status = null,
        [FromQuery] BillType? billType = null,
        [FromQuery] string? reference = null,
        [FromQuery] string? accountNumber = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _adminPaymentService.GetPaymentsAsync(
            page, pageSize, status, billType, reference, accountNumber, fromDate, toDate, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get comprehensive payment detail (Requires STAFF or ADMIN role)
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AdminPaymentDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPaymentDetail(Guid id, CancellationToken cancellationToken)
    {
        var detail = await _adminPaymentService.GetPaymentDetailAsync(id, cancellationToken);
        return Ok(detail);
    }
}
