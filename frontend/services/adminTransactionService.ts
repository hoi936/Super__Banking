import type { AdminTransactionListItemDto, AdminTransactionDetailDto } from '~/types/adminTransaction'
import type { PagedResult } from '~/types/api'
import { useApiClient } from './api'

export function useAdminTransactionService() {
  const { $api } = useApiClient()

  const getTransactions = async (params?: {
    page?: number
    pageSize?: number
    search?: string
    type?: string
    status?: string
    fromDate?: string
    toDate?: string
    accountNumber?: string
  }): Promise<PagedResult<AdminTransactionListItemDto>> => {
    return await $api<PagedResult<AdminTransactionListItemDto>>('/api/v1/admin/transactions', {
      method: 'GET',
      query: params
    })
  }

  const getTransaction = async (id: string): Promise<AdminTransactionDetailDto> => {
    return await $api<AdminTransactionDetailDto>(`/api/v1/admin/transactions/${id}`, {
      method: 'GET'
    })
  }

  return {
    getTransactions,
    getTransaction
  }
}
