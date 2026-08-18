export interface AdminTransactionListItemDto {
  id: string
  referenceNumber: string
  transactionType: 'TRANSFER' | 'PAYMENT' | 'FEE' | string
  amount: number
  currency: string
  status: 'COMPLETED' | 'PENDING' | 'FAILED' | 'REVERSED' | string
  sourceAccountNumber?: string | null
  destinationAccountNumber?: string | null
  createdAtUtc: string
}

export interface AdminTransactionDetailDto {
  id: string
  referenceNumber: string
  transactionType: string
  amount: number
  currency: string
  status: string
  description?: string | null
  sourceAccount?: AdminTransactionAccountDto | null
  destinationAccount?: AdminTransactionAccountDto | null
  createdAtUtc: string
  completedAtUtc?: string | null
}

export interface AdminTransactionAccountDto {
  id: string
  accountNumber: string
  accountName: string
  customerFullName: string
  customerCode: string
}
