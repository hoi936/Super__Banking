export interface NotificationDto {
  id: string
  title: string
  message: string
  type: 'TRANSFER' | 'PAYMENT' | 'ACCOUNT' | 'SYSTEM' | 'SECURITY' | string
  isRead: boolean
  createdAtUtc: string
  readAtUtc?: string | null
}

export interface UnreadNotificationCount {
  count: number
}
