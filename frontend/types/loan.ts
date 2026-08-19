export type LoanApplicationStatus = 'Pending' | 'Disbursed' | 'Rejected' | 'Cancelled' | string

export interface LoanApplicationDto {
  id: string
  applicationNumber: string
  customerId: string
  customerCode: string
  customerFullName: string
  disbursementAccountId: string
  disbursementAccountNumber: string
  requestedAmount: number
  approvedAmount?: number | null
  annualInterestRate: number
  termMonths: number
  monthlyIncome: number
  purpose: string
  currency: string
  status: LoanApplicationStatus
  reviewNote?: string | null
  disbursementReference?: string | null
  submittedAtUtc: string
  reviewedAtUtc?: string | null
  disbursedAtUtc?: string | null
}

export interface CreateLoanApplicationRequest {
  disbursementAccountId: string
  requestedAmount: number
  termMonths: number
  monthlyIncome: number
  purpose: string
}

export interface ApproveLoanApplicationRequest {
  approvedAmount: number
  annualInterestRate: number
  reviewNote?: string | null
}

export interface RejectLoanApplicationRequest {
  reason: string
}
