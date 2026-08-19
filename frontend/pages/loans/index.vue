<template>
  <div class="bank-page">
    <div class="bank-page-header">
      <div>
        <div class="bank-page-kicker">Vay tiêu dùng</div>
        <div class="bank-page-title">Đăng ký vay online</div>
        <div class="bank-page-subtitle">Nộp hồ sơ, theo dõi trạng thái phê duyệt và nhận giải ngân vào tài khoản thanh toán.</div>
      </div>
      <q-btn unelevated color="primary" icon="refresh" label="Cập nhật" class="bank-action-btn" :loading="isLoading" @click="fetchData" />
    </div>

    <AppAlert v-if="errorMessage" type="error" :message="errorMessage" class="q-mb-md" @dismiss="errorMessage = ''" />
    <AppAlert v-if="successMessage" type="success" :message="successMessage" class="q-mb-md" @dismiss="successMessage = ''" />

    <div class="row q-col-gutter-lg">
      <div class="col-12 col-lg-5">
        <q-card flat class="bank-card loan-form-card">
          <q-card-section class="q-pa-lg">
            <div class="loan-section-title">
              <q-icon name="request_quote" color="primary" size="24px" />
              <span>Hồ sơ mới</span>
            </div>

            <q-form class="q-mt-lg" @submit.prevent="submitApplication">
              <div class="bank-field-label">Tài khoản nhận giải ngân</div>
              <q-select
                v-model="form.disbursementAccountId"
                outlined
                emit-value
                map-options
                :options="accountOptions"
                :loading="isLoading"
                :disable="isSubmitting"
                option-label="label"
                option-value="value"
                label="Chọn tài khoản"
                class="q-mb-md"
                :rules="[v => !!v || 'Vui lòng chọn tài khoản nhận giải ngân']"
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

              <div class="row q-col-gutter-md">
                <div class="col-12 col-sm-6">
                  <div class="bank-field-label">Số tiền đề nghị</div>
                  <q-input
                    v-model.number="form.requestedAmount"
                    outlined
                    type="number"
                    min="1000000"
                    step="1000000"
                    suffix="VND"
                    label="Từ 1.000.000"
                    :disable="isSubmitting"
                    :rules="[v => Number(v) >= 1000000 || 'Tối thiểu 1.000.000 VND']"
                  />
                </div>
                <div class="col-12 col-sm-6">
                  <div class="bank-field-label">Thu nhập hàng tháng</div>
                  <q-input
                    v-model.number="form.monthlyIncome"
                    outlined
                    type="number"
                    min="1000000"
                    step="500000"
                    suffix="VND"
                    label="Thu nhập ròng"
                    :disable="isSubmitting"
                    :rules="[v => Number(v) >= 1000000 || 'Thu nhập phải lớn hơn 1.000.000 VND']"
                  />
                </div>
              </div>

              <div class="bank-field-label q-mt-sm">Kỳ hạn</div>
              <div class="loan-term-grid q-mb-md">
                <button
                  v-for="term in loanTerms"
                  :key="term"
                  type="button"
                  class="loan-term-tile"
                  :class="{ 'loan-term-active': form.termMonths === term }"
                  :disabled="isSubmitting"
                  @click="form.termMonths = term"
                >
                  <strong>{{ term }}</strong>
                  <span>tháng</span>
                </button>
              </div>

              <div class="bank-field-label">Mục đích vay</div>
              <q-input
                v-model="form.purpose"
                outlined
                type="textarea"
                autogrow
                maxlength="500"
                label="Ví dụ: mua sắm thiết bị, học phí, sửa nhà..."
                :disable="isSubmitting"
                :rules="[v => !!v?.trim() || 'Vui lòng nhập mục đích vay']"
              />

              <div class="loan-preview q-mt-lg">
                <div>
                  <span>Lãi suất tham chiếu</span>
                  <strong>12,50%/năm</strong>
                </div>
                <div>
                  <span>Ước tính trả góp</span>
                  <strong>{{ formatCurrency(estimatedInstallment) }}</strong>
                </div>
                <div>
                  <span>Tỷ lệ vay/thu nhập</span>
                  <strong>{{ debtRatio.toFixed(1) }}%</strong>
                </div>
              </div>

              <q-btn
                unelevated
                color="primary"
                icon="send"
                label="Nộp hồ sơ vay"
                type="submit"
                size="lg"
                class="bank-action-btn full-width q-mt-lg"
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
            <q-card flat class="bank-card loan-stat-card">
              <q-card-section>
                <div class="stat-label">Hồ sơ chờ duyệt</div>
                <div class="stat-value">{{ pendingApplications.length }}</div>
              </q-card-section>
            </q-card>
          </div>
          <div class="col-12 col-sm-4">
            <q-card flat class="bank-card loan-stat-card">
              <q-card-section>
                <div class="stat-label">Đã giải ngân</div>
                <div class="stat-value stat-money">{{ formatCurrency(totalDisbursed) }}</div>
              </q-card-section>
            </q-card>
          </div>
          <div class="col-12 col-sm-4">
            <q-card flat class="bank-card loan-stat-card">
              <q-card-section>
                <div class="stat-label">Hạn mức đề nghị</div>
                <div class="stat-value stat-money">{{ formatCurrency(totalRequested) }}</div>
              </q-card-section>
            </q-card>
          </div>
        </div>

        <q-card flat class="bank-card">
          <q-card-section class="q-pa-lg">
            <div class="row items-center justify-between q-mb-md">
              <div class="loan-section-title">
                <q-icon name="fact_check" color="primary" size="24px" />
                <span>Danh sách hồ sơ vay</span>
              </div>
              <q-select
                v-model="statusFilter"
                dense
                outlined
                emit-value
                map-options
                :options="statusOptions"
                style="min-width: 190px"
                @update:model-value="fetchApplications"
              />
            </div>

            <div v-if="isLoading" class="q-gutter-md">
              <q-skeleton v-for="i in 3" :key="i" type="rect" height="118px" />
            </div>

            <div v-else-if="applications.length === 0" class="empty-state">
              <q-icon name="request_quote" size="64px" color="grey-4" />
              <div class="text-h6 text-grey-7 q-mt-md">Chưa có hồ sơ vay</div>
              <div class="text-grey-6">Hồ sơ mới sẽ xuất hiện tại đây sau khi gửi.</div>
            </div>

            <div v-else class="loan-list">
              <div v-for="application in applications" :key="application.id" class="loan-row">
                <div>
                  <div class="loan-number">{{ application.applicationNumber }}</div>
                  <div class="loan-meta">
                    {{ application.termMonths }} tháng · {{ application.annualInterestRate.toFixed(2) }}%/năm · {{ formatDate(application.submittedAtUtc) }}
                  </div>
                  <div v-if="application.reviewNote" class="loan-note">{{ application.reviewNote }}</div>
                </div>
                <div class="loan-amount">
                  <span>Đề nghị</span>
                  <strong>{{ formatCurrency(application.requestedAmount, application.currency) }}</strong>
                  <small v-if="application.approvedAmount">Duyệt {{ formatCurrency(application.approvedAmount, application.currency) }}</small>
                </div>
                <q-chip :color="statusColor(application.status)" class="bank-chip">{{ statusLabel(application.status) }}</q-chip>
              </div>
            </div>
          </q-card-section>
        </q-card>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import AppAlert from '~/components/common/AppAlert.vue'
import { useAccountService } from '~/services/accountService'
import { useLoanService } from '~/services/loanService'
import type { AccountSummary } from '~/types/account'
import type { LoanApplicationDto } from '~/types/loan'
import { formatCurrency } from '~/utils/currency'
import { formatDate } from '~/utils/date'

definePageMeta({
  middleware: ['customer']
})

const accountService = useAccountService()
const loanService = useLoanService()

const accounts = ref<AccountSummary[]>([])
const applications = ref<LoanApplicationDto[]>([])
const isLoading = ref(true)
const isSubmitting = ref(false)
const errorMessage = ref('')
const successMessage = ref('')
const statusFilter = ref('')

const loanTerms = [6, 12, 24, 36, 60]
const statusOptions = [
  { label: 'Tất cả hồ sơ', value: '' },
  { label: 'Chờ duyệt', value: 'Pending' },
  { label: 'Đã giải ngân', value: 'Disbursed' },
  { label: 'Từ chối', value: 'Rejected' }
]

const form = reactive({
  disbursementAccountId: '',
  requestedAmount: 20000000,
  termMonths: 24,
  monthlyIncome: 15000000,
  purpose: ''
})

const accountOptions = computed(() =>
  accounts.value
    .filter(account => account.status === 'ACTIVE' || account.status === 'Active')
    .map(account => ({
      label: `${account.accountNumber} - ${account.accountName}`,
      value: account.id,
      accountNumber: account.accountNumber,
      accountName: account.accountName,
      balance: account.balance,
      currency: account.currency
    }))
)

const estimatedInstallment = computed(() => {
  const principal = Number(form.requestedAmount || 0)
  const monthlyRate = 0.125 / 12
  const months = Number(form.termMonths || 1)
  if (principal <= 0 || months <= 0) return 0
  return principal * monthlyRate / (1 - Math.pow(1 + monthlyRate, -months))
})

const debtRatio = computed(() => {
  const income = Number(form.monthlyIncome || 0)
  if (income <= 0) return 0
  return estimatedInstallment.value / income * 100
})

const canSubmit = computed(() =>
  !!form.disbursementAccountId
  && Number(form.requestedAmount) >= 1000000
  && Number(form.monthlyIncome) >= 1000000
  && !!form.purpose.trim()
)

const pendingApplications = computed(() => applications.value.filter(item => item.status === 'Pending' || item.status === 'PENDING'))
const totalRequested = computed(() => applications.value.reduce((sum, item) => sum + item.requestedAmount, 0))
const totalDisbursed = computed(() =>
  applications.value.reduce((sum, item) => sum + (item.status === 'Disbursed' || item.status === 'DISBURSED' ? item.approvedAmount || 0 : 0), 0)
)

const fetchData = async () => {
  isLoading.value = true
  errorMessage.value = ''

  try {
    const [accountResult, applicationResult] = await Promise.all([
      accountService.getAccounts(),
      loanService.getApplications(statusFilter.value || undefined)
    ])
    accounts.value = accountResult
    applications.value = applicationResult
    if (!form.disbursementAccountId && accountOptions.value.length > 0) {
      form.disbursementAccountId = accountOptions.value[0].value
    }
  } catch (e: any) {
    errorMessage.value = e?.data?.message || e?.message || 'Không thể tải dữ liệu vay vốn.'
  } finally {
    isLoading.value = false
  }
}

const fetchApplications = async () => {
  errorMessage.value = ''
  try {
    applications.value = await loanService.getApplications(statusFilter.value || undefined)
  } catch (e: any) {
    errorMessage.value = e?.data?.message || e?.message || 'Không thể tải danh sách hồ sơ vay.'
  }
}

const submitApplication = async () => {
  if (!canSubmit.value) return

  isSubmitting.value = true
  errorMessage.value = ''
  successMessage.value = ''

  try {
    const application = await loanService.createApplication({
      disbursementAccountId: form.disbursementAccountId,
      requestedAmount: Number(form.requestedAmount),
      termMonths: Number(form.termMonths),
      monthlyIncome: Number(form.monthlyIncome),
      purpose: form.purpose.trim()
    })
    successMessage.value = `Đã nộp hồ sơ ${application.applicationNumber}.`
    form.purpose = ''
    await fetchData()
  } catch (e: any) {
    errorMessage.value = e?.data?.message || e?.message || 'Không thể nộp hồ sơ vay.'
  } finally {
    isSubmitting.value = false
  }
}

const statusColor = (status: string) => {
  switch (status) {
    case 'Pending':
    case 'PENDING':
      return 'warning'
    case 'Disbursed':
    case 'DISBURSED':
      return 'positive'
    case 'Rejected':
    case 'REJECTED':
      return 'negative'
    default:
      return 'grey'
  }
}

const statusLabel = (status: string) => {
  switch (status) {
    case 'Pending':
    case 'PENDING':
      return 'Chờ duyệt'
    case 'Disbursed':
    case 'DISBURSED':
      return 'Đã giải ngân'
    case 'Rejected':
    case 'REJECTED':
      return 'Từ chối'
    case 'Cancelled':
    case 'CANCELLED':
      return 'Đã hủy'
    default:
      return status
  }
}

onMounted(fetchData)
</script>

<style scoped>
.loan-section-title {
  align-items: center;
  color: #1d2939;
  display: flex;
  font-size: 18px;
  font-weight: 850;
  gap: 10px;
}

.loan-term-grid {
  display: grid;
  gap: 10px;
  grid-template-columns: repeat(5, minmax(0, 1fr));
}

.loan-term-tile {
  background: #ffffff;
  border: 1px solid #d9e2ef;
  border-radius: 8px;
  color: #344054;
  cursor: pointer;
  min-height: 70px;
  padding: 10px;
  text-align: center;
}

.loan-term-tile strong {
  display: block;
  font-size: 20px;
  font-weight: 900;
}

.loan-term-tile span {
  color: #667085;
  font-size: 12px;
  font-weight: 700;
}

.loan-term-active {
  background: #e7f0ff;
  border-color: #1f6fd1;
  color: #1f6fd1;
}

.loan-preview {
  background: #fff8ed;
  border: 1px solid #fed7aa;
  border-radius: 8px;
  display: grid;
  gap: 12px;
  padding: 16px;
}

.loan-preview div,
.loan-row,
.loan-amount {
  display: flex;
  gap: 12px;
}

.loan-preview div {
  justify-content: space-between;
}

.loan-preview span,
.loan-amount span,
.loan-amount small {
  color: #667085;
  font-weight: 700;
}

.loan-preview strong {
  color: #1d2939;
}

.loan-stat-card {
  min-height: 126px;
}

.loan-list {
  display: grid;
  gap: 12px;
}

.loan-row {
  align-items: center;
  border: 1px solid #eaecf0;
  border-radius: 8px;
  justify-content: space-between;
  padding: 16px;
}

.loan-number {
  color: #1d2939;
  font-size: 16px;
  font-weight: 850;
}

.loan-meta,
.loan-note {
  color: #667085;
  font-size: 13px;
  font-weight: 600;
  margin-top: 4px;
}

.loan-amount {
  align-items: flex-end;
  flex-direction: column;
  min-width: 180px;
}

.loan-amount strong {
  color: #1f6fd1;
  font-size: 18px;
  font-weight: 900;
}

.empty-state {
  padding: 56px 16px;
  text-align: center;
}

@media (max-width: 720px) {
  .loan-term-grid {
    grid-template-columns: repeat(3, minmax(0, 1fr));
  }

  .loan-row {
    align-items: flex-start;
    flex-direction: column;
  }

  .loan-amount {
    align-items: flex-start;
  }
}
</style>
