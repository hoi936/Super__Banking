using LocalLink.Application.Accounts.DTOs;

namespace LocalLink.Application.Customers.DTOs;

public class AdminCustomerDetailDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string CustomerCode { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public DateOnly? DateOfBirth { get; init; }
    public string? Gender { get; init; }
    public string? PhoneNumber { get; init; }
    public string? Address { get; init; }
    public string CustomerStatus { get; init; } = string.Empty;
    public string UserStatus { get; init; } = string.Empty;
    public IReadOnlyList<string> Roles { get; init; } = Array.Empty<string>();
    public IReadOnlyList<AccountSummaryDto> Accounts { get; init; } = Array.Empty<AccountSummaryDto>();
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
}
