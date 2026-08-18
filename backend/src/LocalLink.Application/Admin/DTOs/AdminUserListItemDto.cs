using LocalLink.Domain.Enums;

namespace LocalLink.Application.Admin.DTOs;

public class AdminUserListItemDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public UserStatus Status { get; set; }
    public List<string> Roles { get; set; } = new();
    public string? CustomerCode { get; set; }
    public string? FullName { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
