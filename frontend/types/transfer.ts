export interface CreateTransferRequest {
  sourceAccountId: string
  destinationAccountNumber: string
  amount: number
  description?: string
}

export interface TransferReceipt {
  transferId: string
  reference: string
  sourceAccountId: string
  sourceAccountNumber: string
  destinationAccountId: string
  destinationAccountNumber: string
  destinationAccountName: string
  amount: number
  currency: string
  description?: string
  status: 'PENDING' | 'COMPLETED' | 'FAILED' | 'CANCELLED'
  createdAtUtc: string
  completedAtUtc?: string
}

export interface TransferListItem {
  id: string
  reference: string
  sourceAccountId: string
  sourceAccountNumber: string
  destinationAccountId: string
  destinationAccountNumber: string
  destinationAccountName: string
  amount: number
  description: string
  status: string
  createdAtUtc: string
  completedAtUtc?: string
}
