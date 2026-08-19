using LocalLink.Domain.Enums;

namespace LocalLink.Domain.Entities;

public class Customer
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;

    public string CustomerCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateOnly? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public CustomerStatus Status { get; set; } = CustomerStatus.Active;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    // Navigation properties
    public virtual ICollection<BankAccount> BankAccounts { get; set; } = new List<BankAccount>();
    public virtual ICollection<Beneficiary> Beneficiaries { get; set; } = new List<Beneficiary>();
    public virtual ICollection<Bill> Bills { get; set; } = new List<Bill>();
    public virtual ICollection<BankCard> Cards { get; set; } = new List<BankCard>();
    public virtual ICollection<LoanApplication> LoanApplications { get; set; } = new List<LoanApplication>();
    public virtual ICollection<MobileTopUp> MobileTopUps { get; set; } = new List<MobileTopUp>();
}
