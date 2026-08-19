import type { BankCard, UpdateCardLimitsRequest, UpdateCardSettingsRequest } from '~/types/card'
import { useApiClient } from './api'

export function useCardService() {
  const { $api } = useApiClient()

  const getCards = async (): Promise<BankCard[]> => {
    return await $api<BankCard[]>('/api/v1/cards', {
      method: 'GET'
    })
  }

  const lockCard = async (id: string): Promise<BankCard> => {
    return await $api<BankCard>(`/api/v1/cards/${id}/lock`, {
      method: 'POST'
    })
  }

  const unlockCard = async (id: string): Promise<BankCard> => {
    return await $api<BankCard>(`/api/v1/cards/${id}/unlock`, {
      method: 'POST'
    })
  }

  const updateLimits = async (id: string, request: UpdateCardLimitsRequest): Promise<BankCard> => {
    return await $api<BankCard>(`/api/v1/cards/${id}/limits`, {
      method: 'PATCH',
      body: request
    })
  }

  const updateSettings = async (id: string, request: UpdateCardSettingsRequest): Promise<BankCard> => {
    return await $api<BankCard>(`/api/v1/cards/${id}/settings`, {
      method: 'PATCH',
      body: request
    })
  }

  return {
    getCards,
    lockCard,
    unlockCard,
    updateLimits,
    updateSettings
  }
}
