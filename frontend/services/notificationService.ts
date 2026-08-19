import type { NotificationDto, UnreadNotificationCount } from '~/types/notification'
import type { PagedResult } from '~/types/api'
import { useApiClient } from './api'

export function useNotificationService() {
  const { $api } = useApiClient()

  const getNotifications = async (params?: {
    page?: number
    pageSize?: number
    isRead?: boolean
    type?: string
  }): Promise<PagedResult<NotificationDto>> => {
    return await $api<PagedResult<NotificationDto>>('/api/v1/notifications', {
      method: 'GET',
      query: params
    })
  }

  const getUnreadCount = async (): Promise<UnreadNotificationCount> => {
    return await $api<UnreadNotificationCount>('/api/v1/notifications/unread-count', {
      method: 'GET'
    })
  }

  const markAsRead = async (id: string): Promise<NotificationDto> => {
    return await $api<NotificationDto>(`/api/v1/notifications/${id}/read`, {
      method: 'PATCH'
    })
  }

  const markAllAsRead = async (): Promise<{ updatedCount: number }> => {
    return await $api<{ updatedCount: number }>('/api/v1/notifications/read-all', {
      method: 'PATCH'
    })
  }

  return {
    getNotifications,
    getUnreadCount,
    markAsRead,
    markAllAsRead
  }
}
