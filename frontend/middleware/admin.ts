import { useAuthStore } from '~/stores/auth'
import { Notify } from 'quasar'

export default defineNuxtRouteMiddleware((to, from) => {
  if (process.client) {
    const authStore = useAuthStore()
    
    if (!authStore.isAuthenticated) {
      return navigateTo('/login')
    }
    
    if (!authStore.hasRole('ADMIN') && !authStore.hasRole('STAFF')) {
      if (authStore.hasRole('CUSTOMER')) {
        return navigateTo('/dashboard')
      }
      return navigateTo('/login')
    }

    if (!authStore.hasRole('ADMIN')) {
      const adminOnlyRoutes = ['/admin/users', '/admin/audit-logs']
      if (adminOnlyRoutes.some(r => to.path.startsWith(r))) {
        Notify.create({
          type: 'negative',
          message: 'Bạn không có quyền truy cập chức năng này.',
          position: 'top-right'
        })
        return navigateTo('/admin')
      }
    }
  }
})
