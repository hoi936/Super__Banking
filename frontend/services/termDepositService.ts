import type {
  CreateTermDepositRequest,
  TermDepositDetailDto,
  TermDepositDto,
  TermDepositReceiptDto
} from '~/types/termDeposit'
import { useApiClient } from './api'

export function useTermDepositService() {
  const { $api } = useApiClient()

  const getTermDeposits = async (status?: string): Promise<TermDepositDto[]> => {
    return await $api<TermDepositDto[]>('/api/v1/term-deposits', {
      method: 'GET',
      query: status ? { status } : undefined
    })
  }

  const getTermDeposit = async (id: string): Promise<TermDepositDetailDto> => {
    return await $api<TermDepositDetailDto>(`/api/v1/term-deposits/${id}`, {
      method: 'GET'
    })
  }

  const openTermDeposit = async (request: CreateTermDepositRequest): Promise<TermDepositReceiptDto> => {
    const idempotencyKey = typeof crypto !== 'undefined' && 'randomUUID' in crypto
      ? crypto.randomUUID()
      : `${Date.now()}-${Math.random().toString(16).slice(2)}`

    return await $api<TermDepositReceiptDto>('/api/v1/term-deposits', {
      method: 'POST',
      body: request,
      headers: {
        'Idempotency-Key': idempotencyKey
      }
    })
  }

  return {
    getTermDeposits,
    getTermDeposit,
    openTermDeposit
  }
}
