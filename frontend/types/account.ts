export interface AccountSummary {
  id: string
  accountNumber: string
  accountName: string
  accountType: 'CHECKING' | 'SAVINGS' | 'FIXED_DEPOSIT' | 'LOAN'
  balance: number
  currency: string
  status: 'ACTIVE' | 'DORMANT' | 'FROZEN' | 'LOCKED' | 'CLOSED'
}

export interface AccountDetail extends AccountSummary {
  customerId: string
  createdAtUtc: string
}

export interface AccountLookup {
  accountNumber: string
  accountName: string
}
