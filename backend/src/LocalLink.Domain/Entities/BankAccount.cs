using LocalLink.Domain.Enums;

namespace LocalLink.Domain.Entities;

public class BankAccount
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CustomerId { get; set; }
    public virtual Customer Customer { get; set; } = null!;

    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public AccountType AccountType { get; set; } = AccountType.Checking;
    public decimal Balance { get; set; } = 0.00m;
    public string Currency { get; set; } = "VND";
    public AccountStatus Status { get; set; } = AccountStatus.Active;

    public byte[] RowVersion { get; set; } = [];

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    // Navigation properties
    public virtual ICollection<Beneficiary> BeneficiaryRecords { get; set; } = new List<Beneficiary>();
    public virtual ICollection<Transaction> SourceTransactions { get; set; } = new List<Transaction>();
    public virtual ICollection<Transaction> DestinationTransactions { get; set; } = new List<Transaction>();
    public virtual ICollection<Transfer> SourceTransfers { get; set; } = new List<Transfer>();
    public virtual ICollection<Transfer> DestinationTransfers { get; set; } = new List<Transfer>();
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public virtual ICollection<TermDeposit> TermDeposits { get; set; } = new List<TermDeposit>();
    public virtual ICollection<ExternalTransfer> ExternalTransfers { get; set; } = new List<ExternalTransfer>();
    public virtual ICollection<BankCard> Cards { get; set; } = new List<BankCard>();
    public virtual ICollection<LoanApplication> LoanApplications { get; set; } = new List<LoanApplication>();
    public virtual ICollection<MobileTopUp> MobileTopUps { get; set; } = new List<MobileTopUp>();
}
