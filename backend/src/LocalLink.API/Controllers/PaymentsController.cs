using LocalLink.Application.Auth.Interfaces;
using LocalLink.Application.Common.Models;
using LocalLink.Application.Payments.DTOs;
using LocalLink.Application.Payments.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocalLink.API.Controllers;

[ApiController]
[Route("api/v1/payments")]
[Authorize(Roles = "CUSTOMER")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ICurrentUserService _currentUserService;

    public PaymentsController(IPaymentService paymentService, ICurrentUserService currentUserService)
    {
        _paymentService = paymentService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Executes a bill payment with server-controlled amount, atomic transaction, and idempotency protection.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PaymentReceiptDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PaymentReceiptDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> PayBill(
        [FromBody] CreatePaymentRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            return Unauthorized();
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _paymentService.PayBillAsync(
            request,
            idempotencyKey,
            _currentUserService.UserId.Value,
            ipAddress,
            cancellationToken);

        return CreatedAtAction(nameof(GetPaymentDetail), new { id = result.PaymentId }, result);
    }

    /// <summary>
    /// Retrieves a paginated list of bill payments made by the current customer.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<PaymentListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPayments(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] DateOnly? fromDate = null,
        [FromQuery] DateOnly? toDate = null,
        [FromQuery] string? billType = null,
        CancellationToken cancellationToken = default)
    {
        if (_currentUserService.UserId == null)
        {
            return Unauthorized();
        }

        var result = await _paymentService.GetPaymentsAsync(
            _currentUserService.UserId.Value,
            page,
            pageSize,
            status,
            fromDate,
            toDate,
            billType,
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Retrieves details of a specific payment receipt owned by the current customer.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PaymentDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPaymentDetail(Guid id, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            return Unauthorized();
        }

        var result = await _paymentService.GetPaymentDetailAsync(id, _currentUserService.UserId.Value, cancellationToken);
        return Ok(result);
    }
}
