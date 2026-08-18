namespace LocalLink.Application.Auth.DTOs;

public class CurrentUserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public CustomerProfileDto? Customer { get; set; }
}
