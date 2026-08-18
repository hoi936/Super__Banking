using System.ComponentModel.DataAnnotations;

namespace LocalLink.Application.Payments.DTOs;

public class CreatePaymentRequest
{
    [Required]
    public Guid BillId { get; set; }

    [Required]
    public Guid AccountId { get; set; }
}
