export interface AdminDashboardDto {
  customers: DashboardCustomersDto
  accounts: DashboardAccountsDto
  today: DashboardTodayDto
  notifications: DashboardNotificationsDto
  generatedAtUtc: string
}

export interface DashboardCustomersDto {
  total: number
  active: number
  suspended: number
}

export interface DashboardAccountsDto {
  total: number
  active: number
  locked: number
  totalBalance: number
}

export interface DashboardTodayDto {
  transfers: number
  transferVolume: number
  payments: number
  paymentVolume: number
}

export interface DashboardNotificationsDto {
  createdToday: number
}
