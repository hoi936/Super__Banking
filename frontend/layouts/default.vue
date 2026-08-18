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
        
        <q-item clickable v-ripple to="/dashboard" exact>
          <q-item-section avatar>
            <q-icon name="dashboard" />
          </q-item-section>
          <q-item-section>Tổng quan</q-item-section>
        </q-item>

        <q-item clickable v-ripple to="/accounts">
          <q-item-section avatar>
            <q-icon name="account_balance_wallet" />
          </q-item-section>
          <q-item-section>Tài khoản</q-item-section>
        </q-item>

        <q-item clickable v-ripple to="/transfers">
          <q-item-section avatar>
            <q-icon name="swap_horiz" />
          </q-item-section>
          <q-item-section>Chuyển tiền</q-item-section>
        </q-item>
        
        <q-item clickable v-ripple to="/transactions">
          <q-item-section avatar>
            <q-icon name="receipt_long" />
          </q-item-section>
          <q-item-section>Lịch sử giao dịch</q-item-section>
        </q-item>

        <q-item clickable v-ripple to="/bills">
          <q-item-section avatar>
            <q-icon name="receipt" />
          </q-item-section>
          <q-item-section>Hóa đơn & Thanh toán</q-item-section>
        </q-item>

        <q-item clickable v-ripple to="/notifications">
          <q-item-section avatar>
            <q-icon name="notifications" />
          </q-item-section>
          <q-item-section>Thông báo</q-item-section>
        </q-item>
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
