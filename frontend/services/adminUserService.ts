import type { AdminUserListItemDto, AdminUserDetailDto, UpdateUserStatusRequest } from '~/types/adminUser'
import type { PagedResult } from '~/types/api'
import { useApiClient } from './api'

export function useAdminUserService() {
  const { $api } = useApiClient()

  const getUsers = async (params?: {
    page?: number
    pageSize?: number
    search?: string
    status?: string
    role?: string
  }): Promise<PagedResult<AdminUserListItemDto>> => {
    return await $api<PagedResult<AdminUserListItemDto>>('/api/v1/admin/users', {
      method: 'GET',
      query: params
    })
  }

  const getUser = async (id: string): Promise<AdminUserDetailDto> => {
    return await $api<AdminUserDetailDto>(`/api/v1/admin/users/${id}`, {
      method: 'GET'
    })
  }

  const updateUserStatus = async (id: string, request: UpdateUserStatusRequest): Promise<void> => {
    await $api(`/api/v1/admin/users/${id}/status`, {
      method: 'PATCH',
      body: request
    })
  }

  return {
    getUsers,
    getUser,
    updateUserStatus
  }
}
