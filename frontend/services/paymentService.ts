import type { CreatePaymentRequest, PaymentReceiptDto, PaymentListItemDto, PaymentDetailDto } from '~/types/payment'
import type { PagedResult } from '~/types/api'
import { useApiClient } from './api'

export function usePaymentService() {
  const { $api } = useApiClient()

  const payBill = async (request: CreatePaymentRequest, idempotencyKey: string): Promise<PaymentReceiptDto> => {
    return await $api<PaymentReceiptDto>('/api/v1/payments', {
      method: 'POST',
      body: request,
      headers: {
        'Idempotency-Key': idempotencyKey
      }
    })
  }

  const getPayments = async (params?: {
    page?: number
    pageSize?: number
    status?: string
    billType?: string
    fromDate?: string
    toDate?: string
  }): Promise<PagedResult<PaymentListItemDto>> => {
    return await $api<PagedResult<PaymentListItemDto>>('/api/v1/payments', {
      method: 'GET',
      query: params
    })
  }

  const getPayment = async (id: string): Promise<PaymentDetailDto> => {
    return await $api<PaymentDetailDto>(`/api/v1/payments/${id}`, {
      method: 'GET'
    })
  }

  return {
    payBill,
    getPayments,
    getPayment
  }
}
