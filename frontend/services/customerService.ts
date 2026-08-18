import type { CustomerProfile } from '~/types/customer'
import { useApiClient } from './api'

export function useCustomerService() {
  const { $api } = useApiClient()

  const getProfile = async (): Promise<CustomerProfile> => {
    return await $api<CustomerProfile>('/api/v1/customers/me', {
      method: 'GET'
    })
  }

  const updateProfile = async (data: Partial<CustomerProfile>): Promise<void> => {
    await $api('/api/v1/customers/me', {
      method: 'PUT',
      body: data
    })
  }

  return {
    getProfile,
    updateProfile
  }
}
