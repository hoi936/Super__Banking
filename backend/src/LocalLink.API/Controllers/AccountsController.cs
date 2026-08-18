using LocalLink.Application.Accounts.DTOs;
using LocalLink.Application.Accounts.Interfaces;
using LocalLink.Application.Auth.Interfaces;
using LocalLink.Application.Common.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocalLink.API.Controllers;

[ApiController]
[Route("api/v1/accounts")]
[Authorize(Roles = "CUSTOMER")]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly ICurrentUserService _currentUserService;

    public AccountsController(IAccountService accountService, ICurrentUserService currentUserService)
    {
        _accountService = accountService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Get all bank accounts belonging strictly to the currently authenticated customer
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AccountSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyAccounts(CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        var accounts = await _accountService.GetMyAccountsAsync(_currentUserService.UserId.Value, cancellationToken);
        return Ok(accounts);
    }

    /// <summary>
    /// Get detailed information of a specific bank account owned by the authenticated customer
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AccountDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyAccountDetail(Guid id, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        var account = await _accountService.GetMyAccountDetailAsync(_currentUserService.UserId.Value, id, cancellationToken);
        return Ok(account);
    }

    /// <summary>
    /// Look up minimum public account information (Account Number and Holder Name) for transfer preparation
    /// </summary>
    [HttpGet("lookup/{accountNumber}")]
    [ProducesResponseType(typeof(AccountLookupDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LookupAccount(string accountNumber, CancellationToken cancellationToken)
    {
        var lookup = await _accountService.LookupAccountAsync(accountNumber, cancellationToken);
        return Ok(lookup);
    }
}
