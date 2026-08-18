<template>
  <div class="q-pa-md">
    <q-card flat bordered class="q-mb-md">
      <q-card-section>
        <div class="text-h6">Tổng quan tài khoản</div>
        <div class="text-subtitle2" v-if="authStore.user">
          Xin chào, {{ authStore.user.fullName }} ({{ authStore.user.customer?.customerCode || 'N/A' }})
        </div>
      </q-card-section>
      <q-separator />
      <q-card-section>
        <p class="text-positive">
          <q-icon name="check_circle" /> Kết nối Backend thành công.
        </p>
        <p class="text-grey-7">
          Chi tiết Dashboard khách hàng sẽ được phát triển ở FE2.
        </p>
      </q-card-section>
    </q-card>
  </div>
</template>

<script setup lang="ts">
import { useAuthStore } from '~/stores/auth'

definePageMeta({
  middleware: ['customer']
})

const authStore = useAuthStore()
</script>
