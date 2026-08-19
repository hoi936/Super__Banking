import type { ApproveLoanApplicationRequest, LoanApplicationDto, RejectLoanApplicationRequest } from '~/types/loan'
import type { PagedResult } from '~/types/api'
import { useApiClient } from './api'

export function useAdminLoanService() {
  const { $api } = useApiClient()

  const getApplications = async (params?: {
    page?: number
    pageSize?: number
    status?: string
    keyword?: string
  }): Promise<PagedResult<LoanApplicationDto>> => {
    return await $api<PagedResult<LoanApplicationDto>>('/api/v1/admin/loans', {
      method: 'GET',
      query: params
    })
  }

  const approve = async (id: string, request: ApproveLoanApplicationRequest): Promise<LoanApplicationDto> => {
    return await $api<LoanApplicationDto>(`/api/v1/admin/loans/${id}/approve`, {
      method: 'POST',
      body: request
    })
  }

  const reject = async (id: string, request: RejectLoanApplicationRequest): Promise<LoanApplicationDto> => {
    return await $api<LoanApplicationDto>(`/api/v1/admin/loans/${id}/reject`, {
      method: 'POST',
      body: request
    })
  }

  return {
    getApplications,
    approve,
    reject
  }
}
