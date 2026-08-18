export interface BillListItemDto {
  id: string
  billNumber: string
  providerName: string
  billType: 'ELECTRICITY' | 'WATER' | 'INTERNET' | 'EDUCATION' | 'OTHER' | string
  amount: number
  dueDate: string
  status: 'UNPAID' | 'PAID' | 'OVERDUE' | 'CANCELLED' | string
  createdAtUtc: string
}

export interface BillDetailDto {
  id: string
  billNumber: string
  providerName: string
  billType: string
  amount: number
  dueDate: string
  status: string
  createdAtUtc: string
  updatedAtUtc?: string | null
  paymentId?: string | null
  paidAtUtc?: string | null
}
