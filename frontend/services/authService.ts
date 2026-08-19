import type { LoginRequest, LoginResponse, TokenResponse, CurrentUser } from '~/types/auth'
import { useApiClient } from './api'

export function useAuthService() {
  const { $api } = useApiClient()

  const login = async (request: LoginRequest): Promise<LoginResponse> => {
    return await $api<LoginResponse>('/api/v1/auth/login', {
      method: 'POST',
      body: request
    })
  }

  const refresh = async (refreshToken: string): Promise<TokenResponse> => {
    return await $api<TokenResponse>('/api/v1/auth/refresh', {
      method: 'POST',
      body: { refreshToken }
    })
  }

  const logout = async (refreshToken: string | null): Promise<void> => {
    await $api('/api/v1/auth/logout', {
      method: 'POST',
      body: { refreshToken }
    }).catch(e => {
      // Ignore logout errors (e.g. already revoked)
      console.warn('Logout API failed, ignoring.', e)
    })
  }

  const getCurrentUser = async (): Promise<CurrentUser> => {
    return await $api<CurrentUser>('/api/v1/auth/me', {
      method: 'GET'
    })
  }

  return {
    login,
    refresh,
    logout,
    getCurrentUser
  }
}
