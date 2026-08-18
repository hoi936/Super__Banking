import type { UnreadNotificationCount } from '~/types/notification'
import { useApiClient } from './api'

export function useNotificationService() {
  const { $api } = useApiClient()

  const getUnreadCount = async (): Promise<UnreadNotificationCount> => {
    return await $api<UnreadNotificationCount>('/api/v1/notifications/unread-count', {
      method: 'GET'
    })
  }

  return {
    getUnreadCount
  }
}
