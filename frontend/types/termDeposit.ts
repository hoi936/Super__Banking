export type TermDepositStatus = 'Active' | 'Matured' | 'Closed' | 'Cancelled'

export interface TermDepositDto {
  id: string
  depositNumber: string
  principalAmount: number
  annualInterestRate: number
  tenorMonths: number
  expectedInterestAmount: number
  currency: string
  status: TermDepositStatus | string
  openedAtUtc: string
  maturityDateUtc: string
}

export interface TermDepositDetailDto extends TermDepositDto {
  paidInterestAmount?: number | null
  closedAtUtc?: string | null
  sourceAccountNumber: string
  sourceAccountName: string
  openingTransactionId: string
  maturityTransactionId?: string | null
}

export interface TermDepositReceiptDto {
  termDepositId: string
  depositNumber: string
  sourceAccountNumber: string
  principalAmount: number
  annualInterestRate: number
  tenorMonths: number
  expectedInterestAmount: number
  maturityDateUtc: string
  remainingBalance: number
  currency: string
}

export interface CreateTermDepositRequest {
  sourceAccountId: string
  principalAmount: number
  tenorMonths: number
}
