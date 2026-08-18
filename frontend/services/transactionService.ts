import type { TransactionListItem } from '~/types/transaction'
import type { PagedResult } from '~/types/api'
import { useApiClient } from './api'

export function useTransactionService() {
  const { $api } = useApiClient()

  const getTransactions = async (page: number = 1, pageSize: number = 10, accountId?: string): Promise<PagedResult<TransactionListItem>> => {
    return await $api<PagedResult<TransactionListItem>>('/api/v1/transactions', {
      method: 'GET',
      query: { page, pageSize, accountId }
    })
  }

  return {
    getTransactions
  }
}
