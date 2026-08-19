import { useAuthStore } from '~/stores/auth'

export default defineNuxtRouteMiddleware((to, from) => {
  if (process.client) {
    const authStore = useAuthStore()
    if (authStore.isAuthenticated) {
      if (authStore.hasRole('ADMIN') || authStore.hasRole('STAFF')) {
        return navigateTo('/admin')
      }
      return navigateTo('/dashboard')
    }
  }
})
