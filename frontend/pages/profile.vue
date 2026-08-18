<template>
  <div class="q-pa-md">
    <q-card flat bordered class="q-mb-md">
      <q-card-section>
        <div class="text-h6">Hồ sơ cá nhân</div>
      </q-card-section>
      <q-separator />
      <q-card-section v-if="authStore.user">
        <div class="q-mb-sm"><strong>Họ và tên:</strong> {{ authStore.user.fullName }}</div>
        <div class="q-mb-sm"><strong>Email:</strong> {{ authStore.user.email }}</div>
        <div class="q-mb-sm"><strong>Vai trò:</strong> {{ authStore.user.roles.join(', ') }}</div>
        <div class="q-mb-sm"><strong>Trạng thái:</strong> {{ authStore.user.status }}</div>
      </q-card-section>
    </q-card>
  </div>
</template>

<script setup lang="ts">
import { useAuthStore } from '~/stores/auth'

definePageMeta({
  middleware: ['auth']
})

const authStore = useAuthStore()
</script>
