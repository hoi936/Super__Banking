using LocalLink.Application.Auth.Interfaces;
using LocalLink.Application.Common.Exceptions;
using LocalLink.Application.QrPay.DTOs;
using LocalLink.Application.QrPay.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocalLink.API.Controllers;

[ApiController]
[Route("api/v1/qr-pay")]
[Authorize(Roles = "CUSTOMER")]
public class QrPayController : ControllerBase
{
    private readonly IQrPayService _qrPayService;
    private readonly ICurrentUserService _currentUserService;

    public QrPayController(IQrPayService qrPayService, ICurrentUserService currentUserService)
    {
        _qrPayService = qrPayService;
        _currentUserService = currentUserService;
    }

    [HttpPost("receive-payload")]
    [ProducesResponseType(typeof(QrPayPayloadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateReceivePayload(
        [FromBody] CreateQrPayloadRequest request,
        CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        var payload = await _qrPayService.CreateReceivePayloadAsync(
            _currentUserService.UserId.Value,
            request,
            cancellationToken);

        return Ok(payload);
    }

    [HttpPost("parse")]
    [ProducesResponseType(typeof(ParsedQrPayDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ParsePayload(
        [FromBody] ParseQrPayloadRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _qrPayService.ParsePayloadAsync(request, cancellationToken);
        return Ok(result);
    }
}
