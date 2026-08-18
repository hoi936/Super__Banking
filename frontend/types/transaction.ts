export interface TransactionListItem {
    id: string;
    referenceNumber: string;
    transactionType: string;
    sourceAccountId?: string;
    sourceAccountNumber?: string;
    destinationAccountId?: string;
    destinationAccountNumber?: string;
    amount: number;
    currency: string;
    description?: string;
    status: string;
    createdAtUtc: string;
    completedAtUtc?: string;
}
