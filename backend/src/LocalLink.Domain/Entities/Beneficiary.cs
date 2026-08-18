namespace LocalLink.Domain.Entities;

public class Beneficiary
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CustomerId { get; set; }
    public virtual Customer Customer { get; set; } = null!;

    public Guid BeneficiaryAccountId { get; set; }
    public virtual BankAccount BeneficiaryAccount { get; set; } = null!;

    public string? Nickname { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
