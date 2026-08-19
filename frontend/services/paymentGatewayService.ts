import type { CreateGatewayDepositRequest, GatewayDepositReceiptDto } from '~/types/paymentGateway'
import { useApiClient } from './api'

export function usePaymentGatewayService() {
  const { $api } = useApiClient()

  const createDeposit = async (request: CreateGatewayDepositRequest): Promise<GatewayDepositReceiptDto> => {
    return await $api<GatewayDepositReceiptDto>('/api/v1/payment-gateway/deposit', {
      method: 'POST',
      body: request
    })
  }

  return {
    createDeposit
  }
}
