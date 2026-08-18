export interface AuditLogDto {
  id: string
  userId?: string | null
  action: string
  entityType?: string | null
  entityId?: string | null
  description?: string | null
  ipAddress?: string | null
  createdAtUtc: string
}
