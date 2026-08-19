using System.ComponentModel.DataAnnotations;
using LocalLink.Domain.Enums;

namespace LocalLink.Application.Admin.DTOs;

public class UpdateUserStatusRequest
{
    [Required]
    public UserStatus Status { get; set; }

    public string? Reason { get; set; }
}
