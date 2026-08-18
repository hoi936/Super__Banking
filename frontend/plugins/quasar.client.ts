import { defineNuxtPlugin } from '#app'
import { Quasar } from 'quasar'

export default defineNuxtPlugin((nuxtApp) => {
  nuxtApp.vueApp.use(Quasar, {
    plugins: {}
  })
})
