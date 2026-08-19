<template>
  <q-layout view="lHh Lpr lFf" class="admin-shell">
    <q-header class="admin-topbar">
      <q-toolbar class="admin-toolbar">
        <q-btn flat round dense icon="menu" class="admin-icon-btn q-mr-sm" @click="toggleLeftDrawer" />

        <q-toolbar-title class="admin-brand">
          <q-avatar square size="36px" class="admin-brand-mark">
            <q-icon name="account_balance" size="24px" />
          </q-avatar>
          <div>
            <div class="admin-brand-name"><span>Super</span>Banking</div>
            <div class="admin-brand-caption">Trung tâm quản trị vận hành</div>
          </div>
          <q-badge class="admin-role-badge" label="ADMIN" />
        </q-toolbar-title>

        <q-space />

        <div v-if="authStore.user" class="admin-user-menu">
          <div class="text-right hidden-xs">
            <div class="admin-user-name">{{ authStore.user.fullName || authStore.user.email }}</div>
            <div class="admin-user-role">{{ authStore.user.roles.join(', ') }}</div>
          </div>
          <q-avatar size="40px" color="primary" text-color="white" class="text-weight-bold">
            {{ (authStore.user.fullName || authStore.user.email || 'U').charAt(0).toUpperCase() }}
          </q-avatar>

          <q-menu transition-show="jump-down" transition-hide="jump-up" class="admin-menu-popover">
            <q-list style="min-width: 220px" class="q-py-sm">
              <q-item-label header class="text-weight-bold text-grey-8">Tài khoản</q-item-label>
              <q-item clickable v-close-popup to="/profile">
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

    <q-drawer v-model="leftDrawerOpen" show-if-above :width="288" class="admin-sidebar">
      <div class="admin-sidebar-head">
        <div class="admin-sidebar-title">Super Banking</div>
        <div class="admin-sidebar-subtitle">Admin Console</div>
      </div>

      <q-scroll-area class="fit">
        <q-list class="admin-nav">
          <q-item-label header>Hoạt động chính</q-item-label>

          <q-item clickable v-ripple to="/admin" exact active-class="admin-nav-active">
            <q-item-section avatar>
              <q-icon name="space_dashboard" />
            </q-item-section>
            <q-item-section>Dashboard</q-item-section>
          </q-item>

          <q-item clickable v-ripple to="/admin/customers" active-class="admin-nav-active">
            <q-item-section avatar>
              <q-icon name="group" />
            </q-item-section>
            <q-item-section>Khách hàng</q-item-section>
          </q-item>

          <q-item clickable v-ripple to="/admin/transactions" active-class="admin-nav-active">
            <q-item-section avatar>
              <q-icon name="receipt_long" />
            </q-item-section>
            <q-item-section>Giao dịch hệ thống</q-item-section>
          </q-item>

          <q-item clickable v-ripple to="/admin/payments" active-class="admin-nav-active">
            <q-item-section avatar>
              <q-icon name="payments" />
            </q-item-section>
            <q-item-section>Thanh toán hóa đơn</q-item-section>
          </q-item>

          <template v-if="authStore.hasRole('ADMIN')">
            <q-separator dark class="admin-nav-separator" />
            <q-item-label header>Quản lý hệ thống</q-item-label>

            <q-item clickable v-ripple to="/admin/users" active-class="admin-nav-active">
              <q-item-section avatar>
                <q-icon name="admin_panel_settings" />
              </q-item-section>
              <q-item-section>Nhân sự</q-item-section>
            </q-item>

            <q-item clickable v-ripple to="/admin/audit-logs" active-class="admin-nav-active">
              <q-item-section avatar>
                <q-icon name="policy" />
              </q-item-section>
              <q-item-section>Nhật ký truy cập</q-item-section>
            </q-item>
          </template>
        </q-list>
      </q-scroll-area>
    </q-drawer>

    <q-page-container>
      <q-page class="admin-content">
        <router-view v-slot="{ Component }">
          <transition name="admin-page-transition" mode="out-in">
            <component :is="Component" />
          </transition>
        </router-view>
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

<style>
:root {
  --admin-navy: #101828;
  --admin-blue: #1f6fd1;
  --admin-blue-soft: #e7f0ff;
  --admin-bg: #f4f7fb;
  --admin-card: #ffffff;
  --admin-border: #d9e2ef;
  --admin-muted: #667085;
  --admin-text: #1d2939;
}

.admin-shell {
  background: linear-gradient(180deg, rgba(231, 240, 255, 0.58) 0, rgba(244, 247, 251, 0) 260px), var(--admin-bg);
  color: var(--admin-text);
}

.admin-topbar {
  background: rgba(255, 255, 255, 0.94) !important;
  color: var(--admin-text);
  border-bottom: 1px solid var(--admin-border);
  box-shadow: 0 8px 22px rgba(16, 24, 40, 0.06);
  backdrop-filter: blur(10px);
}

.admin-toolbar {
  min-height: 68px;
  padding: 0 24px;
}

.admin-icon-btn {
  color: #475467;
}

.admin-brand {
  display: flex;
  align-items: center;
  gap: 12px;
  min-width: 0;
}

.admin-brand-mark {
  background: var(--admin-blue-soft);
  color: var(--admin-blue);
  border: 1px solid #c7dcff;
}

.admin-brand-name {
  color: var(--admin-text);
  font-size: 22px;
  font-weight: 800;
  line-height: 1.05;
}

.admin-brand-name span {
  color: var(--admin-blue);
}

.admin-brand-caption {
  color: var(--admin-muted);
  font-size: 12px;
  font-weight: 600;
  margin-top: 3px;
}

.admin-role-badge {
  background: #fff4e5;
  color: #b54708;
  border: 1px solid #fedf89;
  font-weight: 800;
  padding: 4px 8px;
}

.admin-user-menu {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 6px 8px 6px 14px;
  border: 1px solid transparent;
  border-radius: 8px;
  cursor: pointer;
}

.admin-user-menu:hover {
  background: #f8fafc;
  border-color: var(--admin-border);
}

.admin-user-name {
  color: var(--admin-text);
  font-size: 14px;
  font-weight: 700;
}

.admin-user-role {
  color: var(--admin-blue);
  font-size: 12px;
  font-weight: 700;
  text-transform: uppercase;
}

.admin-menu-popover {
  border: 1px solid var(--admin-border);
  border-radius: 8px;
  box-shadow: 0 18px 45px rgba(16, 24, 40, 0.14);
}

.admin-sidebar {
  background: var(--admin-navy);
  color: #f8fafc;
  border-right: 1px solid #263244;
}

.admin-sidebar-head {
  padding: 24px 24px 22px;
  border-bottom: 1px solid rgba(255, 255, 255, 0.08);
}

.admin-sidebar-title {
  font-size: 19px;
  font-weight: 800;
}

.admin-sidebar-subtitle {
  color: #98a2b3;
  font-size: 12px;
  font-weight: 700;
  margin-top: 4px;
  text-transform: uppercase;
}

.admin-nav {
  padding: 18px 12px 28px;
}

.admin-nav .q-item__label--header {
  color: #98a2b3;
  font-size: 11px;
  font-weight: 800;
  letter-spacing: 0;
  padding: 16px 12px 8px;
  text-transform: uppercase;
}

.admin-nav .q-item {
  min-height: 46px;
  margin: 2px 0;
  color: #cbd5e1;
  border-radius: 8px;
  font-weight: 650;
}

.admin-nav .q-item:hover {
  background: rgba(255, 255, 255, 0.06);
  color: #ffffff;
}

.admin-nav .q-item__section--avatar {
  min-width: 42px;
  color: #94a3b8;
}

.admin-nav-active {
  background: #1d4f8f !important;
  color: #ffffff !important;
}

.admin-nav-active .q-item__section--avatar {
  color: #ffffff;
}

.admin-nav-separator {
  margin: 18px 12px 6px;
  opacity: 0.22;
}

.admin-content {
  padding: 28px;
}

.admin-page {
  max-width: 1480px;
  margin: 0 auto;
}

.admin-page-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 20px;
  margin-bottom: 20px;
}

.admin-page-kicker {
  color: var(--admin-blue);
  font-size: 12px;
  font-weight: 800;
  margin-bottom: 6px;
  text-transform: uppercase;
}

.admin-page-title {
  color: var(--admin-text);
  font-size: clamp(28px, 3vw, 38px);
  font-weight: 850;
  line-height: 1.12;
}

.admin-page-subtitle {
  color: var(--admin-muted);
  font-size: 15px;
  margin-top: 8px;
}

.admin-card,
.admin-filter-card {
  background: var(--admin-card);
  border: 1px solid var(--admin-border);
  border-radius: 8px;
  box-shadow: 0 12px 28px rgba(16, 24, 40, 0.05) !important;
}

.admin-filter-card {
  box-shadow: none !important;
}

.admin-section-title {
  display: flex;
  align-items: center;
  gap: 10px;
  color: var(--admin-text);
  font-size: 18px;
  font-weight: 800;
  margin: 28px 0 12px;
}

.admin-section-title .q-icon {
  color: var(--admin-blue);
}

.admin-field-label {
  color: #475467;
  font-size: 11px;
  font-weight: 800;
  margin-bottom: 6px;
  text-transform: uppercase;
}

.admin-action-btn {
  border-radius: 8px;
  font-weight: 800;
}

.admin-soft-btn {
  background: #f2f4f7 !important;
  color: #344054 !important;
  border: 1px solid #d0d5dd;
}

.admin-table-card {
  overflow: hidden;
}

.premium-table {
  border-radius: 8px;
}

.premium-table .q-table__top,
.premium-table thead tr {
  background: #f8fafc;
}

.premium-table th {
  color: #475467 !important;
  font-size: 12px;
  font-weight: 800 !important;
  height: 48px;
  text-transform: uppercase;
}

.premium-table td {
  color: var(--admin-text);
  font-size: 14px;
  height: 58px;
  border-color: #eef2f7;
}

.premium-table tbody tr:hover {
  background: #f8fbff;
}

.admin-chip {
  border-radius: 999px;
  font-weight: 800;
}

.admin-detail-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 16px;
}

.admin-detail-card {
  height: 100%;
}

.admin-detail-card .q-card__section {
  padding: 20px;
}

.admin-detail-title {
  color: var(--admin-text);
  font-size: 17px;
  font-weight: 800;
  margin-bottom: 14px;
}

.admin-kv-list .q-item {
  min-height: 52px;
  padding: 10px 0;
}

.admin-kv-list .q-item__label--caption {
  color: var(--admin-muted);
  font-size: 12px;
  font-weight: 700;
}

.admin-money-panel {
  background: #f8fbff;
  border: 1px solid #d6e6ff;
  border-radius: 8px;
  padding: 22px;
  text-align: center;
}

.font-monospace {
  font-family: "SFMono-Regular", Consolas, "Liberation Mono", Menlo, monospace;
}

.admin-page-transition-enter-active,
.admin-page-transition-leave-active {
  transition: opacity 0.16s ease, transform 0.16s ease;
}

.admin-page-transition-enter-from,
.admin-page-transition-leave-to {
  opacity: 0;
  transform: translateY(8px);
}

@media (max-width: 1023px) {
  .admin-content {
    padding: 20px;
  }

  .admin-page-header {
    flex-direction: column;
  }

  .admin-page-header .q-btn {
    width: 100%;
  }

  .admin-detail-grid {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 599px) {
  .admin-toolbar {
    padding: 0 12px;
  }

  .admin-brand-caption,
  .admin-role-badge {
    display: none;
  }

  .admin-content {
    padding: 16px;
  }
}
</style>
