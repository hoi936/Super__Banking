using LocalLink.Application.Auth.Interfaces;
using LocalLink.Application.Beneficiaries.DTOs;
using LocalLink.Application.Beneficiaries.Interfaces;
using LocalLink.Application.Common.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocalLink.API.Controllers;

[ApiController]
[Route("api/v1/beneficiaries")]
[Authorize(Roles = "CUSTOMER")]
public class BeneficiariesController : ControllerBase
{
    private readonly IBeneficiaryService _beneficiaryService;
    private readonly ICurrentUserService _currentUserService;

    public BeneficiariesController(IBeneficiaryService beneficiaryService, ICurrentUserService currentUserService)
    {
        _beneficiaryService = beneficiaryService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Get list of beneficiaries saved by the currently authenticated customer
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<BeneficiaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyBeneficiaries(CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        var list = await _beneficiaryService.GetMyBeneficiariesAsync(_currentUserService.UserId.Value, cancellationToken);
        return Ok(list);
    }

    /// <summary>
    /// Add a new beneficiary bank account to the customer's directory
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(BeneficiaryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddBeneficiary([FromBody] CreateBeneficiaryRequest request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        var created = await _beneficiaryService.AddBeneficiaryAsync(_currentUserService.UserId.Value, request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, created);
    }

    /// <summary>
    /// Delete a beneficiary from the customer's directory
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBeneficiary(Guid id, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        await _beneficiaryService.DeleteBeneficiaryAsync(_currentUserService.UserId.Value, id, cancellationToken);
        return Ok(new { message = "Beneficiary deleted successfully." });
    }
}
