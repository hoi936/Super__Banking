namespace LocalLink.Domain.Enums;

public enum TransactionType
{
    Transfer = 1,
    Payment = 2,
    Deposit = 3,
    Withdrawal = 4,
    TermDepositOpening = 5,
    TermDepositMaturity = 6,
    NapasTransfer = 7,
    LoanDisbursement = 8,
    MobileTopUp = 9
}
