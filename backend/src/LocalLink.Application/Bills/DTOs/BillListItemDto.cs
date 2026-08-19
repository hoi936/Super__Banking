namespace LocalLink.Application.Bills.DTOs;

public class BillListItemDto
{
    public Guid Id { get; set; }
    public string BillNumber { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string BillType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateOnly DueDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
