using LocalLink.Application.Cards.DTOs;
using LocalLink.Application.Cards.Interfaces;
using LocalLink.Application.Common.Exceptions;
using LocalLink.Domain.Entities;
using LocalLink.Domain.Enums;
using LocalLink.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LocalLink.Infrastructure.Services;

public class CardService : ICardService
{
    private readonly ApplicationDbContext _context;

    public CardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CardDto>> GetMyCardsAsync(Guid currentUserId, CancellationToken cancellationToken = default)
    {
        var customer = await GetCustomerAsync(currentUserId, cancellationToken);

        var cards = await _context.Cards
            .AsNoTracking()
            .Include(card => card.LinkedAccount)
            .Where(card => card.CustomerId == customer.Id)
            .OrderBy(card => card.CardType)
            .ThenBy(card => card.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return cards.Select(ToDto).ToList();
    }

    public async Task<CardDto> GetMyCardAsync(Guid currentUserId, Guid cardId, CancellationToken cancellationToken = default)
    {
        var card = await GetOwnedCardAsync(currentUserId, cardId, false, cancellationToken);
        return ToDto(card);
    }

    public async Task<CardDto> LockCardAsync(Guid currentUserId, Guid cardId, CancellationToken cancellationToken = default)
    {
        var card = await GetOwnedCardAsync(currentUserId, cardId, true, cancellationToken);

        if (card.Status is CardStatus.Blocked or CardStatus.Expired or CardStatus.Cancelled)
        {
            throw new BadRequestException($"Card is {card.Status} and cannot be locked.");
        }

        card.Status = CardStatus.Locked;
        card.UpdatedAtUtc = DateTime.UtcNow;
        AddAuditAndNotification(currentUserId, card, "CARD_LOCKED", "The card was locked by the customer.");

        await _context.SaveChangesAsync(cancellationToken);
        return ToDto(card);
    }

    public async Task<CardDto> UnlockCardAsync(Guid currentUserId, Guid cardId, CancellationToken cancellationToken = default)
    {
        var card = await GetOwnedCardAsync(currentUserId, cardId, true, cancellationToken);

        if (card.Status != CardStatus.Locked)
        {
            throw new BadRequestException("Only locked cards can be unlocked by the customer.");
        }

        card.Status = CardStatus.Active;
        card.UpdatedAtUtc = DateTime.UtcNow;
        AddAuditAndNotification(currentUserId, card, "CARD_UNLOCKED", "The card was unlocked by the customer.");

        await _context.SaveChangesAsync(cancellationToken);
        return ToDto(card);
    }

    public async Task<CardDto> UpdateLimitsAsync(Guid currentUserId, Guid cardId, UpdateCardLimitsRequest request, CancellationToken cancellationToken = default)
    {
        if (request.MonthlyLimit < request.DailyLimit)
        {
            throw new BadRequestException("Monthly limit must be greater than or equal to daily limit.");
        }

        var card = await GetOwnedCardAsync(currentUserId, cardId, true, cancellationToken);

        if (card.Status is CardStatus.Blocked or CardStatus.Expired or CardStatus.Cancelled)
        {
            throw new BadRequestException($"Card is {card.Status} and cannot be updated.");
        }

        card.DailyLimit = request.DailyLimit;
        card.MonthlyLimit = request.MonthlyLimit;
        card.UpdatedAtUtc = DateTime.UtcNow;
        AddAuditAndNotification(currentUserId, card, "CARD_LIMITS_UPDATED", $"Daily limit: {request.DailyLimit:N0}, monthly limit: {request.MonthlyLimit:N0}.");

        await _context.SaveChangesAsync(cancellationToken);
        return ToDto(card);
    }

    public async Task<CardDto> UpdateSettingsAsync(Guid currentUserId, Guid cardId, UpdateCardSettingsRequest request, CancellationToken cancellationToken = default)
    {
        var card = await GetOwnedCardAsync(currentUserId, cardId, true, cancellationToken);

        if (card.Status is CardStatus.Blocked or CardStatus.Expired or CardStatus.Cancelled)
        {
            throw new BadRequestException($"Card is {card.Status} and cannot be updated.");
        }

        card.OnlinePaymentEnabled = request.OnlinePaymentEnabled;
        card.ContactlessEnabled = request.ContactlessEnabled;
        card.UpdatedAtUtc = DateTime.UtcNow;
        AddAuditAndNotification(currentUserId, card, "CARD_SETTINGS_UPDATED", "Card channel settings were updated.");

        await _context.SaveChangesAsync(cancellationToken);
        return ToDto(card);
    }

    private async Task<Customer> GetCustomerAsync(Guid currentUserId, CancellationToken cancellationToken)
    {
        return await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == currentUserId, cancellationToken)
            ?? throw new NotFoundException("Customer", currentUserId);
    }

    private async Task<BankCard> GetOwnedCardAsync(Guid currentUserId, Guid cardId, bool tracking, CancellationToken cancellationToken)
    {
        var customer = await GetCustomerAsync(currentUserId, cancellationToken);

        var query = _context.Cards
            .Include(card => card.LinkedAccount)
            .AsQueryable();

        if (!tracking)
        {
            query = query.AsNoTracking();
        }

        var card = await query.FirstOrDefaultAsync(card => card.Id == cardId && card.CustomerId == customer.Id, cancellationToken);
        return card ?? throw new NotFoundException("Card", cardId);
    }

    private void AddAuditAndNotification(Guid currentUserId, BankCard card, string action, string description)
    {
        var now = DateTime.UtcNow;
        _context.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = currentUserId,
            Action = action,
            EntityType = "Card",
            EntityId = card.Id.ToString(),
            Description = $"{description} Card ending {card.LastFourDigits}.",
            CreatedAtUtc = now
        });

        _context.Notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            UserId = currentUserId,
            Title = "Cap nhat the",
            Message = $"The ket thuc {card.LastFourDigits} vua duoc cap nhat.",
            Type = NotificationType.Security,
            IsRead = false,
            CreatedAtUtc = now
        });
    }

    private static CardDto ToDto(BankCard card)
    {
        return new CardDto
        {
            Id = card.Id,
            LinkedAccountId = card.LinkedAccountId,
            LinkedAccountNumber = card.LinkedAccount.AccountNumber,
            CardNumberMasked = card.CardNumberMasked,
            LastFourDigits = card.LastFourDigits,
            CardholderName = card.CardholderName,
            CardType = card.CardType.ToString().ToUpperInvariant(),
            Status = card.Status.ToString().ToUpperInvariant(),
            DailyLimit = card.DailyLimit,
            MonthlyLimit = card.MonthlyLimit,
            Currency = card.Currency,
            OnlinePaymentEnabled = card.OnlinePaymentEnabled,
            ContactlessEnabled = card.ContactlessEnabled,
            ExpiryMonth = card.ExpiryMonth,
            ExpiryYear = card.ExpiryYear,
            IssuedAtUtc = card.IssuedAtUtc
        };
    }
}
