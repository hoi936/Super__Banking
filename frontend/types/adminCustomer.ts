import type { AccountSummary } from './account'

export interface AdminCustomerListItemDto {
  id: string
  userId: string
  email: string
  customerCode: string
  fullName: string
  phoneNumber?: string | null
  customerStatus: string
  userStatus: string
  accountsCount: number
  createdAtUtc: string
}

export interface AdminCustomerDetailDto {
  id: string
  userId: string
  email: string
  customerCode: string
  fullName: string
  dateOfBirth?: string | null
  gender?: string | null
  phoneNumber?: string | null
  address?: string | null
  customerStatus: string
  userStatus: string
  roles: string[]
  accounts: AccountSummary[]
  createdAtUtc: string
  updatedAtUtc?: string | null
}

export interface UpdateCustomerStatusRequest {
  status: 'ACTIVE' | 'SUSPENDED' | string
}

export interface UpdateAccountStatusRequest {
  status: 'ACTIVE' | 'LOCKED' | string
}
