import type {
  CreateMobileTopUpRequest,
  MobileProvider,
  MobileTopUpPagedResult,
  MobileTopUpReceipt
} from '~/types/mobileTopup'
import { useApiClient } from './api'

export function useMobileTopupService() {
  const { $api } = useApiClient()

  const getProviders = async (): Promise<MobileProvider[]> => {
    return await $api<MobileProvider[]>('/api/v1/mobile-topups/providers', {
      method: 'GET'
    })
  }

  const purchase = async (request: CreateMobileTopUpRequest): Promise<MobileTopUpReceipt> => {
    const idempotencyKey = typeof crypto !== 'undefined' && 'randomUUID' in crypto
      ? crypto.randomUUID()
      : `${Date.now()}-${Math.random().toString(16).slice(2)}`

    return await $api<MobileTopUpReceipt>('/api/v1/mobile-topups/purchases', {
      method: 'POST',
      body: request,
      headers: {
        'Idempotency-Key': idempotencyKey
      }
    })
  }

  const getPurchases = async (params: {
    page?: number
    pageSize?: number
    status?: string
  } = {}): Promise<MobileTopUpPagedResult> => {
    return await $api<MobileTopUpPagedResult>('/api/v1/mobile-topups/purchases', {
      method: 'GET',
      query: params
    })
  }

  return {
    getProviders,
    purchase,
    getPurchases
  }
}
