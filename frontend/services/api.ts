import type { SystemStatus, HealthCheckResponse } from '~/types'
import { useAuthStore } from '~/stores/auth'

let isRefreshing = false
let refreshPromise: Promise<boolean> | null = null

export function useApiClient() {
  const config = useRuntimeConfig()
  const baseUrl = config.public.apiBaseUrl || 'http://localhost:8080'

  const $api = $fetch.create({
    baseURL: baseUrl,
    onRequest({ options }) {
      const authStore = useAuthStore()
      if (authStore.accessToken) {
        options.headers = {
          ...options.headers,
          Authorization: `Bearer ${authStore.accessToken}`
        }
      }
    },
    async onResponseError({ request, response, options }) {
      if (response.status === 401) {
        const authStore = useAuthStore()
        
        // Don't intercept refresh or login requests to avoid infinite loops
        const reqStr = request.toString()
        if (reqStr.includes('/auth/refresh') || reqStr.includes('/auth/login')) {
          if (reqStr.includes('/auth/refresh')) {
             authStore.logoutLocally()
          }
          return
        }

        if (authStore.refreshToken) {
          if (!isRefreshing) {
            isRefreshing = true
            refreshPromise = authStore.refreshSession().finally(() => {
              isRefreshing = false
              refreshPromise = null
            })
          }

          const success = await refreshPromise
          if (success) {
            // Retry the original request
            options.headers = {
              ...options.headers,
              Authorization: `Bearer ${authStore.accessToken}`
            }
            return $fetch(request, options)
          } else {
            authStore.logoutLocally()
            if (process.client) {
               window.location.href = '/login'
            }
          }
        }
      }
    }
  })

  const getSystemStatus = async (): Promise<SystemStatus> => {
    return await $api<SystemStatus>(`/api/system`, {
      method: 'GET',
      headers: { 'Accept': 'application/json' }
    })
  }

  const getHealthCheck = async (): Promise<HealthCheckResponse> => {
    return await $api<HealthCheckResponse>(`/health`, {
      method: 'GET',
      headers: { 'Accept': 'application/json' }
    })
  }

  return {
    $api,
    baseUrl,
    getSystemStatus,
    getHealthCheck
  }
}
