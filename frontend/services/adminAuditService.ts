import type { AuditLogDto } from '~/types/auditLog'
import type { PagedResult } from '~/types/api'
import { useApiClient } from './api'

export function useAdminAuditService() {
  const { $api } = useApiClient()

  const getAuditLogs = async (params?: {
    page?: number
    pageSize?: number
    action?: string
    entityType?: string
    userId?: string
    fromDate?: string
    toDate?: string
    search?: string
  }): Promise<PagedResult<AuditLogDto>> => {
    return await $api<PagedResult<AuditLogDto>>('/api/v1/admin/audit-logs', {
      method: 'GET',
      query: params
    })
  }

  return {
    getAuditLogs
  }
}
