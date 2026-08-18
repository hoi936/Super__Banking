import type { BillListItemDto, BillDetailDto } from '~/types/bill'
import type { PagedResult } from '~/types/api'
import { useApiClient } from './api'

export function useBillService() {
  const { $api } = useApiClient()

  const getBills = async (params?: {
    page?: number
    pageSize?: number
    status?: string
    type?: string
    fromDueDate?: string
    toDueDate?: string
  }): Promise<PagedResult<BillListItemDto>> => {
    return await $api<PagedResult<BillListItemDto>>('/api/v1/bills', {
      method: 'GET',
      query: params
    })
  }

  const getBill = async (id: string): Promise<BillDetailDto> => {
    return await $api<BillDetailDto>(`/api/v1/bills/${id}`, {
      method: 'GET'
    })
  }

  return {
    getBills,
    getBill
  }
}
