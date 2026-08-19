import type { PagedResult } from '~/types/api'
import type {
  CreateNapasTransferRequest,
  NapasBank,
  NapasLookupRequest,
  NapasLookupResult,
  NapasTransferListItem,
  NapasTransferReceipt
} from '~/types/napas'
import { useApiClient } from './api'

export function useNapasService() {
  const { $api } = useApiClient()

  const getBanks = async (): Promise<NapasBank[]> => {
    return await $api<NapasBank[]>('/api/v1/napas/banks', {
      method: 'GET'
    })
  }

  const lookup = async (request: NapasLookupRequest): Promise<NapasLookupResult> => {
    return await $api<NapasLookupResult>('/api/v1/napas/lookup', {
      method: 'POST',
      body: request
    })
  }

  const transfer = async (request: CreateNapasTransferRequest): Promise<NapasTransferReceipt> => {
    const idempotencyKey = typeof crypto !== 'undefined' && 'randomUUID' in crypto
      ? crypto.randomUUID()
      : `${Date.now()}-${Math.random().toString(16).slice(2)}`

    return await $api<NapasTransferReceipt>('/api/v1/napas/transfers', {
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
    status?: string
  } = {}): Promise<PagedResult<NapasTransferListItem>> => {
    return await $api<PagedResult<NapasTransferListItem>>('/api/v1/napas/transfers', {
      method: 'GET',
      query: params
    })
  }

  return {
    getBanks,
    lookup,
    transfer,
    getTransfers
  }
}
