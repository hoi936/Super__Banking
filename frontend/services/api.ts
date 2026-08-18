import type { SystemStatus, HealthCheckResponse } from '~/types'

export function useApiClient() {
  const config = useRuntimeConfig()
  const baseUrl = config.public.apiBaseUrl || 'http://localhost:8080'

  const getSystemStatus = async (): Promise<SystemStatus> => {
    return await $fetch<SystemStatus>(`${baseUrl}/api/system`, {
      method: 'GET',
      headers: {
        'Accept': 'application/json'
      }
    })
  }

  const getHealthCheck = async (): Promise<HealthCheckResponse> => {
    return await $fetch<HealthCheckResponse>(`${baseUrl}/health`, {
      method: 'GET',
      headers: {
        'Accept': 'application/json'
      }
    })
  }

  return {
    baseUrl,
    getSystemStatus,
    getHealthCheck
  }
}
