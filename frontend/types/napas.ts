export interface NapasBank {
  code: string
  name: string
  shortName: string
  supportsAccountTransfer: boolean
  supportsCardTransfer: boolean
}

export interface NapasLookupRequest {
  bankCode: string
  destinationNumber: string
  destinationType: 'ACCOUNT' | 'CARD'
}

export interface NapasLookupResult {
  bankCode: string
  bankName: string
  destinationNumber: string
  destinationName: string
  destinationType: 'ACCOUNT' | 'CARD'
}

export interface CreateNapasTransferRequest extends NapasLookupRequest {
  sourceAccountId: string
  amount: number
  description?: string
}

export interface NapasTransferReceipt {
  externalTransferId: string
  reference: string
  sourceAccountId: string
  sourceAccountNumber: string
  externalBankCode: string
  externalBankName: string
  destinationNumber: string
  destinationName: string
  destinationType: string
  amount: number
  feeAmount: number
  totalDebitAmount: number
  remainingBalance: number
  currency: string
  description?: string | null
  status: string
  createdAtUtc: string
  completedAtUtc?: string | null
}

export interface NapasTransferListItem {
  id: string
  reference: string
  sourceAccountNumber: string
  externalBankCode: string
  externalBankName: string
  destinationNumber: string
  destinationName: string
  destinationType: string
  amount: number
  feeAmount: number
  currency: string
  status: string
  createdAtUtc: string
  completedAtUtc?: string | null
}
