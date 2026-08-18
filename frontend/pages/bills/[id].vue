<template>
  <div class="q-pa-md max-width-800 q-mx-auto">
    <div class="row items-center q-mb-md">
      <q-btn flat round dense icon="arrow_back" color="primary" @click="goBack" class="q-mr-sm" />
      <div class="text-h6 text-primary">Chi tiết hóa đơn</div>
    </div>

    <div v-if="isLoading" class="text-center q-pa-xl">
      <q-spinner color="primary" size="3em" />
    </div>

    <AppAlert v-else-if="error" :message="error" type="error" />

    <q-stepper
      v-else-if="bill"
      v-model="step"
      ref="stepper"
      color="primary"
      animated
      flat
      bordered
    >
      <!-- Step 1: Chi tiết hóa đơn -->
      <q-step
        :name="1"
        title="Thông tin hóa đơn"
        icon="receipt"
        :done="step > 1"
      >
        <q-list separator class="bg-grey-1 rounded-borders">
          <q-item>
            <q-item-section>
              <q-item-label caption>Nhà cung cấp</q-item-label>
              <q-item-label class="text-weight-bold">{{ bill.providerName }}</q-item-label>
            </q-item-section>
          </q-item>
          <q-item>
            <q-item-section>
              <q-item-label caption>Mã hóa đơn</q-item-label>
              <q-item-label class="text-weight-bold">{{ bill.billNumber }}</q-item-label>
            </q-item-section>
          </q-item>
          <q-item>
            <q-item-section>
              <q-item-label caption>Số tiền</q-item-label>
              <q-item-label class="text-weight-bold text-h6 text-primary">{{ formatCurrency(bill.amount) }}</q-item-label>
            </q-item-section>
          </q-item>
          <q-item>
            <q-item-section>
              <q-item-label caption>Hạn thanh toán</q-item-label>
              <q-item-label>{{ formatDate(bill.dueDate) }}</q-item-label>
            </q-item-section>
          </q-item>
          <q-item>
            <q-item-section>
              <q-item-label caption>Trạng thái</q-item-label>
              <q-item-label>
                <q-chip :color="getStatusColor(bill.status)" text-color="white">
                  {{ getStatusLabel(bill.status) }}
                </q-chip>
              </q-item-label>
            </q-item-section>
          </q-item>
        </q-list>

        <q-stepper-navigation class="row justify-end q-mt-md">
          <q-btn v-if="canPay" @click="goToStep2" color="primary" label="Thanh toán ngay" />
          <q-btn v-else-if="bill.status === 'PAID'" color="positive" label="Đã thanh toán" disable />
          <q-btn v-else-if="bill.status === 'CANCELLED'" color="grey" label="Đã hủy" disable />
        </q-stepper-navigation>
      </q-step>

      <!-- Step 2: Thanh toán -->
      <q-step
        :name="2"
        title="Thanh toán"
        icon="payment"
        :done="step > 2"
      >
        <div class="q-mb-md text-subtitle1 text-weight-bold">Chọn tài khoản thanh toán</div>
        <q-select
          v-model="selectedAccountId"
          :options="accounts"
          option-value="id"
          :option-label="opt => `${opt.accountNumber} - ${opt.accountName} (${formatCurrency(opt.balance)})`"
          emit-value
          map-options
          outlined
          label="Tài khoản nguồn"
          :rules="[val => !!val || 'Vui lòng chọn tài khoản thanh toán']"
        >
          <template v-slot:option="scope">
            <q-item v-bind="scope.itemProps">
              <q-item-section>
                <q-item-label>{{ scope.opt.accountNumber }} - {{ scope.opt.accountName }}</q-item-label>
                <q-item-label caption>Số dư khả dụng: <span class="text-primary text-weight-bold">{{ formatCurrency(scope.opt.balance) }}</span></q-item-label>
              </q-item-section>
            </q-item>
          </template>
        </q-select>

        <div v-if="insufficientFunds" class="text-negative q-mb-md q-ml-sm text-caption">
          Tài khoản không đủ số dư để thanh toán hóa đơn này.
        </div>

        <q-card flat bordered class="q-mt-md bg-orange-1">
          <q-card-section>
            <div class="text-weight-bold q-mb-sm">Xác nhận thông tin</div>
            <div class="row justify-between q-mb-xs">
              <span class="text-grey-8">Hóa đơn:</span>
              <span class="text-weight-medium">{{ bill.providerName }} - {{ bill.billNumber }}</span>
            </div>
            <div class="row justify-between">
              <span class="text-grey-8">Số tiền thanh toán:</span>
              <span class="text-weight-bold text-negative">{{ formatCurrency(bill.amount) }}</span>
            </div>
          </q-card-section>
        </q-card>

        <AppAlert v-if="paymentError" :message="paymentError" type="error" class="q-mt-md" />

        <q-stepper-navigation class="row justify-between q-mt-lg">
          <q-btn flat @click="step = 1" color="primary" label="Quay lại" :disable="isPaying" />
          <q-btn 
            @click="submitPayment" 
            color="primary" 
            label="Xác nhận thanh toán" 
            :loading="isPaying"
            :disable="!selectedAccountId || insufficientFunds"
          />
        </q-stepper-navigation>
      </q-step>

      <!-- Step 3: Kết quả -->
      <q-step
        :name="3"
        title="Kết quả"
        icon="check_circle"
      >
        <div class="text-center q-pa-md">
          <q-icon name="check_circle" color="positive" size="5em" />
          <div class="text-h5 q-mt-md text-weight-bold text-positive">Thanh toán thành công!</div>
        </div>

        <q-card flat bordered class="q-mt-md" v-if="receipt">
          <q-card-section>
            <q-list separator>
              <q-item>
                <q-item-section>
                  <q-item-label caption>Mã giao dịch</q-item-label>
                  <q-item-label class="text-weight-bold">
                    {{ receipt.reference }}
                    <q-btn flat dense round icon="content_copy" size="xs" color="grey" @click="copyToClipboard(receipt.reference)" />
                  </q-item-label>
                </q-item-section>
              </q-item>
              <q-item>
                <q-item-section>
                  <q-item-label caption>Hóa đơn</q-item-label>
                  <q-item-label>{{ receipt.providerName }} ({{ receipt.billNumber }})</q-item-label>
                </q-item-section>
              </q-item>
              <q-item>
                <q-item-section>
                  <q-item-label caption>Số tiền</q-item-label>
                  <q-item-label class="text-weight-bold text-negative">{{ formatCurrency(receipt.amount) }}</q-item-label>
                </q-item-section>
              </q-item>
              <q-item>
                <q-item-section>
                  <q-item-label caption>Thời gian</q-item-label>
                  <q-item-label>{{ formatDateTime(receipt.paidAtUtc) }}</q-item-label>
                </q-item-section>
              </q-item>
            </q-list>
          </q-card-section>
        </q-card>

        <q-stepper-navigation class="row justify-center q-mt-lg q-gutter-sm">
          <q-btn to="/dashboard" color="primary" flat label="Về trang chủ" />
          <q-btn to="/payments" color="primary" outline label="Lịch sử thanh toán" />
          <q-btn to="/bills" color="primary" label="Hóa đơn khác" />
        </q-stepper-navigation>
      </q-step>
    </q-stepper>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useQuasar } from 'quasar'
import AppAlert from '~/components/common/AppAlert.vue'
import { formatCurrency } from '~/utils/currency'
import { formatDate, formatDateTime } from '~/utils/date'
import { useBillService } from '~/services/billService'
import { useAccountService } from '~/services/accountService'
import { usePaymentService } from '~/services/paymentService'
import { useDashboard } from '~/composables/useDashboard'
import type { BillDetailDto } from '~/types/bill'
import type { AccountSummary } from '~/types/account'
import type { PaymentReceiptDto } from '~/types/payment'

definePageMeta({
  layout: 'default',
  middleware: ['customer']
})

const route = useRoute()
const router = useRouter()
const $q = useQuasar()
const billService = useBillService()
const accountService = useAccountService()
const paymentService = usePaymentService()
const { fetchDashboardData } = useDashboard() // For refresh after payment

const step = ref(1)
const bill = ref<BillDetailDto | null>(null)
const isLoading = ref(true)
const error = ref('')

// Payment state
const accounts = ref<AccountSummary[]>([])
const selectedAccountId = ref('')
const isPaying = ref(false)
const paymentError = ref('')
const receipt = ref<PaymentReceiptDto | null>(null)
let idempotencyKey = ''

onMounted(async () => {
  const id = route.params.id as string
  if (!id) {
    error.value = 'Mã hóa đơn không hợp lệ'
    isLoading.value = false
    return
  }

  try {
    bill.value = await billService.getBill(id)
  } catch (err: any) {
    if (err.response?.status === 404) {
      error.value = 'Không tìm thấy hóa đơn.'
    } else {
      error.value = 'Đã có lỗi xảy ra khi tải dữ liệu hóa đơn.'
    }
  } finally {
    isLoading.value = false
  }
})

const canPay = computed(() => {
  return bill.value && (bill.value.status === 'UNPAID' || bill.value.status === 'OVERDUE')
})

const insufficientFunds = computed(() => {
  if (!selectedAccountId.value || !bill.value) return false
  const acc = accounts.value.find(a => a.id === selectedAccountId.value)
  return acc ? acc.balance < bill.value.amount : false
})

const goToStep2 = async () => {
  try {
    $q.loading.show()
    // Load accounts if not loaded
    if (accounts.value.length === 0) {
      const allAccounts = await accountService.getAccounts()
      accounts.value = allAccounts.filter(a => a.status === 'Active')
      if (accounts.value.length > 0) {
        selectedAccountId.value = accounts.value[0].id
      }
    }
    
    // Generate new Idempotency Key for this payment attempt
    idempotencyKey = crypto.randomUUID()
    paymentError.value = ''
    step.value = 2
  } catch (err) {
    $q.notify({ type: 'negative', message: 'Không thể tải danh sách tài khoản' })
  } finally {
    $q.loading.hide()
  }
}

const submitPayment = async () => {
  if (!bill.value || !selectedAccountId.value || !idempotencyKey) return

  isPaying.value = true
  paymentError.value = ''

  try {
    const response = await paymentService.payBill({
      billId: bill.value.id,
      accountId: selectedAccountId.value
    }, idempotencyKey)

    receipt.value = response
    step.value = 3
    
    // Refetch data globally to update balances and dashboard
    fetchDashboardData()

  } catch (err: any) {
    // If it's a network error (no response), it might be ambiguous
    if (!err.response) {
      paymentError.value = 'Không thể xác định trạng thái thanh toán do lỗi mạng. Vui lòng kiểm tra lịch sử thanh toán trước khi thử lại.'
    } else {
      const status = err.response.status
      if (status === 400) {
        paymentError.value = err.response._data?.title || 'Yêu cầu không hợp lệ. Số dư có thể không đủ hoặc hóa đơn đã thanh toán.'
      } else if (status === 409) {
        paymentError.value = 'Xung đột giao dịch. Vui lòng thử lại sau.'
      } else {
        paymentError.value = 'Đã có lỗi xảy ra khi thanh toán.'
      }
    }
  } finally {
    isPaying.value = false
  }
}

const goBack = () => {
  router.push('/bills')
}

const copyToClipboard = async (text: string) => {
  try {
    await navigator.clipboard.writeText(text)
    $q.notify({ message: 'Đã sao chép', color: 'positive', position: 'bottom' })
  } catch (err) {}
}

const getStatusColor = (status: string) => {
  switch (status) {
    case 'UNPAID': return 'warning'
    case 'PAID': return 'positive'
    case 'OVERDUE': return 'negative'
    case 'CANCELLED': return 'grey'
    default: return 'primary'
  }
}

const getStatusLabel = (status: string) => {
  switch (status) {
    case 'UNPAID': return 'Chưa thanh toán'
    case 'PAID': return 'Đã thanh toán'
    case 'OVERDUE': return 'Quá hạn'
    case 'CANCELLED': return 'Đã hủy'
    default: return status
  }
}
</script>

<style scoped>
.max-width-800 {
  max-width: 800px;
}
</style>
