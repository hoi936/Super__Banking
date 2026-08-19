using System.Net;
using LocalLink.Application.Common.Models;
using LocalLink.Application.PaymentGateway.DTOs;
using LocalLink.Application.PaymentGateway.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocalLink.API.Controllers;

[ApiController]
[Route("api/v1/payment-gateway")]
public class PaymentGatewayController : ControllerBase
{
    private readonly IPaymentGatewayService _gatewayService;

    public PaymentGatewayController(IPaymentGatewayService gatewayService)
    {
        _gatewayService = gatewayService;
    }

    [Authorize]
    [HttpPost("deposit")]
    [ProducesResponseType(typeof(GatewayDepositReceiptDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> CreateDeposit([FromBody] CreateGatewayDepositRequest request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!);
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        var result = await _gatewayService.CreateDepositAsync(request, userId, ipAddress, cancellationToken);
        return Ok(result);
    }

    // Webhook callback endpoint - intentionally not authorized to simulate external calls
    [HttpPost("callback")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> HandleCallback([FromBody] GatewayCallbackRequest request, CancellationToken cancellationToken)
    {
        var result = await _gatewayService.HandleCallbackAsync(request, cancellationToken);
        if (result)
        {
            return Ok();
        }
        
        return BadRequest();
    }
}
