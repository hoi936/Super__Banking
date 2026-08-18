import { useAuthStore } from '~/stores/auth'

export default defineNuxtRouteMiddleware((to, from) => {
  if (process.client) {
    const authStore = useAuthStore()
    
    if (!authStore.isAuthenticated) {
      return navigateTo('/login')
    }
    
    if (!authStore.hasRole('ADMIN') && !authStore.hasRole('STAFF')) {
      alert('Không có quyền truy cập. Đang chuyển hướng...')
      if (authStore.hasRole('CUSTOMER')) {
        return navigateTo('/dashboard')
      }
      return navigateTo('/login')
    }
  }
})
