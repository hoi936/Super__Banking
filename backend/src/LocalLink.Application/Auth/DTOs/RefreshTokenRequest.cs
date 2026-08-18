using System.ComponentModel.DataAnnotations;

namespace LocalLink.Application.Auth.DTOs;

public class RefreshTokenRequest
{
    [Required(ErrorMessage = "RefreshToken is required.")]
    public string RefreshToken { get; set; } = string.Empty;
}
