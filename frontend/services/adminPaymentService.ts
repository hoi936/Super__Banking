import type { AdminPaymentListItemDto, AdminPaymentDetailDto } from '~/types/adminPayment'
import type { PagedResult } from '~/types/api'
import { useApiClient } from './api'

export function useAdminPaymentService() {
  const { $api } = useApiClient()

  const getPayments = async (params?: {
    page?: number
    pageSize?: number
    status?: string
    billType?: string
    reference?: string
    accountNumber?: string
    fromDate?: string
    toDate?: string
  }): Promise<PagedResult<AdminPaymentListItemDto>> => {
    return await $api<PagedResult<AdminPaymentListItemDto>>('/api/v1/admin/payments', {
      method: 'GET',
      query: params
    })
  }

  const getPayment = async (id: string): Promise<AdminPaymentDetailDto> => {
    return await $api<AdminPaymentDetailDto>(`/api/v1/admin/payments/${id}`, {
      method: 'GET'
    })
  }

  return {
    getPayments,
    getPayment
  }
}
