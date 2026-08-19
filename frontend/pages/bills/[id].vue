<template>
  <div class="bank-page-narrow">
    <div class="bank-page-header">
      <div class="row items-center">
        <q-btn flat round dense icon="arrow_back" class="bank-soft-btn q-mr-sm" @click="goBack" />
        <div>
          <div class="bank-page-kicker">Dịch vụ tiện ích</div>
          <div class="bank-page-title">Chi tiết hóa đơn</div>
          <div class="bank-page-subtitle">Kiểm tra hóa đơn và chọn tài khoản thanh toán an toàn.</div>
        </div>
      </div>
    </div>

    <div v-if="isLoading" class="text-center q-pa-xl">
      <q-spinner color="primary" size="3em" />
    </div>

    <AppAlert v-else-if="error" :message="error" type="error" />

    <q-card v-else-if="bill" flat class="bank-card bill-payment-card">
      <q-card-section>
        <div class="bill-flow-header">
          <div
            v-for="item in flowSteps"
            :key="item.value"
            class="bill-flow-step"
            :class="{ 'bill-flow-step--active': step === item.value, 'bill-flow-step--done': step > item.value }"
          >
            <q-icon :name="item.icon" size="20px" />
            <span>{{ item.label }}</span>
          </div>
        </div>

        <div v-if="step === 1">
          <q-list separator class="bank-kv-list">
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
                  <q-chip :color="getStatusColor(bill.status)" text-color="white" class="bank-chip">
                    {{ getStatusLabel(bill.status) }}
                  </q-chip>
                </q-item-label>
              </q-item-section>
            </q-item>
          </q-list>

          <q-banner v-if="isPaidBill" rounded class="bg-positive-1 text-positive q-mt-md">
            <template v-slot:avatar>
              <q-icon name="check_circle" color="positive" />
            </template>
            Hóa đơn này đã được thanh toán.
            <div v-if="bill.paidAtUtc" class="text-caption text-grey-8 q-mt-xs">
              Thời gian thanh toán: {{ formatDateTime(bill.paidAtUtc) }}
            </div>
          </q-banner>

          <q-banner v-else-if="isCancelledBill" rounded class="bg-grey-2 text-grey-8 q-mt-md">
            <template v-slot:avatar>
              <q-icon name="block" color="grey-7" />
            </template>
            Hóa đơn này đã bị hủy và không thể thanh toán.
          </q-banner>

          <div class="row justify-end q-mt-md">
            <q-btn
              v-if="canPay"
              unelevated
              color="primary"
              label="Thanh toán ngay"
              class="bank-action-btn"
              :loading="isPreparingPayment"
              @click.stop.prevent="preparePayment"
            />
            <q-btn v-else-if="isPaidBill && bill.paymentId" :to="`/payments/${bill.paymentId}`" color="positive" label="Xem biên lai" class="bank-action-btn" />
            <q-btn v-else-if="isPaidBill" to="/payments" color="positive" label="Lịch sử thanh toán" class="bank-action-btn" />
            <q-btn v-else-if="isCancelledBill" color="grey" label="Đã hủy" disable />
          </div>
        </div>

        <div v-else-if="step === 2">
          <div class="q-mb-md text-subtitle1 text-weight-bold">Chọn tài khoản thanh toán</div>
          <q-banner v-if="accounts.length === 0" rounded class="bg-orange-1 text-orange-10 q-mb-md">
            <template v-slot:avatar>
              <q-icon name="warning" color="orange-8" />
            </template>
            Bạn chưa có tài khoản VND đang hoạt động để thanh toán hóa đơn này.
          </q-banner>

          <q-select
            popup-content-class="text-dark bg-white shadow-2"
            options-selected-class="text-primary text-weight-bold"
            v-model="selectedAccountId"
            :options="accounts"
            option-value="id"
            :option-label="opt => `${opt.accountNumber} - ${opt.accountName} (${formatCurrency(opt.balance)})`"
            emit-value
            map-options
            outlined
            label="Tài khoản nguồn"
            :disable="accounts.length === 0"
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

          <q-card flat class="q-mt-md confirm-card">
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
              <div v-if="selectedAccount" class="row justify-between q-mt-xs">
                <span class="text-grey-8">Số dư sau thanh toán:</span>
                <span class="text-weight-bold">{{ formatCurrency(selectedAccount.balance - bill.amount, selectedAccount.currency) }}</span>
              </div>
            </q-card-section>
          </q-card>

          <AppAlert v-if="paymentError" :message="paymentError" type="error" class="q-mt-md" />

          <div class="row justify-between q-mt-lg">
            <q-btn flat @click="step = 1" color="primary" label="Quay lại" :disable="isPaying" />
            <q-btn
              unelevated
              color="primary"
              label="Xác nhận thanh toán"
              class="bank-action-btn"
              :loading="isPaying"
              :disable="!selectedAccountId || insufficientFunds || accounts.length === 0"
              @click.stop.prevent="submitPayment"
            />
          </div>
        </div>

        <div v-else-if="step === 3">
          <div class="text-center q-pa-md">
            <q-icon name="check_circle" color="positive" size="5em" />
            <div class="text-h5 q-mt-md text-weight-bold text-positive">Thanh toán thành công!</div>
          </div>

          <q-card flat class="bank-card q-mt-md" v-if="receipt">
            <q-card-section>
              <q-list separator class="bank-kv-list">
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

          <div class="row justify-center q-mt-lg q-gutter-sm">
            <q-btn to="/dashboard" color="primary" flat label="Về trang chủ" />
            <q-btn to="/payments" color="primary" outline label="Lịch sử thanh toán" />
            <q-btn to="/bills" color="primary" label="Hóa đơn khác" />
          </div>
        </div>
      </q-card-section>
    </q-card>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useQuasar } from 'quasar'
import AppAlert from '~/components/common/AppAlert.vue'
import { formatCurrency } from '~/utils/currency'
import { formatDate, formatDateTime } from '~/utils/date'
import { getBillStatusColor as getStatusColor, getBillStatusLabel as getStatusLabel } from '~/utils/status'
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
const isPreparingPayment = ref(false)
const isPaying = ref(false)
const paymentError = ref('')
const receipt = ref<PaymentReceiptDto | null>(null)
let idempotencyKey = ''

const flowSteps = [
  { value: 1, label: 'Thông tin hóa đơn', icon: 'receipt' },
  { value: 2, label: 'Thanh toán', icon: 'payment' },
  { value: 3, label: 'Kết quả', icon: 'check_circle' }
]

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

const normalizeStatus = (status?: string | null) => (status || '').toUpperCase()

const billStatus = computed(() => normalizeStatus(bill.value?.status))

const canPay = computed(() => {
  return bill.value && (billStatus.value === 'UNPAID' || billStatus.value === 'OVERDUE')
})

const isPaidBill = computed(() => billStatus.value === 'PAID')

const isCancelledBill = computed(() => billStatus.value === 'CANCELLED')

const isActiveAccount = (account: AccountSummary) => {
  return normalizeStatus(account.status) === 'ACTIVE' && account.currency === 'VND'
}

const selectedAccount = computed(() => {
  return accounts.value.find(a => a.id === selectedAccountId.value) || null
})

const insufficientFunds = computed(() => {
  if (!selectedAccountId.value || !bill.value) return false
  return selectedAccount.value ? selectedAccount.value.balance < bill.value.amount : false
})

const createIdempotencyKey = () => {
  if (process.client && globalThis.crypto?.randomUUID) {
    return globalThis.crypto.randomUUID()
  }

  const randomPart = process.client && globalThis.crypto?.getRandomValues
    ? Array.from(globalThis.crypto.getRandomValues(new Uint32Array(4))).map(value => value.toString(16)).join('')
    : Math.random().toString(16).slice(2)

  return `pay-${Date.now()}-${randomPart}`
}

const preparePayment = async () => {
  if (!bill.value || !canPay.value || isPreparingPayment.value) return

  isPreparingPayment.value = true
  paymentError.value = ''
  idempotencyKey = createIdempotencyKey()

  try {
    const allAccounts = await accountService.getAccounts()
    accounts.value = allAccounts.filter(isActiveAccount)

    if (accounts.value.length === 0) {
      selectedAccountId.value = ''
      paymentError.value = 'Không tìm thấy tài khoản VND đang hoạt động để thanh toán.'
      step.value = 2
      $q.notify({ type: 'warning', message: paymentError.value })
    } else {
      const payableAccount = accounts.value.find(a => bill.value && a.balance >= bill.value.amount)
      selectedAccountId.value = payableAccount?.id || accounts.value[0].id
      paymentError.value = ''
      step.value = 2
      $q.notify({ type: 'positive', message: 'Đã tải tài khoản thanh toán. Vui lòng kiểm tra và xác nhận.' })
    }
  } catch (err: any) {
    paymentError.value = err?.data?.title || err?.response?._data?.title || 'Không thể tải danh sách tài khoản. Vui lòng thử lại.'
    $q.notify({ type: 'negative', message: paymentError.value })
    step.value = 1 // Prevent jumping to step 2 if API fails
  } finally {
    isPreparingPayment.value = false
  }
}

const submitPayment = async () => {
  if (!bill.value || !selectedAccountId.value) return

  if (!idempotencyKey) {
    idempotencyKey = createIdempotencyKey()
  }

  isPaying.value = true
  paymentError.value = ''

  try {
    const response = await paymentService.payBill({
      billId: bill.value.id,
      accountId: selectedAccountId.value
    }, idempotencyKey)

    receipt.value = response
    bill.value = await billService.getBill(bill.value.id)
    step.value = 3
    $q.notify({ type: 'positive', message: 'Thanh toán hóa đơn thành công.' })
    
    // Refetch data globally to update balances and dashboard
    fetchDashboardData()

  } catch (err: any) {
    const status = err.response?.status || err.statusCode || err.status
    const title = err.response?._data?.title || err.data?.title
    const detail = err.response?._data?.detail || err.data?.detail || ''

    if (!status) {
      paymentError.value = 'Không thể xác định trạng thái thanh toán do lỗi mạng. Vui lòng kiểm tra lịch sử thanh toán trước khi thử lại.'
    } else if (status === 400) {
      if (detail.includes('Insufficient funds')) {
        paymentError.value = 'Số dư tài khoản không đủ để thanh toán hóa đơn này.'
      } else if (detail.includes('already been paid')) {
        paymentError.value = 'Hóa đơn này đã được thanh toán trước đó.'
      } else if (detail.includes('cannot be used for payments')) {
        paymentError.value = 'Tài khoản đã chọn hiện không thể dùng để thanh toán.'
      } else {
        paymentError.value = title || 'Yêu cầu không hợp lệ. Vui lòng kiểm tra lại thông tin thanh toán.'
      }
    } else if (status === 409) {
      paymentError.value = 'Xung đột giao dịch. Vui lòng làm mới hóa đơn và thử lại.'
    } else if (status === 429) {
      paymentError.value = 'Bạn đang thao tác quá nhanh. Vui lòng thử lại sau.'
    } else {
      paymentError.value = title || 'Đã có lỗi xảy ra khi thanh toán.'
    }
    $q.notify({ type: 'negative', message: paymentError.value })
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

</script>

<style scoped>
.bill-payment-card {
  overflow: hidden;
}

.bill-flow-header {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 10px;
  margin-bottom: 24px;
}

.bill-flow-step {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  min-height: 44px;
  border: 1px solid #d0d5dd;
  border-radius: 8px;
  color: #667085;
  font-size: 13px;
  font-weight: 800;
  background: #ffffff;
}

.bill-flow-step--active {
  border-color: #1f7ae0;
  color: #1f7ae0;
  background: #eff6ff;
}

.bill-flow-step--done {
  border-color: #22c55e;
  color: #15803d;
  background: #f0fdf4;
}

.confirm-card {
  background: #fff7ed;
  border: 1px solid #fed7aa;
  border-radius: 8px;
}

@media (max-width: 720px) {
  .bill-flow-header {
    grid-template-columns: 1fr;
  }
}
</style>
