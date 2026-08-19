import { useAuthStore } from '~/stores/auth'

export default defineNuxtPlugin(async (nuxtApp) => {
  const authStore = useAuthStore()
  
  // We only run this on the client side since our app is SPA and relies on localStorage
  if (process.client) {
    await authStore.initializeAuth()
  }
})
