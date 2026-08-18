namespace LocalLink.Application.Customers.DTOs;

public class CustomerProfileDto
{
    public Guid Id { get; init; }
    public string CustomerCode { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public DateOnly? DateOfBirth { get; init; }
    public string? Gender { get; init; }
    public string? PhoneNumber { get; init; }
    public string? Address { get; init; }
    public string Status { get; init; } = string.Empty;
}
