<template>
  <div class="q-pa-md">
    <q-card flat bordered class="q-mb-md">
      <q-card-section>
        <div class="text-h6">Admin Dashboard</div>
        <div class="text-subtitle2" v-if="authStore.user">
          Xin chào, {{ authStore.user.fullName }}
        </div>
      </q-card-section>
      <q-separator />
      <q-card-section>
        <p class="text-positive">
          <q-icon name="check_circle" /> Kết nối Backend thành công.
        </p>
        <p class="text-grey-7">
          Full Admin dashboard sẽ được phát triển ở FE5.
        </p>
      </q-card-section>
    </q-card>
  </div>
</template>

<script setup lang="ts">
import { useAuthStore } from '~/stores/auth'

definePageMeta({
  layout: 'admin',
  middleware: ['admin']
})

const authStore = useAuthStore()
</script>
