<template>
  <div class="admin-page">
    <div class="admin-page-header">
      <div>
        <div class="admin-page-kicker">Tổng quan vận hành</div>
        <div class="admin-page-title">Dashboard quản trị</div>
        <div class="admin-page-subtitle">Theo dõi khách hàng, tài khoản, dòng tiền và hoạt động trong ngày của Super Banking.</div>
      </div>
      <q-btn
        unelevated
        color="primary"
        icon="refresh"
        label="Cập nhật dữ liệu"
        class="admin-action-btn"
        padding="10px 18px"
        @click="fetchDashboard"
        :loading="isLoading"
      />
    </div>

    <q-banner v-if="hasError" inline-actions rounded class="bg-red-1 text-negative q-mb-lg">
      <template v-slot:avatar>
        <q-icon name="error_outline" color="negative" />
      </template>
      <div class="text-weight-medium">Có lỗi xảy ra khi tải dữ liệu!</div>
      <div class="text-caption">{{ errorMessage }}</div>
      <template v-slot:action>
        <q-btn flat color="negative" label="Thử lại" @click="fetchDashboard" />
      </template>
    </q-banner>

    <div v-if="isLoading" class="row q-col-gutter-lg q-mb-xl">
      <div class="col-12 col-sm-6 col-lg-3" v-for="i in 4" :key="'skel1-'+i">
        <q-card flat class="admin-card">
          <q-card-section>
            <q-skeleton type="text" width="40%" />
            <q-skeleton type="text" class="text-h3 q-mt-md" width="70%" />
          </q-card-section>
        </q-card>
      </div>
    </div>

    <template v-else-if="dashboardData">
      <div class="row q-col-gutter-md">
        <div class="col-12 col-sm-6 col-lg-3">
          <q-card flat class="admin-card ops-tile">
            <q-card-section>
              <div class="row items-center justify-between q-mb-md">
                <div class="ops-label">Tổng khách hàng</div>
                <q-icon name="groups" class="ops-icon text-primary" />
              </div>
              <div class="ops-value">{{ dashboardData.customers.total }}</div>
              <div class="ops-note">{{ dashboardData.customers.active }} đang hoạt động</div>
            </q-card-section>
          </q-card>
        </div>
        <div class="col-12 col-sm-6 col-lg-3">
          <q-card flat class="admin-card ops-tile">
            <q-card-section>
              <div class="row items-center justify-between q-mb-md">
                <div class="ops-label">Tổng tài khoản</div>
                <q-icon name="account_balance_wallet" class="ops-icon text-primary" />
              </div>
              <div class="ops-value">{{ dashboardData.accounts.total }}</div>
              <div class="ops-note">{{ dashboardData.accounts.locked }} tài khoản bị khóa</div>
            </q-card-section>
          </q-card>
        </div>
        <div class="col-12 col-sm-6 col-lg-3">
          <q-card flat class="admin-card ops-tile">
            <q-card-section>
              <div class="row items-center justify-between q-mb-md">
                <div class="ops-label">Giao dịch hôm nay</div>
                <q-icon name="swap_horiz" class="ops-icon text-positive" />
              </div>
              <div class="ops-value">{{ dashboardData.today.transfers }}</div>
              <div class="ops-note">{{ formatCurrency(dashboardData.today.transferVolume) }}</div>
            </q-card-section>
          </q-card>
        </div>
        <div class="col-12 col-sm-6 col-lg-3">
          <q-card flat class="admin-card ops-tile ops-tile-dark">
            <q-card-section>
              <div class="row items-center justify-between q-mb-md">
                <div class="ops-label">Tổng số dư hệ thống</div>
                <q-icon name="savings" class="ops-icon" />
              </div>
              <div class="ops-value ops-money">{{ formatCurrency(dashboardData.accounts.totalBalance) }}</div>
              <div class="ops-note">{{ dashboardData.accounts.active }} tài khoản hoạt động</div>
            </q-card-section>
          </q-card>
        </div>
      </div>

      <div class="admin-section-title">
        <q-icon name="account_tree" />
        <span>Cấu trúc hệ thống</span>
      </div>

      <div class="row q-col-gutter-md">
        <div class="col-12 col-sm-6 col-md-3">
          <q-card flat class="admin-card status-card">
            <q-card-section class="q-pa-lg">
              <div class="status-label">Khách hàng hoạt động</div>
              <div class="status-value text-positive">{{ dashboardData.customers.active }}</div>
              <q-linear-progress rounded size="8px" color="positive" :value="customerActiveRatio" class="q-mt-md" />
            </q-card-section>
          </q-card>
        </div>
        <div class="col-12 col-sm-6 col-md-3">
          <q-card flat class="admin-card status-card">
            <q-card-section class="q-pa-lg">
              <div class="status-label">Khách hàng tạm khóa</div>
              <div class="status-value text-negative">{{ dashboardData.customers.suspended }}</div>
              <q-linear-progress rounded size="8px" color="negative" :value="customerSuspendedRatio" class="q-mt-md" />
            </q-card-section>
          </q-card>
        </div>
        <div class="col-12 col-sm-6 col-md-3">
          <q-card flat class="admin-card status-card">
            <q-card-section class="q-pa-lg">
              <div class="status-label">Tài khoản hoạt động</div>
              <div class="status-value text-positive">{{ dashboardData.accounts.active }}</div>
              <q-linear-progress rounded size="8px" color="primary" :value="accountActiveRatio" class="q-mt-md" />
            </q-card-section>
          </q-card>
        </div>
        <div class="col-12 col-sm-6 col-md-3">
          <q-card flat class="admin-card status-card">
            <q-card-section class="q-pa-lg">
              <div class="status-label">Tài khoản bị khóa</div>
              <div class="status-value text-warning">{{ dashboardData.accounts.locked }}</div>
              <q-linear-progress rounded size="8px" color="warning" :value="accountLockedRatio" class="q-mt-md" />
            </q-card-section>
          </q-card>
        </div>
      </div>

      <div class="row q-col-gutter-md q-mt-md">
        <div class="col-12 col-lg-8">
          <div class="admin-section-title">
            <q-icon name="monitoring" />
            <span>Hoạt động hôm nay</span>
          </div>

          <q-card flat class="admin-card">
            <q-card-section class="q-pa-none">
              <div class="activity-row">
                <q-avatar size="44px" color="blue-1" text-color="primary" icon="swap_horiz" />
                <div class="activity-copy">
                  <div class="activity-title">Chuyển tiền</div>
                  <div class="activity-meta">{{ dashboardData.today.transfers }} giao dịch</div>
                </div>
                <div class="activity-amount text-primary">{{ formatCurrency(dashboardData.today.transferVolume) }}</div>
              </div>
              <q-separator />
              <div class="activity-row">
                <q-avatar size="44px" color="orange-1" text-color="orange-8" icon="receipt_long" />
                <div class="activity-copy">
                  <div class="activity-title">Thanh toán hóa đơn</div>
                  <div class="activity-meta">{{ dashboardData.today.payments }} giao dịch</div>
                </div>
                <div class="activity-amount text-orange-8">{{ formatCurrency(dashboardData.today.paymentVolume) }}</div>
              </div>
            </q-card-section>
          </q-card>
        </div>

        <div class="col-12 col-lg-4">
          <div class="admin-section-title">
            <q-icon name="bolt" />
            <span>Lối tắt truy cập</span>
          </div>

          <q-card flat class="admin-card">
            <q-list separator>
              <q-item clickable v-ripple to="/admin/customers" class="q-py-md">
                <q-item-section avatar>
                  <q-avatar color="blue-1" text-color="primary" icon="people" />
                </q-item-section>
                <q-item-section>
                  <q-item-label class="text-weight-bold text-dark">Quản lý Khách hàng</q-item-label>
                  <q-item-label caption>Xem chi tiết danh sách khách hàng</q-item-label>
                </q-item-section>
                <q-item-section side>
                  <q-icon name="chevron_right" color="grey" />
                </q-item-section>
              </q-item>

              <q-item clickable v-ripple to="/admin/transactions" class="q-py-md">
                <q-item-section avatar>
                  <q-avatar color="green-1" text-color="positive" icon="list_alt" />
                </q-item-section>
                <q-item-section>
                  <q-item-label class="text-weight-bold text-dark">Lịch sử Giao dịch</q-item-label>
                  <q-item-label caption>Tra cứu chuyển nhận tiền</q-item-label>
                </q-item-section>
                <q-item-section side>
                  <q-icon name="chevron_right" color="grey" />
                </q-item-section>
              </q-item>

              <q-item v-if="authStore.hasRole('ADMIN')" clickable v-ripple to="/admin/users" class="q-py-md">
                <q-item-section avatar>
                  <q-avatar color="purple-1" text-color="purple" icon="admin_panel_settings" />
                </q-item-section>
                <q-item-section>
                  <q-item-label class="text-weight-bold text-dark">Quản lý Nhân sự</q-item-label>
                  <q-item-label caption>Phân quyền Staff/Admin</q-item-label>
                </q-item-section>
                <q-item-section side>
                  <q-icon name="chevron_right" color="grey" />
                </q-item-section>
              </q-item>
            </q-list>
          </q-card>
        </div>
      </div>

      <div class="text-caption text-grey-6 q-mt-lg text-right">
        Dữ liệu được làm mới lần cuối lúc: {{ formatDateTime(dashboardData.generatedAtUtc) }}
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, onMounted } from 'vue'
import { useAuthStore } from '~/stores/auth'
import { useAdminDashboardService } from '~/services/adminDashboardService'
import type { AdminDashboardDto } from '~/types/adminDashboard'
import { formatCurrency } from '~/utils/currency'
import { formatDateTime } from '~/utils/date'

definePageMeta({
  layout: 'admin',
  middleware: ['admin']
})

const authStore = useAuthStore()
const dashboardService = useAdminDashboardService()

const isLoading = ref(true)
const hasError = ref(false)
const errorMessage = ref('')
const dashboardData = ref<AdminDashboardDto | null>(null)

const ratio = (value: number, total: number) => {
  if (!total) return 0
  return Math.min(value / total, 1)
}

const customerActiveRatio = computed(() =>
  ratio(dashboardData.value?.customers.active || 0, dashboardData.value?.customers.total || 0)
)
const customerSuspendedRatio = computed(() =>
  ratio(dashboardData.value?.customers.suspended || 0, dashboardData.value?.customers.total || 0)
)
const accountActiveRatio = computed(() =>
  ratio(dashboardData.value?.accounts.active || 0, dashboardData.value?.accounts.total || 0)
)
const accountLockedRatio = computed(() =>
  ratio(dashboardData.value?.accounts.locked || 0, dashboardData.value?.accounts.total || 0)
)

const fetchDashboard = async () => {
  isLoading.value = true
  hasError.value = false
  errorMessage.value = ''

  try {
    dashboardData.value = await dashboardService.getDashboard()
  } catch (error: any) {
    console.error('Lỗi tải dashboard:', error)
    hasError.value = true
    errorMessage.value = error.data?.title || 'Không thể tải dữ liệu.'
  } finally {
    isLoading.value = false
  }
}

onMounted(() => {
  fetchDashboard()
})
</script>

<style scoped>
.ops-tile .q-card__section {
  padding: 20px;
}

.ops-label,
.status-label {
  color: #667085;
  font-size: 12px;
  font-weight: 800;
  text-transform: uppercase;
}

.ops-icon {
  font-size: 26px;
}

.ops-value {
  color: #101828;
  font-size: 34px;
  font-weight: 850;
  line-height: 1.1;
}

.ops-note {
  color: #667085;
  font-size: 13px;
  font-weight: 650;
  margin-top: 8px;
}

.ops-tile-dark {
  background: #101828;
  color: #ffffff;
}

.ops-tile-dark .ops-label,
.ops-tile-dark .ops-note {
  color: #cbd5e1;
}

.ops-tile-dark .ops-value,
.ops-tile-dark .ops-icon {
  color: #93c5fd;
}

.ops-money {
  font-size: 25px;
  white-space: nowrap;
}

.status-card .q-card__section {
  min-height: 130px;
}

.status-value {
  color: #101828;
  font-size: 32px;
  font-weight: 850;
  margin-top: 8px;
}

.activity-row {
  display: flex;
  align-items: center;
  gap: 14px;
  padding: 18px 20px;
}

.activity-copy {
  flex: 1;
  min-width: 0;
}

.activity-title {
  color: #101828;
  font-weight: 800;
}

.activity-meta {
  color: #667085;
  font-size: 13px;
  margin-top: 2px;
}

.activity-amount {
  font-size: 18px;
  font-weight: 850;
  text-align: right;
}

@media (max-width: 599px) {
  .activity-row {
    align-items: flex-start;
    flex-direction: column;
  }

  .activity-amount {
    text-align: left;
  }
}
</style>
