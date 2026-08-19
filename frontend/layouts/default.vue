<template>
  <q-layout view="lHh Lpr lFf" class="bank-shell">
    <q-header class="bank-topbar">
      <q-toolbar class="bank-toolbar">
        <q-btn dense flat round icon="menu" class="bank-icon-btn q-mr-sm" @click="toggleLeftDrawer" />

        <q-toolbar-title class="bank-brand">
          <q-avatar square size="36px" class="bank-brand-mark">
            <q-icon name="account_balance" size="24px" />
          </q-avatar>
          <div>
            <div class="bank-brand-name"><span>Super</span>Banking</div>
            <div class="bank-brand-caption">Ngân hàng số cá nhân</div>
          </div>
        </q-toolbar-title>

        <q-space />

        <q-btn flat round dense icon="notifications" class="bank-icon-btn q-mr-sm" to="/notifications">
          <q-badge v-if="unreadCount > 0" color="negative" floating rounded>
            {{ unreadCount > 99 ? '99+' : unreadCount }}
          </q-badge>
        </q-btn>

        <div v-if="authStore.user" class="bank-user-menu">
          <div class="text-right hidden-xs">
            <div class="bank-user-name">{{ authStore.user.fullName || authStore.user.email }}</div>
            <div class="bank-user-role">Khách hàng</div>
          </div>
          <q-avatar size="40px" color="primary" text-color="white" class="text-weight-bold">
            {{ (authStore.user.fullName || authStore.user.email || 'U').charAt(0).toUpperCase() }}
          </q-avatar>

          <q-menu transition-show="jump-down" transition-hide="jump-up" class="bank-menu-popover">
            <q-list style="min-width: 220px" class="q-py-sm text-dark bg-white">
              <q-item-label header class="text-weight-bold text-grey-8">Tài khoản</q-item-label>
              <q-item clickable v-close-popup to="/profile" class="text-dark">
                <q-item-section avatar>
                  <q-icon name="person_outline" />
                </q-item-section>
                <q-item-section>Hồ sơ cá nhân</q-item-section>
              </q-item>
              <q-separator class="q-my-sm" />
              <q-item clickable v-close-popup @click="onLogout" class="text-negative">
                <q-item-section avatar>
                  <q-icon name="logout" />
                </q-item-section>
                <q-item-section class="text-weight-medium">Đăng xuất</q-item-section>
              </q-item>
            </q-list>
          </q-menu>
        </div>
      </q-toolbar>
    </q-header>

    <q-drawer v-model="leftDrawerOpen" show-if-above :width="284" class="bank-sidebar">
      <div class="bank-sidebar-head">
        <div class="bank-sidebar-title">Super Banking</div>
        <div class="bank-sidebar-subtitle">Customer Portal</div>
      </div>

      <q-scroll-area class="fit">
        <q-list class="bank-nav">
          <q-item-label header>Tài chính cá nhân</q-item-label>

          <q-item clickable v-ripple to="/dashboard" exact active-class="bank-nav-active">
            <q-item-section avatar>
              <q-icon name="dashboard" />
            </q-item-section>
            <q-item-section>Tổng quan</q-item-section>
          </q-item>

          <q-item clickable v-ripple to="/accounts" active-class="bank-nav-active">
            <q-item-section avatar>
              <q-icon name="account_balance_wallet" />
            </q-item-section>
            <q-item-section>Tài khoản</q-item-section>
          </q-item>

          <q-item clickable v-ripple to="/transfer" active-class="bank-nav-active">
            <q-item-section avatar>
              <q-icon name="swap_horiz" />
            </q-item-section>
            <q-item-section>Chuyển tiền</q-item-section>
          </q-item>

          <q-item clickable v-ripple to="/transactions" active-class="bank-nav-active">
            <q-item-section avatar>
              <q-icon name="receipt_long" />
            </q-item-section>
            <q-item-section>Lịch sử giao dịch</q-item-section>
          </q-item>

          <q-separator dark class="bank-nav-separator" />
          <q-item-label header>Dịch vụ</q-item-label>

          <q-item clickable v-ripple to="/bills" active-class="bank-nav-active">
            <q-item-section avatar>
              <q-icon name="receipt" />
            </q-item-section>
            <q-item-section>Hóa đơn</q-item-section>
          </q-item>

          <q-item clickable v-ripple to="/payments" active-class="bank-nav-active">
            <q-item-section avatar>
              <q-icon name="payments" />
            </q-item-section>
            <q-item-section>Lịch sử thanh toán</q-item-section>
          </q-item>

          <q-item clickable v-ripple to="/notifications" active-class="bank-nav-active">
            <q-item-section avatar>
              <q-icon name="notifications" />
            </q-item-section>
            <q-item-section>
              <div class="row items-center no-wrap">
                <span>Thông báo</span>
                <q-badge v-if="unreadCount > 0" color="negative" class="q-ml-sm">{{ unreadCount }}</q-badge>
              </div>
            </q-item-section>
          </q-item>
        </q-list>
      </q-scroll-area>
    </q-drawer>

    <q-page-container>
      <q-page class="bank-content">
        <slot />
      </q-page>
    </q-page-container>
  </q-layout>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
import { useRoute } from 'vue-router'
import { useAuthStore } from '~/stores/auth'
import { useNotificationService } from '~/services/notificationService'

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

watch(() => route.path, () => {
  if (authStore.isAuthenticated) {
    fetchUnreadCount()
  }
})

const onLogout = async () => {
  await authStore.logout()
}
</script>

<style>
:root {
  --bank-navy: #101828;
  --bank-navy-2: #182230;
  --bank-blue: #1f6fd1;
  --bank-blue-soft: #e7f0ff;
  --bank-bg: #f4f7fb;
  --bank-card: #ffffff;
  --bank-border: #d9e2ef;
  --bank-muted: #667085;
  --bank-text: #1d2939;
}

.bank-shell {
  background: linear-gradient(180deg, rgba(231, 240, 255, 0.7) 0, rgba(244, 247, 251, 0) 280px), var(--bank-bg);
  color: var(--bank-text);
}

.bank-topbar {
  background: rgba(255, 255, 255, 0.94) !important;
  color: var(--bank-text);
  border-bottom: 1px solid var(--bank-border);
  box-shadow: 0 8px 22px rgba(16, 24, 40, 0.06);
  backdrop-filter: blur(10px);
}

.bank-toolbar {
  min-height: 68px;
  padding: 0 24px;
}

.bank-icon-btn {
  color: #475467;
}

.bank-brand {
  display: flex;
  align-items: center;
  gap: 12px;
  min-width: 0;
}

.bank-brand-mark {
  background: var(--bank-blue-soft);
  color: var(--bank-blue);
  border: 1px solid #c7dcff;
}

.bank-brand-name {
  color: var(--bank-text);
  font-size: 22px;
  font-weight: 800;
  line-height: 1.05;
}

.bank-brand-name span {
  color: var(--bank-blue);
}

.bank-brand-caption {
  color: var(--bank-muted);
  font-size: 12px;
  font-weight: 650;
  margin-top: 3px;
}

.bank-user-menu {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 6px 8px 6px 14px;
  border: 1px solid transparent;
  border-radius: 8px;
  cursor: pointer;
}

.bank-user-menu:hover {
  background: #f8fafc;
  border-color: var(--bank-border);
}

.bank-user-name {
  color: var(--bank-text);
  font-size: 14px;
  font-weight: 750;
}

.bank-user-role {
  color: var(--bank-blue);
  font-size: 12px;
  font-weight: 800;
  text-transform: uppercase;
}

.bank-menu-popover {
  border: 1px solid var(--bank-border);
  border-radius: 8px;
  box-shadow: 0 18px 45px rgba(16, 24, 40, 0.14);
}

.bank-sidebar {
  background: var(--bank-navy);
  color: #f8fafc;
  border-right: 1px solid #263244;
}

.bank-sidebar-head {
  padding: 24px 24px 22px;
  border-bottom: 1px solid rgba(255, 255, 255, 0.08);
}

.bank-sidebar-title {
  font-size: 19px;
  font-weight: 850;
}

.bank-sidebar-subtitle {
  color: #98a2b3;
  font-size: 12px;
  font-weight: 750;
  margin-top: 4px;
  text-transform: uppercase;
}

.bank-nav {
  padding: 18px 12px 28px;
}

.bank-nav .q-item__label--header {
  color: #98a2b3;
  font-size: 11px;
  font-weight: 800;
  letter-spacing: 0;
  padding: 16px 12px 8px;
  text-transform: uppercase;
}

.bank-nav .q-item {
  min-height: 46px;
  margin: 2px 0;
  color: #cbd5e1;
  border-radius: 8px;
  font-weight: 650;
}

.bank-nav .q-item:hover {
  background: rgba(255, 255, 255, 0.06);
  color: #ffffff;
}

.bank-nav .q-item__section--avatar {
  min-width: 42px;
  color: #94a3b8;
}

.bank-nav-active {
  background: #1d4f8f !important;
  color: #ffffff !important;
}

.bank-nav-active .q-item__section--avatar {
  color: #ffffff;
}

.bank-nav-separator {
  margin: 18px 12px 6px;
  opacity: 0.22;
}

.bank-content {
  padding: 28px;
}

.bank-page {
  max-width: 1240px;
  margin: 0 auto;
}

.bank-page-narrow {
  max-width: 860px;
  margin: 0 auto;
}

.bank-page-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 20px;
  margin-bottom: 20px;
}

.bank-page-kicker {
  color: var(--bank-blue);
  font-size: 12px;
  font-weight: 850;
  margin-bottom: 6px;
  text-transform: uppercase;
}

.bank-page-title {
  color: var(--bank-text);
  font-size: clamp(28px, 3vw, 38px);
  font-weight: 850;
  line-height: 1.12;
}

.bank-page-subtitle {
  color: var(--bank-muted);
  font-size: 15px;
  margin-top: 8px;
}

.bank-card,
.bank-filter-card {
  background: var(--bank-card);
  border: 1px solid var(--bank-border);
  border-radius: 8px;
  box-shadow: 0 12px 28px rgba(16, 24, 40, 0.05) !important;
}

.bank-filter-card {
  box-shadow: none !important;
}

.bank-action-btn {
  border-radius: 8px;
  font-weight: 800;
}

.bank-soft-btn {
  background: #f2f4f7 !important;
  color: #344054 !important;
  border: 1px solid #d0d5dd;
}

.bank-section-title {
  display: flex;
  align-items: center;
  gap: 10px;
  color: var(--bank-text);
  font-size: 18px;
  font-weight: 850;
  margin: 28px 0 12px;
}

.bank-section-title .q-icon {
  color: var(--bank-blue);
}

.bank-field-label {
  color: #475467;
  font-size: 11px;
  font-weight: 800;
  margin-bottom: 6px;
  text-transform: uppercase;
}

.bank-table-card {
  overflow: hidden;
}

.bank-table {
  border-radius: 8px;
}

.bank-table .q-table__top,
.bank-table thead tr {
  background: #f8fafc;
}

.bank-table th {
  color: #475467 !important;
  font-size: 12px;
  font-weight: 800 !important;
  height: 48px;
  text-transform: uppercase;
}

.bank-table td {
  color: var(--bank-text);
  font-size: 14px;
  height: 58px;
  border-color: #eef2f7;
}

.bank-table tbody tr:hover {
  background: #f8fbff;
}

.bank-chip {
  border-radius: 999px;
  font-weight: 800;
}

.bank-kv-list .q-item {
  min-height: 54px;
  padding: 10px 0;
}

.bank-kv-list .q-item__label--caption {
  color: var(--bank-muted);
  font-size: 12px;
  font-weight: 700;
}

.bank-money-panel {
  background: #f8fbff;
  border: 1px solid #d6e6ff;
  border-radius: 8px;
  padding: 24px;
  text-align: center;
}

.bank-mobile-list {
  background: #ffffff;
  border: 1px solid var(--bank-border);
  border-radius: 8px;
}

.font-mono,
.font-monospace {
  font-family: "SFMono-Regular", Consolas, "Liberation Mono", Menlo, monospace;
}

@media (max-width: 1023px) {
  .bank-content {
    padding: 20px;
  }

  .bank-page-header {
    flex-direction: column;
  }

  .bank-page-header .q-btn {
    width: 100%;
  }
}

@media (max-width: 599px) {
  .bank-toolbar {
    padding: 0 12px;
  }

  .bank-brand-caption {
    display: none;
  }

  .bank-content {
    padding: 16px;
  }
}
</style>
