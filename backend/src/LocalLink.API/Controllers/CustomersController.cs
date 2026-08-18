using LocalLink.Application.Auth.Interfaces;
using LocalLink.Application.Common.Exceptions;
using LocalLink.Application.Customers.DTOs;
using LocalLink.Application.Customers.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocalLink.API.Controllers;

[ApiController]
[Route("api/v1/customers")]
[Authorize(Roles = "CUSTOMER")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly ICurrentUserService _currentUserService;

    public CustomersController(ICustomerService customerService, ICurrentUserService currentUserService)
    {
        _customerService = customerService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Get the profile of the currently authenticated customer
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(CustomerProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        var profile = await _customerService.GetMyProfileAsync(_currentUserService.UserId.Value, cancellationToken);
        return Ok(profile);
    }

    /// <summary>
    /// Update personal profile information of the currently authenticated customer
    /// </summary>
    [HttpPut("me")]
    [ProducesResponseType(typeof(CustomerProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateCustomerProfileRequest request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        var updatedProfile = await _customerService.UpdateMyProfileAsync(_currentUserService.UserId.Value, request, cancellationToken);
        return Ok(updatedProfile);
    }
}
