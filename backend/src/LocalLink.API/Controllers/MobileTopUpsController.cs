using LocalLink.Application.Auth.Interfaces;
using LocalLink.Application.Common.Exceptions;
using LocalLink.Application.Common.Models;
using LocalLink.Application.MobileTopUps.DTOs;
using LocalLink.Application.MobileTopUps.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocalLink.API.Controllers;

[ApiController]
[Route("api/v1/mobile-topups")]
[Authorize(Roles = "CUSTOMER")]
public class MobileTopUpsController : ControllerBase
{
    private readonly IMobileTopUpService _mobileTopUpService;
    private readonly ICurrentUserService _currentUserService;

    public MobileTopUpsController(IMobileTopUpService mobileTopUpService, ICurrentUserService currentUserService)
    {
        _mobileTopUpService = mobileTopUpService;
        _currentUserService = currentUserService;
    }

    [HttpGet("providers")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<MobileProviderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProviders(CancellationToken cancellationToken)
    {
        var providers = await _mobileTopUpService.GetProvidersAsync(cancellationToken);
        return Ok(providers);
    }

    [HttpGet("purchases")]
    [ProducesResponseType(typeof(PagedResult<MobileTopUpListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyPurchases(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        if (_currentUserService.UserId == null)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        var result = await _mobileTopUpService.GetMyPurchasesAsync(
            _currentUserService.UserId.Value,
            page,
            pageSize,
            status,
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("purchases")]
    [ProducesResponseType(typeof(MobileTopUpReceiptDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Purchase(
        [FromBody] CreateMobileTopUpRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        var result = await _mobileTopUpService.PurchaseAsync(
            request,
            idempotencyKey,
            _currentUserService.UserId.Value,
            _currentUserService.IpAddress,
            cancellationToken);

        return CreatedAtAction(nameof(GetMyPurchases), new { id = result.MobileTopUpId }, result);
    }
}
