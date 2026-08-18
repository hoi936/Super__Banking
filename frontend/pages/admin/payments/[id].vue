<template>
  <div class="q-pa-md">
    <div class="row items-center justify-between q-mb-md">
      <div class="row items-center">
        <q-btn flat round dense icon="arrow_back" @click="router.back()" class="q-mr-sm" />
        <div class="text-h5 text-weight-bold">Chi tiết Thanh toán</div>
      </div>
      <q-btn flat color="primary" icon="refresh" label="Làm mới" @click="fetchPayment" />
    </div>

    <!-- Error state -->
    <q-banner v-if="hasError" inline-actions rounded class="bg-negative text-white q-mb-md">
      Có lỗi xảy ra khi tải dữ liệu. {{ errorMessage }}
      <template v-slot:action>
        <q-btn flat label="Thử lại" @click="fetchPayment" />
      </template>
    </q-banner>

    <!-- Skeleton Loading -->
    <div v-if="isLoading">
      <q-card flat bordered class="q-mb-md">
        <q-card-section>
          <q-skeleton type="text" width="30%" class="text-h6" />
          <q-skeleton type="text" width="60%" class="q-mt-md" />
          <q-skeleton type="text" width="50%" />
        </q-card-section>
      </q-card>
    </div>

    <template v-else-if="payment">
      <div class="row q-col-gutter-md">
        <!-- Thông tin hóa đơn / thanh toán -->
        <div class="col-12 col-md-6">
          <q-card flat bordered class="h-100">
            <q-card-section>
              <div class="text-h6 q-mb-md">Thông tin Hóa đơn</div>
              
              <div class="flex flex-center q-mb-lg">
                <div class="text-center">
                  <div class="text-subtitle1 text-grey-7">Số tiền thanh toán</div>
                  <div class="text-h3 text-weight-bold text-orange-8">
                    {{ formatCurrency(payment.amount, payment.currency) }}
                  </div>
                  <div class="q-mt-sm">
                    <q-badge :color="getStatusColor(payment.status)">
                      {{ getStatusLabel(payment.status) }}
                    </q-badge>
                  </div>
                </div>
              </div>

              <q-list dense>
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

        <!-- Thông tin Giao dịch / Khách hàng -->
        <div class="col-12 col-md-6">
          <q-card flat bordered class="h-100">
            <q-card-section>
              <div class="text-h6 q-mb-md">Thông tin Giao dịch</div>
              
              <q-list dense>
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
