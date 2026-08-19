<template>
  <q-layout view="hHh Lpr lFf">
    <q-header elevated class="bg-dark text-white">
      <q-toolbar>
        <q-btn dense flat round icon="menu" @click="toggleLeftDrawer" />

        <q-toolbar-title>
          InterLink Admin
        </q-toolbar-title>

        <q-space />

        <div v-if="authStore.user" class="q-mr-sm text-right">
          <div class="text-subtitle2">{{ authStore.user.fullName }}</div>
          <div class="text-caption text-weight-light">{{ authStore.user.roles.join(', ') }}</div>
        </div>
        
        <q-btn flat round dense icon="admin_panel_settings">
          <q-menu>
            <q-list style="min-width: 150px">
              <q-item clickable v-close-popup to="/profile">
                <q-item-section avatar>
                  <q-icon name="person" />
                </q-item-section>
                <q-item-section>Hồ sơ cá nhân</q-item-section>
              </q-item>
              <q-separator />
              <q-item clickable v-close-popup @click="onLogout">
                <q-item-section avatar>
                  <q-icon name="logout" color="red" />
                </q-item-section>
                <q-item-section class="text-red">Đăng xuất</q-item-section>
              </q-item>
            </q-list>
          </q-menu>
        </q-btn>
      </q-toolbar>
    </q-header>

    <q-drawer v-model="leftDrawerOpen" show-if-above bordered class="bg-grey-2">
      <q-list padding>
        <q-item-label header>QUẢN TRỊ</q-item-label>
        
        <q-item clickable v-ripple to="/admin" exact>
          <q-item-section avatar>
            <q-icon name="speed" />
          </q-item-section>
          <q-item-section>Dashboard</q-item-section>
        </q-item>

        <q-item clickable v-ripple to="/admin/customers">
          <q-item-section avatar>
            <q-icon name="people" />
          </q-item-section>
          <q-item-section>Khách hàng</q-item-section>
        </q-item>

        <q-item clickable v-ripple to="/admin/transactions">
          <q-item-section avatar>
            <q-icon name="list_alt" />
          </q-item-section>
          <q-item-section>Giao dịch</q-item-section>
        </q-item>

        <q-item clickable v-ripple to="/admin/payments">
          <q-item-section avatar>
            <q-icon name="payments" />
          </q-item-section>
          <q-item-section>Thanh toán hóa đơn</q-item-section>
        </q-item>

        <template v-if="authStore.hasRole('ADMIN')">
          <q-separator class="q-my-md" />
          <q-item-label header>HỆ THỐNG</q-item-label>

          <q-item clickable v-ripple to="/admin/users">
            <q-item-section avatar>
              <q-icon name="manage_accounts" />
            </q-item-section>
            <q-item-section>Người dùng (Staff/Admin)</q-item-section>
          </q-item>

          <q-item clickable v-ripple to="/admin/audit-logs">
            <q-item-section avatar>
              <q-icon name="policy" />
            </q-item-section>
            <q-item-section>Audit Logs</q-item-section>
          </q-item>
        </template>
      </q-list>
    </q-drawer>

    <q-page-container>
      <q-page padding>
        <slot />
      </q-page>
    </q-page-container>
  </q-layout>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useAuthStore } from '~/stores/auth'

const leftDrawerOpen = ref(false)
const authStore = useAuthStore()

const toggleLeftDrawer = () => {
  leftDrawerOpen.value = !leftDrawerOpen.value
}

const onLogout = async () => {
  await authStore.logout()
}
</script>
