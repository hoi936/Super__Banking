import type { BillListItem } from '~/types/bill'
import type { PagedResult } from '~/types/api'
import { useApiClient } from './api'

export function useBillService() {
  const { $api } = useApiClient()

  const getBills = async (page: number = 1, pageSize: number = 10, status?: string): Promise<PagedResult<BillListItem>> => {
    return await $api<PagedResult<BillListItem>>('/api/v1/bills', {
      method: 'GET',
      query: { page, pageSize, status }
    })
  }

  return {
    getBills
  }
}
