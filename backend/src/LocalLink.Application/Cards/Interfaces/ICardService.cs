using LocalLink.Application.Cards.DTOs;

namespace LocalLink.Application.Cards.Interfaces;

public interface ICardService
{
    Task<IReadOnlyList<CardDto>> GetMyCardsAsync(Guid currentUserId, CancellationToken cancellationToken = default);
    Task<CardDto> GetMyCardAsync(Guid currentUserId, Guid cardId, CancellationToken cancellationToken = default);
    Task<CardDto> LockCardAsync(Guid currentUserId, Guid cardId, CancellationToken cancellationToken = default);
    Task<CardDto> UnlockCardAsync(Guid currentUserId, Guid cardId, CancellationToken cancellationToken = default);
    Task<CardDto> UpdateLimitsAsync(Guid currentUserId, Guid cardId, UpdateCardLimitsRequest request, CancellationToken cancellationToken = default);
    Task<CardDto> UpdateSettingsAsync(Guid currentUserId, Guid cardId, UpdateCardSettingsRequest request, CancellationToken cancellationToken = default);
}
