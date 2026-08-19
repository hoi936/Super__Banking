export interface AdminPaymentListItemDto {
  id: string
  paymentReference: string
  amount: number
  currency: string
  status: 'COMPLETED' | 'PENDING' | 'FAILED' | string
  billNumber: string
  billType: 'ELECTRICITY' | 'WATER' | 'INTERNET' | 'EDUCATION' | 'OTHER' | string
  accountNumber: string
  createdAtUtc: string
}

export interface AdminPaymentDetailDto {
  id: string
  paymentReference: string
  amount: number
  currency: string
  status: string
  customerFullName: string
  customerCode: string
  accountNumber: string
  billNumber: string
  billType: string
  providerName: string
  transactionReference?: string | null
  idempotencyKey?: string | null
  createdAtUtc: string
  paidAtUtc?: string | null
}
