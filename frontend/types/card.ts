export interface BankCard {
  id: string
  linkedAccountId: string
  linkedAccountNumber: string
  cardNumberMasked: string
  lastFourDigits: string
  cardholderName: string
  cardType: 'DEBIT' | 'CREDIT' | string
  status: 'ACTIVE' | 'LOCKED' | 'BLOCKED' | 'EXPIRED' | 'CANCELLED' | string
  dailyLimit: number
  monthlyLimit: number
  currency: string
  onlinePaymentEnabled: boolean
  contactlessEnabled: boolean
  expiryMonth: number
  expiryYear: number
  issuedAtUtc: string
}

export interface UpdateCardLimitsRequest {
  dailyLimit: number
  monthlyLimit: number
}

export interface UpdateCardSettingsRequest {
  onlinePaymentEnabled: boolean
  contactlessEnabled: boolean
}
