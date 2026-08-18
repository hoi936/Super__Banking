namespace LocalLink.Application.Customers.DTOs;

public class AdminCustomerListItemDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string CustomerCode { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public string CustomerStatus { get; init; } = string.Empty;
    public string UserStatus { get; init; } = string.Empty;
    public int AccountsCount { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}
