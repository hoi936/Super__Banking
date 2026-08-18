import type { Beneficiary, CreateBeneficiaryRequest } from '~/types'
import { useApiClient } from './api'

export function useBeneficiaryService() {
  const { $api } = useApiClient()

  const getBeneficiaries = async (): Promise<Beneficiary[]> => {
    return await $api<Beneficiary[]>('/api/v1/beneficiaries', {
      method: 'GET'
    })
  }

  const addBeneficiary = async (request: CreateBeneficiaryRequest): Promise<Beneficiary> => {
    return await $api<Beneficiary>('/api/v1/beneficiaries', {
      method: 'POST',
      body: request
    })
  }

  const deleteBeneficiary = async (id: string): Promise<void> => {
    return await $api<void>(`/api/v1/beneficiaries/${id}`, {
      method: 'DELETE'
    })
  }

  return {
    getBeneficiaries,
    addBeneficiary,
    deleteBeneficiary
  }
}
