import type { CreateLoanApplicationRequest, LoanApplicationDto } from '~/types/loan'
import { useApiClient } from './api'

export function useLoanService() {
  const { $api } = useApiClient()

  const getApplications = async (status?: string): Promise<LoanApplicationDto[]> => {
    return await $api<LoanApplicationDto[]>('/api/v1/loans', {
      method: 'GET',
      query: status ? { status } : undefined
    })
  }

  const getApplication = async (id: string): Promise<LoanApplicationDto> => {
    return await $api<LoanApplicationDto>(`/api/v1/loans/${id}`, {
      method: 'GET'
    })
  }

  const createApplication = async (request: CreateLoanApplicationRequest): Promise<LoanApplicationDto> => {
    return await $api<LoanApplicationDto>('/api/v1/loans', {
      method: 'POST',
      body: request
    })
  }

  return {
    getApplications,
    getApplication,
    createApplication
  }
}
