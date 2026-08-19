export interface CreatePaymentRequest {
  billId: string
  accountId: string
}

export interface PaymentReceiptDto {
  paymentId: string
  reference: string
  billNumber: string
  providerName: string
  accountNumber: string
  amount: number
  currency: string
  status: 'COMPLETED' | string
  paidAtUtc: string
}

export interface PaymentListItemDto {
  id: string
  referenceNumber: string
  billNumber: string
  providerName: string
  billType: string
  accountNumber: string
  amount: number
  currency: string
  status: string
  paidAtUtc?: string | null
  createdAtUtc: string
}

export interface PaymentDetailDto {
  id: string
  referenceNumber: string
  billId: string
  billNumber: string
  providerName: string
  billType: string
  accountId: string
  accountNumber: string
  amount: number
  currency: string
  status: string
  paidAtUtc?: string | null
  createdAtUtc: string
  idempotencyKey?: string | null
}
