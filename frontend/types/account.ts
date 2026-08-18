export interface AccountSummary {
    id: string;
    accountNumber: string;
    accountName: string;
    accountType: string;
    balance: number;
    currency: string;
    status: string;
}

export interface AccountDetail extends AccountSummary {
    customerId: string;
    createdAtUtc: string;
}
