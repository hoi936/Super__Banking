export interface TransactionListItem {
  id: string
  referenceNumber: string
  transactionType: 'TRANSFER' | 'PAYMENT' | 'DEPOSIT' | 'WITHDRAWAL'
  sourceAccountId?: string
  sourceAccountNumber?: string
  destinationAccountId?: string
  destinationAccountNumber?: string
  amount: number
  currency: string
  description?: string
  status: 'PENDING' | 'COMPLETED' | 'FAILED' | 'CANCELLED'
  createdAtUtc: string
  completedAtUtc?: string
}

export interface TransactionDetail extends TransactionListItem {
  sourceAccountName?: string
  destinationAccountName?: string
}
