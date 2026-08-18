import type { CreateTransferRequest, TransferReceipt, TransferListItem } from '~/types'
import { useApiClient } from './api'
import type { PagedResult } from '~/types/api'

export function useTransferService() {
  const { $api } = useApiClient()

  const createTransfer = async (request: CreateTransferRequest, idempotencyKey: string): Promise<TransferReceipt> => {
    return await $api<TransferReceipt>('/api/v1/transfers', {
      method: 'POST',
      body: request,
      headers: {
        'Idempotency-Key': idempotencyKey
      }
    })
  }

  const getTransfers = async (params: {
    page?: number
    pageSize?: number
    accountId?: string
    status?: string
    fromDate?: string
    toDate?: string
  } = {}): Promise<PagedResult<TransferListItem>> => {
    return await $api<PagedResult<TransferListItem>>('/api/v1/transfers', {
      method: 'GET',
      query: params
    })
  }

  const getTransferDetail = async (id: string): Promise<TransferReceipt> => {
    return await $api<TransferReceipt>(`/api/v1/transfers/${id}`, {
      method: 'GET'
    })
  }

  return {
    createTransfer,
    getTransfers,
    getTransferDetail
  }
}
