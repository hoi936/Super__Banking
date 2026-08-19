export interface CreateGatewayDepositRequest {
  accountId: string
  amount: number
  provider: string
}

export interface GatewayDepositReceiptDto {
  transactionId: string
  referenceNumber: string
  provider: string
  amount: number
  status: string
  paymentUrl: string
  createdAtUtc: string
}
