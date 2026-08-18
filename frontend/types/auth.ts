export type UserRole = 'CUSTOMER' | 'STAFF' | 'ADMIN'
export type UserStatus = 'ACTIVE' | 'SUSPENDED' | 'LOCKED' | 'CLOSED'

export interface CustomerSummary {
  id: string
  customerCode: string
  fullName: string
}

export interface CurrentUser {
  id: string
  email: string
  status: UserStatus
  roles: UserRole[]
  customer?: CustomerSummary
}

export interface TokenResponse {
  accessToken: string
  refreshToken: string
  expiresIn: number
}

export interface LoginResponse {
  user: CurrentUser
  tokens: TokenResponse
}
