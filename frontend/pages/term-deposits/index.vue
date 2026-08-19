<template>
  <div class="bank-page">
    <div class="bank-page-header">
      <div>
        <div class="bank-page-kicker">Tiền gửi tiết kiệm</div>
        <div class="bank-page-title">Mở sổ tiết kiệm online</div>
        <div class="bank-page-subtitle">Chọn tài khoản nguồn, kỳ hạn và theo dõi lãi dự kiến ngay trong SuperBanking.</div>
      </div>
      <q-btn
        unelevated
        color="primary"
        icon="refresh"
        label="Cập nhật"
        class="bank-action-btn"
        :loading="isLoading"
        @click="loadData"
      />
    </div>

    <AppAlert v-if="errorMessage" type="error" :message="errorMessage" class="q-mb-md" @dismiss="errorMessage = ''" />
    <AppAlert v-if="successMessage" type="success" :message="successMessage" class="q-mb-md" @dismiss="successMessage = ''" />

    <div class="row q-col-gutter-lg">
      <div class="col-12 col-lg-5">
        <q-card flat class="bank-card deposit-form-card">
          <q-card-section class="q-pa-lg">
            <div class="deposit-section-title">
              <q-icon name="add_card" color="primary" size="24px" />
              <span>Tạo khoản tiết kiệm</span>
            </div>

            <q-form class="q-mt-lg" @submit.prevent="openDeposit">
              <div class="bank-field-label">Tài khoản nguồn</div>
              <q-select
                v-model="selectedAccountId"
                outlined
                emit-value
                map-options
                :options="accountOptions"
                :loading="isLoading"
                :disable="isSubmitting"
                option-label="label"
                option-value="value"
                label="Chọn tài khoản trích tiền"
                class="q-mb-md"
                :rules="[val => !!val || 'Vui lòng chọn tài khoản nguồn']"
              >
                <template #option="scope">
                  <q-item v-bind="scope.itemProps">
                    <q-item-section>
                      <q-item-label class="text-weight-bold">{{ scope.opt.accountName }}</q-item-label>
                      <q-item-label caption>{{ scope.opt.accountNumber }}</q-item-label>
                    </q-item-section>
                    <q-item-section side class="text-primary text-weight-bold">
                      {{ formatCurrency(scope.opt.balance, scope.opt.currency) }}
                    </q-item-section>
                  </q-item>
                </template>
              </q-select>

              <div class="bank-field-label">Số tiền gửi</div>
              <q-input
                v-model.number="principalAmount"
                outlined
                type="number"
                min="100000"
                step="100000"
                suffix="VND"
                label="Tối thiểu 100.000 VND"
                :disable="isSubmitting"
                class="q-mb-md"
                :rules="[
                  val => !!val || 'Vui lòng nhập số tiền gửi',
                  val => Number(val) >= 100000 || 'Số tiền gửi tối thiểu là 100.000 VND',
                  val => Number(val) <= selectedAccountBalance || 'Số dư tài khoản không đủ'
                ]"
              />

              <div class="bank-field-label">Kỳ hạn</div>
              <div class="tenor-grid q-mb-lg">
                <button
                  v-for="option in tenorOptions"
                  :key="option.months"
                  type="button"
                  class="tenor-tile"
                  :class="{ 'tenor-tile-active': tenorMonths === option.months }"
                  :disabled="isSubmitting"
                  @click="tenorMonths = option.months"
                >
                  <span>{{ option.label }}</span>
                  <strong>{{ option.rate.toFixed(2) }}%/năm</strong>
                </button>
              </div>

              <div class="deposit-preview">
                <div>
                  <span>Lãi dự kiến</span>
                  <strong>{{ formatCurrency(expectedInterest) }}</strong>
                </div>
                <div>
                  <span>Ngày đáo hạn</span>
                  <strong>{{ estimatedMaturityDate }}</strong>
                </div>
                <div>
                  <span>Số dư sau mở sổ</span>
                  <strong>{{ formatCurrency(remainingBalance) }}</strong>
                </div>
              </div>

              <q-btn
                unelevated
                color="primary"
                icon="savings"
                label="Mở sổ tiết kiệm"
                type="submit"
                class="bank-action-btn full-width q-mt-lg"
                size="lg"
                :loading="isSubmitting"
                :disable="!canSubmit"
              />
            </q-form>
          </q-card-section>
        </q-card>
      </div>

      <div class="col-12 col-lg-7">
        <div class="row q-col-gutter-md q-mb-lg">
          <div class="col-12 col-sm-4">
            <q-card flat class="bank-card stat-card">
              <q-card-section>
                <div class="stat-label">Sổ đang hiệu lực</div>
                <div class="stat-value">{{ activeDeposits.length }}</div>
              </q-card-section>
            </q-card>
          </div>
          <div class="col-12 col-sm-4">
            <q-card flat class="bank-card stat-card">
              <q-card-section>
                <div class="stat-label">Tổng gốc</div>
                <div class="stat-value stat-money">{{ formatCurrency(totalPrincipal) }}</div>
              </q-card-section>
            </q-card>
          </div>
          <div class="col-12 col-sm-4">
            <q-card flat class="bank-card stat-card">
              <q-card-section>
                <div class="stat-label">Lãi dự kiến</div>
                <div class="stat-value stat-money">{{ formatCurrency(totalExpectedInterest) }}</div>
              </q-card-section>
            </q-card>
          </div>
        </div>

        <q-card flat class="bank-card">
          <q-card-section class="q-pa-lg">
            <div class="row items-center justify-between q-mb-md">
              <div class="deposit-section-title">
                <q-icon name="account_balance" color="primary" size="24px" />
                <span>Danh sách sổ tiết kiệm</span>
              </div>
              <q-select
                v-model="statusFilter"
                dense
                outlined
                emit-value
                map-options
                :options="statusOptions"
                style="min-width: 180px"
                @update:model-value="loadDeposits"
              />
            </div>

            <div v-if="isLoading" class="q-gutter-md">
              <q-skeleton v-for="i in 3" :key="i" type="rect" height="116px" />
            </div>

            <div v-else-if="termDeposits.length === 0" class="empty-state">
              <q-icon name="savings" size="64px" color="grey-4" />
              <div class="text-h6 text-grey-7 q-mt-md">Chưa có sổ tiết kiệm</div>
              <div class="text-grey-6">Bạn có thể mở sổ đầu tiên bằng form bên trái.</div>
            </div>

            <div v-else class="deposit-list">
              <div v-for="deposit in termDeposits" :key="deposit.id" class="deposit-row">
                <div>
                  <div class="deposit-number">{{ deposit.depositNumber }}</div>
                  <div class="deposit-meta">
                    {{ deposit.tenorMonths }} tháng · {{ deposit.annualInterestRate.toFixed(2) }}%/năm · Đáo hạn {{ formatDate(deposit.maturityDateUtc) }}
                  </div>
                </div>
                <div class="deposit-amount">
                  <div>{{ formatCurrency(deposit.principalAmount, deposit.currency) }}</div>
                  <span>+ {{ formatCurrency(deposit.expectedInterestAmount, deposit.currency) }}</span>
                </div>
                <q-badge :color="getStatusColor(deposit.status)" class="bank-chip">
                  {{ getStatusLabel(deposit.status) }}
                </q-badge>
              </div>
            </div>
          </q-card-section>
        </q-card>
      </div>
    </div>

    <q-dialog v-model="receiptDialog">
      <q-card class="receipt-card">
        <q-card-section class="text-center q-pa-lg">
          <q-avatar size="64px" color="positive" text-color="white" icon="check" />
          <div class="text-h5 text-weight-bold q-mt-md">Mở sổ thành công</div>
          <div class="text-grey-7 q-mt-sm">Sổ tiết kiệm đã được ghi nhận trên hệ thống.</div>
        </q-card-section>
        <q-separator />
        <q-card-section v-if="lastReceipt" class="q-pa-lg">
          <div class="receipt-line">
            <span>Mã sổ</span>
            <strong>{{ lastReceipt.depositNumber }}</strong>
          </div>
          <div class="receipt-line">
            <span>Tài khoản nguồn</span>
            <strong>{{ lastReceipt.sourceAccountNumber }}</strong>
          </div>
          <div class="receipt-line">
            <span>Số tiền gửi</span>
            <strong>{{ formatCurrency(lastReceipt.principalAmount, lastReceipt.currency) }}</strong>
          </div>
          <div class="receipt-line">
            <span>Lãi dự kiến</span>
            <strong>{{ formatCurrency(lastReceipt.expectedInterestAmount, lastReceipt.currency) }}</strong>
          </div>
          <div class="receipt-line">
            <span>Ngày đáo hạn</span>
            <strong>{{ formatDate(lastReceipt.maturityDateUtc) }}</strong>
          </div>
        </q-card-section>
        <q-card-actions align="right" class="q-pa-md">
          <q-btn flat color="primary" label="Đóng" v-close-popup />
        </q-card-actions>
      </q-card>
    </q-dialog>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import AppAlert from '~/components/common/AppAlert.vue'
import { useAccountService } from '~/services/accountService'
import { useTermDepositService } from '~/services/termDepositService'
import type { AccountSummary } from '~/types/account'
import type { TermDepositDto, TermDepositReceiptDto } from '~/types/termDeposit'
import { formatCurrency } from '~/utils/currency'
import { formatDate } from '~/utils/date'

definePageMeta({
  middleware: ['customer']
})

const accountService = useAccountService()
const termDepositService = useTermDepositService()

const accounts = ref<AccountSummary[]>([])
const termDeposits = ref<TermDepositDto[]>([])
const selectedAccountId = ref('')
const principalAmount = ref(1000000)
const tenorMonths = ref(3)
const statusFilter = ref('')
const isLoading = ref(true)
const isSubmitting = ref(false)
const errorMessage = ref('')
const successMessage = ref('')
const receiptDialog = ref(false)
const lastReceipt = ref<TermDepositReceiptDto | null>(null)

const tenorOptions = [
  { months: 1, label: '1 tháng', rate: 4.2 },
  { months: 3, label: '3 tháng', rate: 4.8 },
  { months: 12, label: '12 tháng', rate: 5.8 }
]

const statusOptions = [
  { label: 'Tất cả', value: '' },
  { label: 'Đang hiệu lực', value: 'Active' },
  { label: 'Đã đáo hạn', value: 'Matured' },
  { label: 'Đã đóng', value: 'Closed' }
]

const accountOptions = computed(() =>
  accounts.value
    .filter(account => account.status === 'ACTIVE' || account.status === 'Active')
    .map(account => ({
      label: `${account.accountNumber} - ${account.accountName}`,
      value: account.id,
      accountName: account.accountName,
      accountNumber: account.accountNumber,
      balance: account.balance,
      currency: account.currency
    }))
)

const selectedAccount = computed(() =>
  accounts.value.find(account => account.id === selectedAccountId.value)
)

const selectedAccountBalance = computed(() => selectedAccount.value?.balance ?? 0)

const selectedTenor = computed(() =>
  tenorOptions.find(option => option.months === tenorMonths.value) || tenorOptions[1]
)

const expectedInterest = computed(() =>
  Math.round((Number(principalAmount.value || 0) * selectedTenor.value.rate / 100 * tenorMonths.value / 12) * 100) / 100
)

const remainingBalance = computed(() =>
  Math.max(selectedAccountBalance.value - Number(principalAmount.value || 0), 0)
)

const estimatedMaturityDate = computed(() => {
  const date = new Date()
  date.setMonth(date.getMonth() + tenorMonths.value)
  return formatDate(date)
})

const activeDeposits = computed(() =>
  termDeposits.value.filter(deposit => deposit.status === 'Active' || deposit.status === 'ACTIVE')
)

const totalPrincipal = computed(() =>
  activeDeposits.value.reduce((sum, deposit) => sum + deposit.principalAmount, 0)
)

const totalExpectedInterest = computed(() =>
  activeDeposits.value.reduce((sum, deposit) => sum + deposit.expectedInterestAmount, 0)
)

const canSubmit = computed(() =>
  !!selectedAccountId.value &&
  Number(principalAmount.value) >= 100000 &&
  Number(principalAmount.value) <= selectedAccountBalance.value
)

const loadData = async () => {
  isLoading.value = true
  errorMessage.value = ''

  try {
    const [accountData, depositData] = await Promise.all([
      accountService.getAccounts(),
      termDepositService.getTermDeposits(statusFilter.value || undefined)
    ])

    accounts.value = accountData
    termDeposits.value = depositData

    if (!selectedAccountId.value && accountOptions.value.length > 0) {
      selectedAccountId.value = accountOptions.value[0].value
    }
  } catch (e: any) {
    errorMessage.value = e?.data?.message || e?.message || 'Không thể tải dữ liệu tiết kiệm. Vui lòng thử lại.'
  } finally {
    isLoading.value = false
  }
}

const loadDeposits = async () => {
  errorMessage.value = ''

  try {
    termDeposits.value = await termDepositService.getTermDeposits(statusFilter.value || undefined)
  } catch (e: any) {
    errorMessage.value = e?.data?.message || e?.message || 'Không thể tải danh sách sổ tiết kiệm.'
  }
}

const openDeposit = async () => {
  if (!canSubmit.value) return

  isSubmitting.value = true
  errorMessage.value = ''
  successMessage.value = ''

  try {
    const receipt = await termDepositService.openTermDeposit({
      sourceAccountId: selectedAccountId.value,
      principalAmount: Number(principalAmount.value),
      tenorMonths: tenorMonths.value
    })

    lastReceipt.value = receipt
    receiptDialog.value = true
    successMessage.value = `Đã mở sổ tiết kiệm ${receipt.depositNumber}.`
    await loadData()
  } catch (e: any) {
    errorMessage.value = e?.data?.message || e?.message || 'Không thể mở sổ tiết kiệm. Vui lòng kiểm tra lại thông tin.'
  } finally {
    isSubmitting.value = false
  }
}

const getStatusColor = (status: string) => {
  switch (status) {
    case 'Active':
    case 'ACTIVE':
      return 'positive'
    case 'Matured':
    case 'MATURED':
      return 'primary'
    case 'Closed':
    case 'CLOSED':
      return 'grey'
    default:
      return 'warning'
  }
}

const getStatusLabel = (status: string) => {
  switch (status) {
    case 'Active':
    case 'ACTIVE':
      return 'Đang hiệu lực'
    case 'Matured':
    case 'MATURED':
      return 'Đã đáo hạn'
    case 'Closed':
    case 'CLOSED':
      return 'Đã đóng'
    case 'Cancelled':
    case 'CANCELLED':
      return 'Đã hủy'
    default:
      return status
  }
}

onMounted(loadData)
</script>

<style scoped>
.deposit-form-card {
  border-top: 4px solid #1f6fd1;
}

.deposit-section-title {
  display: flex;
  align-items: center;
  gap: 10px;
  color: #1d2939;
  font-size: 20px;
  font-weight: 850;
}

.tenor-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 10px;
}

.tenor-tile {
  min-height: 82px;
  border: 1px solid #d0d5dd;
  border-radius: 8px;
  background: #ffffff;
  color: #344054;
  cursor: pointer;
  display: flex;
  flex-direction: column;
  justify-content: center;
  gap: 6px;
  padding: 12px;
  transition: border-color 0.16s ease, background-color 0.16s ease, box-shadow 0.16s ease;
}

.tenor-tile span {
  font-size: 14px;
  font-weight: 800;
}

.tenor-tile strong {
  color: #1f6fd1;
  font-size: 18px;
}

.tenor-tile-active {
  border-color: #1f6fd1;
  background: #eff6ff;
  box-shadow: 0 8px 20px rgba(31, 111, 209, 0.12);
}

.deposit-preview {
  border: 1px solid #fedf89;
  border-radius: 8px;
  background: #fffbeb;
  padding: 16px;
}

.deposit-preview > div,
.receipt-line {
  display: flex;
  justify-content: space-between;
  gap: 16px;
  padding: 8px 0;
}

.deposit-preview span,
.receipt-line span {
  color: #667085;
  font-weight: 700;
}

.deposit-preview strong,
.receipt-line strong {
  color: #1d2939;
  text-align: right;
}

.stat-card {
  min-height: 120px;
}

.stat-label {
  color: #667085;
  font-size: 12px;
  font-weight: 850;
  text-transform: uppercase;
}

.stat-value {
  color: #1d2939;
  font-size: 34px;
  font-weight: 900;
  margin-top: 10px;
}

.stat-money {
  color: #1f6fd1;
  font-size: 22px;
}

.empty-state {
  text-align: center;
  padding: 48px 16px;
}

.deposit-list {
  display: grid;
  gap: 12px;
}

.deposit-row {
  border: 1px solid #eaecf0;
  border-radius: 8px;
  display: grid;
  grid-template-columns: 1fr auto auto;
  align-items: center;
  gap: 18px;
  padding: 16px;
}

.deposit-number {
  color: #1d2939;
  font-size: 17px;
  font-weight: 850;
}

.deposit-meta {
  color: #667085;
  margin-top: 4px;
}

.deposit-amount {
  color: #1d2939;
  font-weight: 850;
  text-align: right;
}

.deposit-amount span {
  color: #12b76a;
  display: block;
  font-size: 13px;
  margin-top: 4px;
}

.receipt-card {
  width: min(520px, calc(100vw - 32px));
}

@media (max-width: 720px) {
  .tenor-grid,
  .deposit-row {
    grid-template-columns: 1fr;
  }

  .deposit-amount,
  .deposit-preview strong,
  .receipt-line strong {
    text-align: left;
  }

  .deposit-preview > div,
  .receipt-line {
    flex-direction: column;
    gap: 4px;
  }
}
</style>
