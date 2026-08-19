<template>
  <q-layout view="hHh Lpr lFf">
    <q-header elevated class="bg-primary text-white">
      <q-toolbar>
        <q-btn dense flat round icon="menu" @click="toggleLeftDrawer" />

        <q-toolbar-title>
          InterLink Banking
        </q-toolbar-title>

        <q-space />

        <div v-if="authStore.user" class="q-mr-sm text-right">
          <div class="text-subtitle2">{{ authStore.user.fullName }}</div>
          <div class="text-caption text-weight-light">{{ authStore.user.roles.join(', ') }}</div>
        </div>
        
        <q-btn flat round dense icon="notifications" class="q-mr-sm" to="/notifications">
          <q-badge v-if="unreadCount > 0" color="red" floating rounded>
            {{ unreadCount > 99 ? '99+' : unreadCount }}
          </q-badge>
        </q-btn>

        <q-btn flat round dense icon="account_circle">
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

    <q-drawer v-model="leftDrawerOpen" show-if-above bordered>
      <q-list padding>
        <q-item-label header>MENU CHÍNH</q-item-label>
        
        <q-item clickable v-ripple to="/dashboard" exact active-class="bg-blue-1 text-primary text-weight-bold">
          <q-item-section avatar>
            <q-icon name="dashboard" />
          </q-item-section>
          <q-item-section>Tổng quan</q-item-section>
        </q-item>

        <q-item clickable v-ripple to="/accounts" active-class="bg-blue-1 text-primary text-weight-bold">
          <q-item-section avatar>
            <q-icon name="account_balance_wallet" />
          </q-item-section>
          <q-item-section>Tài khoản</q-item-section>
        </q-item>

        <q-item clickable v-ripple to="/transfers" active-class="bg-blue-1 text-primary text-weight-bold">
          <q-item-section avatar>
            <q-icon name="swap_horiz" />
          </q-item-section>
          <q-item-section>Chuyển tiền</q-item-section>
        </q-item>
        
        <q-item clickable v-ripple to="/transactions" active-class="bg-blue-1 text-primary text-weight-bold">
          <q-item-section avatar>
            <q-icon name="receipt_long" />
          </q-item-section>
          <q-item-section>Lịch sử giao dịch</q-item-section>
        </q-item>

        <q-item clickable v-ripple to="/bills" active-class="bg-blue-1 text-primary text-weight-bold">
          <q-item-section avatar>
            <q-icon name="receipt" />
          </q-item-section>
          <q-item-section>Hóa đơn & Thanh toán</q-item-section>
        </q-item>

        <q-item clickable v-ripple to="/notifications" active-class="bg-blue-1 text-primary text-weight-bold">
          <q-item-section avatar>
            <q-icon name="notifications" />
          </q-item-section>
          <q-item-section>
            Thông báo
            <q-badge v-if="unreadCount > 0" color="red" class="q-ml-sm">{{ unreadCount }}</q-badge>
          </q-item-section>
        </q-item>
      </q-list>
    </q-drawer>

    <q-page-container>
      <q-page padding class="bg-grey-1">
        <slot />
      </q-page>
    </q-page-container>
  </q-layout>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
import { useAuthStore } from '~/stores/auth'
import { useNotificationService } from '~/services/notificationService'
import { useRoute } from 'vue-router'

const leftDrawerOpen = ref(false)
const authStore = useAuthStore()
const notificationService = useNotificationService()
const unreadCount = useState<number>('unreadNotificationCount', () => 0)
const route = useRoute()

const toggleLeftDrawer = () => {
  leftDrawerOpen.value = !leftDrawerOpen.value
}

const fetchUnreadCount = async () => {
  try {
    const res = await notificationService.getUnreadCount()
    unreadCount.value = res.count
  } catch (e) {
    console.error('Failed to fetch unread notifications count', e)
  }
}

onMounted(() => {
  if (authStore.isAuthenticated) {
    fetchUnreadCount()
  }
})

// Optional: refresh count on route change
watch(() => route.path, () => {
  if (authStore.isAuthenticated) {
    fetchUnreadCount()
  }
})

const onLogout = async () => {
  await authStore.logout()
}
</script>
