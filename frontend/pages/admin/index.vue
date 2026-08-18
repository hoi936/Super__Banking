<template>
  <div class="q-pa-md">
    <div class="row items-center justify-between q-mb-md">
      <div class="text-h5 text-weight-bold">Dashboard Quản Trị</div>
      <q-btn
        flat
        color="primary"
        icon="refresh"
        label="Làm mới"
        @click="fetchDashboard"
        :loading="isLoading"
      />
    </div>

    <!-- Error state -->
    <q-banner v-if="hasError" inline-actions rounded class="bg-negative text-white q-mb-md">
      Có lỗi xảy ra khi tải dữ liệu. {{ errorMessage }}
      <template v-slot:action>
        <q-btn flat label="Thử lại" @click="fetchDashboard" />
      </template>
    </q-banner>

    <!-- Skeleton Loading -->
    <div v-if="isLoading" class="row q-col-gutter-md">
      <div class="col-12 col-md-6 col-lg-3" v-for="i in 4" :key="i">
        <q-card flat bordered>
          <q-card-section>
            <q-skeleton type="text" width="50%" />
            <q-skeleton type="text" class="text-h4 q-mt-sm" width="80%" />
          </q-card-section>
        </q-card>
      </div>
    </div>

    <!-- Dashboard Content -->
    <template v-else-if="dashboardData">
      <!-- 1. Customers -->
      <div class="text-h6 q-mb-sm q-mt-md">Khách hàng</div>
      <div class="row q-col-gutter-md q-mb-lg">
        <div class="col-12 col-sm-4">
          <q-card flat bordered class="bg-blue-1">
            <q-card-section>
              <div class="text-subtitle2 text-grey-8">Tổng khách hàng</div>
              <div class="text-h4 text-primary q-mt-sm">{{ dashboardData.customers.total }}</div>
            </q-card-section>
          </q-card>
        </div>
        <div class="col-12 col-sm-4">
          <q-card flat bordered class="bg-green-1">
            <q-card-section>
              <div class="text-subtitle2 text-grey-8">Đang hoạt động</div>
              <div class="text-h4 text-positive q-mt-sm">{{ dashboardData.customers.active }}</div>
            </q-card-section>
          </q-card>
        </div>
        <div class="col-12 col-sm-4">
          <q-card flat bordered class="bg-red-1">
            <q-card-section>
              <div class="text-subtitle2 text-grey-8">Bị tạm khóa</div>
              <div class="text-h4 text-negative q-mt-sm">{{ dashboardData.customers.suspended }}</div>
            </q-card-section>
          </q-card>
        </div>
      </div>

      <!-- 2. Accounts / Total Balance -->
      <div class="text-h6 q-mb-sm">Tài khoản & Số dư</div>
      <div class="row q-col-gutter-md q-mb-lg">
        <div class="col-12 col-sm-6 col-md-3">
          <q-card flat bordered>
            <q-card-section>
              <div class="text-subtitle2 text-grey-8">Tổng tài khoản</div>
              <div class="text-h4 text-weight-bold q-mt-sm">{{ dashboardData.accounts.total }}</div>
            </q-card-section>
          </q-card>
        </div>
        <div class="col-12 col-sm-6 col-md-3">
          <q-card flat bordered>
            <q-card-section>
              <div class="text-subtitle2 text-grey-8">Đang hoạt động</div>
              <div class="text-h4 text-positive q-mt-sm">{{ dashboardData.accounts.active }}</div>
            </q-card-section>
          </q-card>
        </div>
        <div class="col-12 col-sm-6 col-md-3">
          <q-card flat bordered>
            <q-card-section>
              <div class="text-subtitle2 text-grey-8">Bị khóa</div>
              <div class="text-h4 text-warning q-mt-sm">{{ dashboardData.accounts.locked }}</div>
            </q-card-section>
          </q-card>
        </div>
        <div class="col-12 col-sm-6 col-md-3">
          <q-card flat bordered class="bg-grey-2">
            <q-card-section>
              <div class="text-subtitle2 text-grey-8">Tổng số dư hệ thống</div>
              <div class="text-h5 text-weight-bold text-primary q-mt-sm">{{ formatCurrency(dashboardData.accounts.totalBalance) }}</div>
            </q-card-section>
          </q-card>
        </div>
      </div>

      <!-- 3. Transfers Today & Payments Today -->
      <div class="text-h6 q-mb-sm">Hoạt động trong ngày</div>
      <div class="row q-col-gutter-md q-mb-lg">
        <div class="col-12 col-md-6">
          <q-card flat bordered>
            <q-card-section>
              <div class="row items-center justify-between">
                <div>
                  <div class="text-subtitle2 text-grey-8">Giao dịch chuyển tiền (Hôm nay)</div>
                  <div class="text-h4 text-weight-bold q-mt-sm">{{ dashboardData.today.transfers }}</div>
                  <div class="text-subtitle1 text-primary q-mt-xs">{{ formatCurrency(dashboardData.today.transferVolume) }}</div>
                </div>
                <q-icon name="swap_horiz" size="3rem" color="primary" class="opacity-30" />
              </div>
            </q-card-section>
          </q-card>
        </div>
        <div class="col-12 col-md-6">
          <q-card flat bordered>
            <q-card-section>
              <div class="row items-center justify-between">
                <div>
                  <div class="text-subtitle2 text-grey-8">Thanh toán hóa đơn (Hôm nay)</div>
                  <div class="text-h4 text-weight-bold q-mt-sm">{{ dashboardData.today.payments }}</div>
                  <div class="text-subtitle1 text-orange-8 q-mt-xs">{{ formatCurrency(dashboardData.today.paymentVolume) }}</div>
                </div>
                <q-icon name="receipt_long" size="3rem" color="orange" class="opacity-30" />
              </div>
            </q-card-section>
          </q-card>
        </div>
      </div>

      <!-- Quick Actions -->
      <div class="text-h6 q-mb-sm">Lối tắt thao tác</div>
      <div class="row q-col-gutter-sm">
        <div class="col-auto">
          <q-btn outline color="primary" icon="people" label="Xem Khách hàng" to="/admin/customers" />
        </div>
        <div class="col-auto">
          <q-btn outline color="primary" icon="list_alt" label="Xem Giao dịch" to="/admin/transactions" />
        </div>
        <div class="col-auto">
          <q-btn outline color="primary" icon="payments" label="Xem Thanh toán" to="/admin/payments" />
        </div>
        <div class="col-auto" v-if="authStore.hasRole('ADMIN')">
          <q-btn outline color="secondary" icon="manage_accounts" label="Quản lý Users" to="/admin/users" />
        </div>
        <div class="col-auto" v-if="authStore.hasRole('ADMIN')">
          <q-btn outline color="grey-8" icon="policy" label="Xem Audit Logs" to="/admin/audit-logs" />
        </div>
      </div>

      <div class="text-caption text-grey-6 q-mt-xl text-right">
        Cập nhật lúc: {{ formatDateTime(dashboardData.generatedAtUtc) }}
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
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
.opacity-30 {
  opacity: 0.3;
}
</style>
