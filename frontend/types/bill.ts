export interface BillListItem {
    id: string;
    billNumber: string;
    providerName: string;
    billType: string;
    amount: number;
    dueDate: string;
    status: string;
    createdAtUtc: string;
}
