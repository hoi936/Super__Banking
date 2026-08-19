<template>
  <div class="q-pa-md max-width-800 q-mx-auto">
    <div class="row items-center q-mb-md">
      <q-btn flat round dense icon="arrow_back" color="primary" @click="goBack" class="q-mr-sm" />
      <div class="text-h6 text-primary">Chi tiết thanh toán</div>
    </div>

    <div v-if="isLoading" class="text-center q-pa-xl">
      <q-spinner color="primary" size="3em" />
    </div>

    <AppAlert v-else-if="error" :message="error" type="error" />

    <q-card v-else-if="payment" flat bordered class="bg-white">
      <q-card-section class="text-center q-pb-none">
        <div class="text-subtitle1 text-grey-8">Thanh toán hóa đơn</div>
        <div class="text-h4 text-weight-bold text-negative q-my-md">
          -{{ formatCurrency(payment.amount) }}
        </div>
        <q-chip :color="getStatusColor(payment.status)" text-color="white">
          {{ getStatusLabel(payment.status) }}
        </q-chip>
      </q-card-section>

      <q-card-section>
        <q-list separator>
          <q-item>
            <q-item-section>
              <q-item-label caption>Mã tham chiếu</q-item-label>
              <q-item-label class="text-weight-medium">
                {{ payment.referenceNumber }}
                <q-btn flat dense round icon="content_copy" size="xs" color="grey-7" @click="copyToClipboard(payment.referenceNumber)" />
              </q-item-label>
            </q-item-section>
          </q-item>
          
          <q-item>
            <q-item-section>
              <q-item-label caption>Hóa đơn</q-item-label>
              <q-item-label class="text-weight-medium">{{ payment.providerName }} ({{ payment.billNumber }})</q-item-label>
              <q-item-label v-if="payment.billType" class="text-grey-8">{{ getBillTypeLabel(payment.billType) }}</q-item-label>
            </q-item-section>
            <q-item-section side>
              <q-btn flat dense color="primary" icon="receipt" size="sm" :to="`/bills/${payment.billId}`" />
            </q-item-section>
          </q-item>

          <q-item>
            <q-item-section>
              <q-item-label caption>Tài khoản thanh toán</q-item-label>
              <q-item-label class="text-weight-medium font-mono">{{ payment.accountNumber }}</q-item-label>
            </q-item-section>
          </q-item>

          <q-item>
            <q-item-section>
              <q-item-label caption>Thời gian tạo</q-item-label>
              <q-item-label>{{ formatDateTime(payment.createdAtUtc) }}</q-item-label>
            </q-item-section>
          </q-item>

          <q-item v-if="payment.paidAtUtc">
            <q-item-section>
              <q-item-label caption>Thời gian hoàn tất</q-item-label>
              <q-item-label>{{ formatDateTime(payment.paidAtUtc) }}</q-item-label>
            </q-item-section>
          </q-item>
        </q-list>
      </q-card-section>
    </q-card>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useQuasar } from 'quasar'
import AppAlert from '~/components/common/AppAlert.vue'
import { formatCurrency } from '~/utils/currency'
import { formatDateTime } from '~/utils/date'
import { getTransactionStatusColor as getStatusColor, getTransactionStatusLabel as getStatusLabel } from '~/utils/status'
import { usePaymentService } from '~/services/paymentService'
import type { PaymentDetailDto } from '~/types/payment'

definePageMeta({
  layout: 'default',
  middleware: ['customer']
})

const route = useRoute()
const router = useRouter()
const $q = useQuasar()
const paymentService = usePaymentService()

const payment = ref<PaymentDetailDto | null>(null)
const isLoading = ref(true)
const error = ref('')

onMounted(async () => {
  const id = route.params.id as string
  if (!id) {
    error.value = 'Mã giao dịch thanh toán không hợp lệ'
    isLoading.value = false
    return
  }

  try {
    payment.value = await paymentService.getPayment(id)
  } catch (err: any) {
    if (err.response?.status === 404) {
      error.value = 'Không tìm thấy giao dịch thanh toán.'
    } else {
      error.value = 'Đã có lỗi xảy ra khi tải dữ liệu thanh toán.'
    }
  } finally {
    isLoading.value = false
  }
})

const goBack = () => {
  if (window.history.length > 2) {
    router.back()
  } else {
    router.push('/payments')
  }
}

const copyToClipboard = async (text: string) => {
  try {
    await navigator.clipboard.writeText(text)
    $q.notify({ message: 'Đã sao chép vào khay nhớ tạm', color: 'positive', position: 'bottom' })
  } catch (err) {}
}

const getBillTypeLabel = (type: string) => {
  switch (type) {
    case 'ELECTRICITY': return 'Điện'
    case 'WATER': return 'Nước'
    case 'INTERNET': return 'Internet'
    case 'EDUCATION': return 'Giáo dục'
    case 'OTHER': return 'Khác'
    default: return type
  }
}
</script>

<style scoped>
.max-width-800 {
  max-width: 800px;
}
.font-mono {
  font-family: monospace;
}
</style>
