import type { TransactionListItem, TransactionDetail } from '~/types/transaction'
import type { PagedResult } from '~/types/api'
import { useApiClient } from './api'

export function useTransactionService() {
  const { $api } = useApiClient()

  const getTransactions = async (params: { 
    page?: number, 
    pageSize?: number,
    accountId?: string,
    type?: string,
    fromDate?: string,
    toDate?: string
  } = {}): Promise<PagedResult<TransactionListItem>> => {
    return await $api<PagedResult<TransactionListItem>>('/api/v1/transactions', {
      method: 'GET',
      query: params
    })
  }

  const getTransaction = async (id: string): Promise<TransactionDetail> => {
    return await $api<TransactionDetail>(`/api/v1/transactions/${id}`, {
      method: 'GET'
    })
  }

  return {
    getTransactions,
    getTransaction
  }
}
