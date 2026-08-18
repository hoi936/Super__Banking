export interface AdminUserListItemDto {
  id: string
  email: string
  status: 'ACTIVE' | 'SUSPENDED' | 'LOCKED' | string
  roles: string[]
  customerCode?: string | null
  fullName?: string | null
  createdAtUtc: string
}

export interface AdminUserDetailDto {
  id: string
  email: string
  status: string
  roles: string[]
  customerCode?: string | null
  fullName?: string | null
  createdAtUtc: string
  lastLoginAtUtc?: string | null
}

export interface UpdateUserStatusRequest {
  status: 'ACTIVE' | 'SUSPENDED' | string
  reason?: string | null
}
