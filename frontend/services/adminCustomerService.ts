import type { 
  AdminCustomerListItemDto, 
  AdminCustomerDetailDto,
  UpdateCustomerStatusRequest,
  UpdateAccountStatusRequest
} from '~/types/adminCustomer'
import type { AccountSummary } from '~/types/account'
import type { PagedResult } from '~/types/api'
import { useApiClient } from './api'

export function useAdminCustomerService() {
  const { $api } = useApiClient()

  const getCustomers = async (params?: {
    page?: number
    pageSize?: number
    search?: string
    status?: string
  }): Promise<PagedResult<AdminCustomerListItemDto>> => {
    return await $api<PagedResult<AdminCustomerListItemDto>>('/api/v1/admin/customers', {
      method: 'GET',
      query: params
    })
  }

  const getCustomer = async (id: string): Promise<AdminCustomerDetailDto> => {
    return await $api<AdminCustomerDetailDto>(`/api/v1/admin/customers/${id}`, {
      method: 'GET'
    })
  }

  const getCustomerAccounts = async (id: string): Promise<AccountSummary[]> => {
    return await $api<AccountSummary[]>(`/api/v1/admin/customers/${id}/accounts`, {
      method: 'GET'
    })
  }

  const updateCustomerStatus = async (id: string, request: UpdateCustomerStatusRequest): Promise<void> => {
    await $api(`/api/v1/admin/customers/${id}/status`, {
      method: 'PATCH',
      body: request
    })
  }

  const updateAccountStatus = async (id: string, request: UpdateAccountStatusRequest): Promise<void> => {
    await $api(`/api/v1/admin/accounts/${id}/status`, {
      method: 'PATCH',
      body: request
    })
  }

  return {
    getCustomers,
    getCustomer,
    getCustomerAccounts,
    updateCustomerStatus,
    updateAccountStatus
  }
}
