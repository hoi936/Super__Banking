export interface CreateQrPayloadRequest {
  accountId: string
  amount?: number | null
  description?: string | null
}

export interface ParseQrPayloadRequest {
  payload: string
}

export interface QrPayPayload {
  payload: string
  payloadFormat: string
  accountId: string
  accountNumber: string
  accountName: string
  bankCode: string
  bankName: string
  amount?: number | null
  description?: string | null
  generatedAtUtc: string
}

export interface ParsedQrPay {
  isSupported: boolean
  paymentRail: 'LOCALBANK' | 'VIETQR' | string
  bankCode: string
  bankName: string
  accountNumber: string
  accountName?: string | null
  amount?: number | null
  description?: string | null
  rawPayload: string
  warning?: string | null
}
