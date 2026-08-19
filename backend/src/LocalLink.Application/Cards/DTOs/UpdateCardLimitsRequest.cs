using System.ComponentModel.DataAnnotations;

namespace LocalLink.Application.Cards.DTOs;

public class UpdateCardLimitsRequest
{
    [Range(100000, 500000000, ErrorMessage = "Daily limit must be between 100,000 and 500,000,000 VND.")]
    public decimal DailyLimit { get; set; }

    [Range(100000, 2000000000, ErrorMessage = "Monthly limit must be between 100,000 and 2,000,000,000 VND.")]
    public decimal MonthlyLimit { get; set; }
}
