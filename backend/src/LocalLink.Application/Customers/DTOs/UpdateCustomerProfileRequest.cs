namespace LocalLink.Application.Customers.DTOs;

public class UpdateCustomerProfileRequest
{
    public string FullName { get; init; } = string.Empty;
    public DateOnly? DateOfBirth { get; init; }
    public string? Gender { get; init; }
    public string? PhoneNumber { get; init; }
    public string? Address { get; init; }
}
