using LocalLink.Application.Auth.Interfaces;
using LocalLink.Application.Cards.DTOs;
using LocalLink.Application.Cards.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocalLink.API.Controllers;

[ApiController]
[Route("api/v1/cards")]
[Authorize(Roles = "CUSTOMER")]
public class CardsController : ControllerBase
{
    private readonly ICardService _cardService;
    private readonly ICurrentUserService _currentUserService;

    public CardsController(ICardService cardService, ICurrentUserService currentUserService)
    {
        _cardService = cardService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CardDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyCards(CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            return Unauthorized();
        }

        var cards = await _cardService.GetMyCardsAsync(_currentUserService.UserId.Value, cancellationToken);
        return Ok(cards);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCard(Guid id, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            return Unauthorized();
        }

        var card = await _cardService.GetMyCardAsync(_currentUserService.UserId.Value, id, cancellationToken);
        return Ok(card);
    }

    [HttpPost("{id:guid}/lock")]
    [ProducesResponseType(typeof(CardDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> LockCard(Guid id, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            return Unauthorized();
        }

        var card = await _cardService.LockCardAsync(_currentUserService.UserId.Value, id, cancellationToken);
        return Ok(card);
    }

    [HttpPost("{id:guid}/unlock")]
    [ProducesResponseType(typeof(CardDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UnlockCard(Guid id, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            return Unauthorized();
        }

        var card = await _cardService.UnlockCardAsync(_currentUserService.UserId.Value, id, cancellationToken);
        return Ok(card);
    }

    [HttpPatch("{id:guid}/limits")]
    [ProducesResponseType(typeof(CardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateLimits(Guid id, [FromBody] UpdateCardLimitsRequest request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            return Unauthorized();
        }

        var card = await _cardService.UpdateLimitsAsync(_currentUserService.UserId.Value, id, request, cancellationToken);
        return Ok(card);
    }

    [HttpPatch("{id:guid}/settings")]
    [ProducesResponseType(typeof(CardDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateSettings(Guid id, [FromBody] UpdateCardSettingsRequest request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            return Unauthorized();
        }

        var card = await _cardService.UpdateSettingsAsync(_currentUserService.UserId.Value, id, request, cancellationToken);
        return Ok(card);
    }
}
