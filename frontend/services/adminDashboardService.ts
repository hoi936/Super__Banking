import type { AdminDashboardDto } from '~/types/adminDashboard'
import { useApiClient } from './api'

export function useAdminDashboardService() {
  const { $api } = useApiClient()

  const getDashboard = async (): Promise<AdminDashboardDto> => {
    return await $api<AdminDashboardDto>('/api/v1/admin/dashboard', {
      method: 'GET'
    })
  }

  return {
    getDashboard
  }
}
