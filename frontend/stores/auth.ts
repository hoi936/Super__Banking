import { defineStore } from 'pinia'
import type { CurrentUser, LoginRequest, UserRole } from '~/types/auth'
import { useAuthService } from '~/services/authService'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: null as CurrentUser | null,
    accessToken: null as string | null,
    isLoading: false
  }),
  
  getters: {
    isAuthenticated: (state) => !!state.accessToken && !!state.user,
    
    // In SPA, we can read/write localStorage directly. 
    // This getter checks if there's a stored refresh token.
    refreshToken: () => {
      if (process.client) {
        return localStorage.getItem('locallink_refresh_token')
      }
      return null
    }
  },

  actions: {
    hasRole(role: UserRole): boolean {
      return this.user?.roles.includes(role) || false
    },

    async login(request: LoginRequest) {
      this.isLoading = true
      try {
        const authService = useAuthService()
        const response = await authService.login(request)
        
        this.user = response.user
        this.accessToken = response.tokens.accessToken
        
        if (process.client) {
          localStorage.setItem('locallink_refresh_token', response.tokens.refreshToken)
        }
        
        return true
      } catch (error) {
        console.error('Login failed')
        throw error
      } finally {
        this.isLoading = false
      }
    },

    async fetchCurrentUser() {
      if (!this.accessToken) return false
      try {
        const authService = useAuthService()
        const user = await authService.getCurrentUser()
        this.user = user
        return true
      } catch (error) {
        return false
      }
    },

    async refreshSession(): Promise<boolean> {
      const currentRefreshToken = this.refreshToken
      if (!currentRefreshToken) return false

      try {
        const authService = useAuthService()
        const response = await authService.refresh(currentRefreshToken)
        
        this.accessToken = response.accessToken
        if (process.client) {
          localStorage.setItem('locallink_refresh_token', response.refreshToken)
        }
        
        // After refresh, we should fetch current user to restore session fully
        await this.fetchCurrentUser()
        
        return true
      } catch (error) {
        this.logoutLocally()
        return false
      }
    },

    async initializeAuth() {
      if (!process.client) return
      
      this.isLoading = true
      // Only initialize if we have a refresh token
      if (this.refreshToken) {
        await this.refreshSession()
      }
      this.isLoading = false
    },

    async logout() {
      const currentRefreshToken = this.refreshToken
      if (currentRefreshToken) {
        const authService = useAuthService()
        await authService.logout(currentRefreshToken)
      }
      this.logoutLocally()
      
      // Redirect to login
      if (process.client) {
        const router = useRouter()
        router.push('/login')
      }
    },

    logoutLocally() {
      this.user = null
      this.accessToken = null
      if (process.client) {
        localStorage.removeItem('locallink_refresh_token')
      }
    }
  }
})
