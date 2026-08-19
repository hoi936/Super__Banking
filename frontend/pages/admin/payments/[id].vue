<template>
  <div class="admin-page">
    <div class="admin-page-header">
      <div class="row items-center">
        <q-btn flat round dense icon="arrow_back" @click="router.back()" class="admin-soft-btn q-mr-sm" />
        <div>
          <div class="admin-page-kicker">Dịch vụ tiện ích</div>
          <div class="admin-page-title">Chi tiết thanh toán</div>
          <div class="admin-page-subtitle">Kiểm tra nhà cung cấp, hóa đơn, tài khoản trích tiền và giao dịch liên kết.</div>
        </div>
      </div>
      <q-btn unelevated color="primary" icon="refresh" label="Làm mới" class="admin-action-btn" @click="fetchPayment" />
    </div>

    <q-banner v-if="hasError" inline-actions rounded class="bg-red-1 text-negative q-mb-md">
      Có lỗi xảy ra khi tải dữ liệu. {{ errorMessage }}
      <template v-slot:action>
        <q-btn flat label="Thử lại" @click="fetchPayment" />
      </template>
    </q-banner>

    <!-- Skeleton Loading -->
    <div v-if="isLoading">
      <q-card flat class="admin-card q-mb-md">
        <q-card-section>
          <q-skeleton type="text" width="30%" class="text-h6" />
          <q-skeleton type="text" width="60%" class="q-mt-md" />
          <q-skeleton type="text" width="50%" />
        </q-card-section>
      </q-card>
    </div>

    <template v-else-if="payment">
      <div class="admin-detail-grid">
        <div class="col-12 col-md-6">
          <q-card flat class="admin-card admin-detail-card">
            <q-card-section>
              <div class="admin-detail-title">Thông tin hóa đơn</div>
              
              <div class="admin-money-panel q-mb-lg">
                <div class="text-center">
                  <div class="text-subtitle1 text-grey-7">Số tiền thanh toán</div>
                  <div class="text-h3 text-weight-bold text-orange-8">
                    {{ formatCurrency(payment.amount, payment.currency) }}
                  </div>
                  <div class="q-mt-sm">
                    <q-badge :color="getStatusColor(payment.status)" class="admin-chip">
                      {{ getStatusLabel(payment.status) }}
                    </q-badge>
                  </div>
                </div>
              </div>

              <q-list dense class="admin-kv-list">
                <q-item>
                  <q-item-section>
                    <q-item-label caption>Nhà cung cấp</q-item-label>
                    <q-item-label class="text-weight-medium">{{ payment.providerName }}</q-item-label>
                  </q-item-section>
                </q-item>
                <q-item>
                  <q-item-section>
                    <q-item-label caption>Dịch vụ</q-item-label>
                    <q-item-label>
                      <div class="row items-center no-wrap">
                        <q-icon :name="getBillTypeIcon(payment.billType)" size="xs" class="q-mr-xs text-grey-7" />
                        <span>{{ getBillTypeLabel(payment.billType) }}</span>
                      </div>
                    </q-item-label>
                  </q-item-section>
                </q-item>
                <q-item>
                  <q-item-section>
                    <q-item-label caption>Mã hóa đơn</q-item-label>
                    <q-item-label class="text-weight-medium">{{ payment.billNumber }}</q-item-label>
                  </q-item-section>
                </q-item>
                <q-item>
                  <q-item-section>
                    <q-item-label caption>Mã thanh toán (Payment Ref)</q-item-label>
                    <q-item-label>{{ payment.paymentReference }}</q-item-label>
                  </q-item-section>
                </q-item>
              </q-list>
            </q-card-section>
          </q-card>
        </div>

        <div class="col-12 col-md-6">
          <q-card flat class="admin-card admin-detail-card">
            <q-card-section>
              <div class="admin-detail-title">Thông tin giao dịch</div>
              
              <q-list dense class="admin-kv-list">
                <q-item>
                  <q-item-section>
                    <q-item-label caption>Khách hàng thanh toán</q-item-label>
                    <q-item-label class="text-weight-medium">
                      {{ payment.customerFullName }} 
                      <span class="text-caption text-grey-6">({{ payment.customerCode }})</span>
                    </q-item-label>
                  </q-item-section>
                </q-item>
                <q-item>
                  <q-item-section>
                    <q-item-label caption>Tài khoản trích tiền</q-item-label>
                    <q-item-label>{{ payment.accountNumber }}</q-item-label>
                  </q-item-section>
                </q-item>
                <q-separator spaced />
                <q-item>
                  <q-item-section>
                    <q-item-label caption>Mã giao dịch (Tx Ref)</q-item-label>
                    <q-item-label>
                      <template v-if="payment.transactionReference">
                        {{ payment.transactionReference }}
                      </template>
                      <span v-else class="text-grey-6 text-italic">Chưa có giao dịch</span>
                    </q-item-label>
                  </q-item-section>
                </q-item>
                <q-item>
                  <q-item-section>
                    <q-item-label caption>Idempotency Key</q-item-label>
                    <q-item-label class="text-caption text-grey-8">{{ payment.idempotencyKey || 'N/A' }}</q-item-label>
                  </q-item-section>
                </q-item>
                <q-item>
                  <q-item-section>
                    <q-item-label caption>Ngày tạo</q-item-label>
                    <q-item-label>{{ formatDateTime(payment.createdAtUtc) }}</q-item-label>
                  </q-item-section>
                </q-item>
                <q-item v-if="payment.paidAtUtc">
                  <q-item-section>
                    <q-item-label caption>Ngày hoàn tất</q-item-label>
                    <q-item-label>{{ formatDateTime(payment.paidAtUtc) }}</q-item-label>
                  </q-item-section>
                </q-item>
              </q-list>
            </q-card-section>
          </q-card>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAdminPaymentService } from '~/services/adminPaymentService'
import type { AdminPaymentDetailDto } from '~/types/adminPayment'
import { formatCurrency } from '~/utils/currency'
import { formatDateTime } from '~/utils/date'
import { getTransactionStatusColor as getStatusColor, getTransactionStatusLabel as getStatusLabel } from '~/utils/status'

definePageMeta({
  layout: 'admin',
  middleware: ['admin']
})

const route = useRoute()
const router = useRouter()
const paymentService = useAdminPaymentService()

const paymentId = route.params.id as string
const payment = ref<AdminPaymentDetailDto | null>(null)
const isLoading = ref(true)
const hasError = ref(false)
const errorMessage = ref('')

const fetchPayment = async () => {
  isLoading.value = true
  hasError.value = false
  
  try {
    payment.value = await paymentService.getPayment(paymentId)
  } catch (error: any) {
    console.error('Lỗi lấy chi tiết thanh toán:', error)
    hasError.value = true
    errorMessage.value = error.data?.title || 'Không thể lấy thông tin thanh toán'
  } finally {
    isLoading.value = false
  }
}

const getBillTypeIcon = (type: string) => {
  switch (type) {
    case 'ELECTRICITY': return 'bolt'
    case 'WATER': return 'water_drop'
    case 'INTERNET': return 'wifi'
    case 'EDUCATION': return 'school'
    default: return 'receipt'
  }
}

const getBillTypeLabel = (type: string) => {
  switch (type) {
    case 'ELECTRICITY': return 'Điện lực'
    case 'WATER': return 'Cấp nước'
    case 'INTERNET': return 'Internet/Viễn thông'
    case 'EDUCATION': return 'Giáo dục'
    default: return 'Khác'
  }
}

onMounted(() => {
  fetchPayment()
})
</script>

<style scoped>
.h-100 {
  height: 100%;
}
</style>
