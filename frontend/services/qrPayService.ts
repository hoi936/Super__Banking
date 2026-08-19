import type { CreateQrPayloadRequest, ParseQrPayloadRequest, ParsedQrPay, QrPayPayload } from '~/types'
import { useApiClient } from './api'

export function useQrPayService() {
  const { $api } = useApiClient()

  const createReceivePayload = async (request: CreateQrPayloadRequest): Promise<QrPayPayload> => {
    return await $api<QrPayPayload>('/api/v1/qr-pay/receive-payload', {
      method: 'POST',
      body: request
    })
  }

  const parsePayload = async (request: ParseQrPayloadRequest): Promise<ParsedQrPay> => {
    return await $api<ParsedQrPay>('/api/v1/qr-pay/parse', {
      method: 'POST',
      body: request
    })
  }

  return {
    createReceivePayload,
    parsePayload
  }
}
