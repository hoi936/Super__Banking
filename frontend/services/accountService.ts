import type { AccountSummary, AccountDetail } from '~/types/account'
import { useApiClient } from './api'

export function useAccountService() {
  const { $api } = useApiClient()

  const getAccounts = async (): Promise<AccountSummary[]> => {
    return await $api<AccountSummary[]>('/api/v1/accounts', {
      method: 'GET'
    })
  }

  const getAccount = async (id: string): Promise<AccountDetail> => {
    return await $api<AccountDetail>(`/api/v1/accounts/${id}`, {
      method: 'GET'
    })
  }

  const lookupAccount = async (accountNumber: string): Promise<{ accountNumber: string, accountName: string }> => {
    return await $api<{ accountNumber: string, accountName: string }>(`/api/v1/accounts/lookup/${accountNumber}`, {
      method: 'GET'
    })
  }

  return {
    getAccounts,
    getAccount,
    lookupAccount
  }
}
