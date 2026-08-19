<template>
  <div class="bank-page">
    <div class="bank-page-header">
      <div>
        <div class="bank-page-kicker">Napas 24/7</div>
        <div class="bank-page-title">Chuyển tiền liên ngân hàng</div>
        <div class="bank-page-subtitle">Giả lập chuyển nhanh đến tài khoản hoặc thẻ ngân hàng ngoài.</div>
      </div>
      <q-btn unelevated color="primary" icon="refresh" label="Cập nhật" class="bank-action-btn" :loading="isLoading" @click="loadData" />
    </div>

    <AppAlert v-if="errorMessage" type="error" :message="errorMessage" class="q-mb-md" @dismiss="errorMessage = ''" />
    <AppAlert v-if="successMessage" type="success" :message="successMessage" class="q-mb-md" @dismiss="successMessage = ''" />

    <div class="row q-col-gutter-lg">
      <div class="col-12 col-lg-7">
        <q-card flat class="bank-card napas-form-card">
          <q-card-section class="q-pa-lg">
            <div class="napas-section-title">
              <q-icon name="public" color="primary" size="24px" />
              <span>Thông tin chuyển tiền</span>
            </div>

            <q-form class="q-mt-lg" @submit.prevent="submitTransfer">
              <div class="bank-field-label">Tài khoản nguồn</div>
              <q-select
                v-model="form.sourceAccountId"
                outlined
                emit-value
                map-options
                :options="accountOptions"
                option-label="label"
                option-value="value"
                label="Chọn tài khoản trích tiền"
                :loading="isLoading"
                :disable="isSubmitting"
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

              <div class="row q-col-gutter-md">
                <div class="col-12 col-md-7">
                  <div class="bank-field-label">Ngân hàng nhận</div>
                  <q-select
                    v-model="form.bankCode"
                    outlined
                    emit-value
                    map-options
                    :options="bankOptions"
                    option-label="label"
                    option-value="value"
                    label="Chọn ngân hàng ngoài"
                    :disable="isSubmitting"
                    class="q-mb-md"
                    :rules="[val => !!val || 'Vui lòng chọn ngân hàng nhận']"
                    @update:model-value="clearLookup"
                  />
                </div>
                <div class="col-12 col-md-5">
                  <div class="bank-field-label">Loại định danh</div>
                  <q-btn-toggle
                    v-model="form.destinationType"
                    spread
                    unelevated
                    toggle-color="primary"
                    color="grey-3"
                    text-color="dark"
                    :options="[
                      { label: 'Tài khoản', value: 'ACCOUNT' },
                      { label: 'Thẻ', value: 'CARD' }
                    ]"
                    class="napas-toggle q-mb-md"
                    @update:model-value="clearLookup"
                  />
                </div>
              </div>

              <div class="bank-field-label">Số tài khoản / số thẻ nhận</div>
              <div class="row q-col-gutter-sm q-mb-md">
                <div class="col">
                  <q-input
                    v-model="form.destinationNumber"
                    outlined
                    label="Nhập 6-19 chữ số"
                    mask="###################"
                    :disable="isSubmitting"
                    :rules="[
                      val => !!val || 'Vui lòng nhập số nhận',
                      val => normalizeDigits(val).length >= 6 || 'Số nhận phải có ít nhất 6 chữ số'
                    ]"
                    @update:model-value="clearLookup"
                  />
                </div>
                <div class="col-auto">
                  <q-btn
                    outline
                    color="primary"
                    icon="search"
                    label="Tra cứu"
                    class="lookup-btn"
                    :loading="isLookingUp"
                    :disable="!canLookup || isSubmitting"
                    @click="lookupDestination"
                  />
                </div>
              </div>

              <div v-if="lookupResult" class="lookup-card lookup-ok q-mb-md">
                <q-icon name="verified" color="positive" size="28px" />
                <div>
                  <div class="lookup-name">{{ lookupResult.destinationName }}</div>
                  <div class="lookup-meta">{{ lookupResult.bankName }} · {{ lookupResult.destinationNumber }}</div>
                </div>
              </div>

              <div class="bank-field-label">Số tiền</div>
              <q-input
                v-model.number="form.amount"
                outlined
                type="number"
                min="1000"
                step="1000"
                suffix="VND"
                label="Tối thiểu 1.000 VND"
                :disable="isSubmitting"
                class="q-mb-md"
                :rules="[
                  val => Number(val) >= 1000 || 'Số tiền tối thiểu là 1.000 VND',
                  val => Number(val) + estimatedFee <= selectedAccountBalance || 'Số dư không đủ bao gồm phí Napas'
                ]"
              />

              <div class="bank-field-label">Nội dung</div>
              <q-input
                v-model="form.description"
                outlined
                type="textarea"
                rows="2"
                maxlength="500"
                counter
                label="Nội dung chuyển tiền"
                :disable="isSubmitting"
                class="q-mb-md"
              />

              <div class="napas-preview q-mb-lg">
                <div>
                  <span>Số tiền chuyển</span>
                  <strong>{{ formatCurrency(Number(form.amount || 0)) }}</strong>
                </div>
                <div>
                  <span>Phí Napas</span>
                  <strong>{{ formatCurrency(estimatedFee) }}</strong>
                </div>
                <div>
                  <span>Tổng trích tài khoản</span>
                  <strong>{{ formatCurrency(totalDebit) }}</strong>
                </div>
              </div>

              <q-btn
                unelevated
                color="primary"
                icon="send"
                label="Xác nhận chuyển Napas"
                type="submit"
                size="lg"
                class="bank-action-btn full-width"
                :loading="isSubmitting"
                :disable="!canSubmit"
              />
            </q-form>
          </q-card-section>
        </q-card>
      </div>

      <div class="col-12 col-lg-5">
        <q-card flat class="bank-card">
          <q-card-section class="q-pa-lg">
            <div class="napas-section-title">
              <q-icon name="history" color="primary" size="24px" />
              <span>Giao dịch gần đây</span>
            </div>

            <div v-if="isLoading" class="q-gutter-md q-mt-md">
              <q-skeleton v-for="i in 4" :key="i" type="rect" height="82px" />
            </div>

            <div v-else-if="recentTransfers.length === 0" class="empty-state">
              <q-icon name="public" size="56px" color="grey-4" />
              <div class="text-grey-7 q-mt-md">Chưa có giao dịch Napas.</div>
            </div>

            <div v-else class="recent-list q-mt-md">
              <div v-for="item in recentTransfers" :key="item.id" class="recent-row">
                <div>
                  <div class="recent-title">{{ item.destinationName }}</div>
                  <div class="recent-meta">{{ item.externalBankName }} · {{ formatDateTime(item.createdAtUtc) }}</div>
                </div>
                <div class="recent-amount">
                  {{ formatCurrency(item.amount, item.currency) }}
                  <span>Phí {{ formatCurrency(item.feeAmount, item.currency) }}</span>
                </div>
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
          <div class="text-h5 text-weight-bold q-mt-md">Chuyển Napas thành công</div>
          <div class="text-grey-7 q-mt-sm">Giao dịch đã được xử lý qua cổng giả lập Napas 24/7.</div>
        </q-card-section>
        <q-separator />
        <q-card-section v-if="receipt" class="q-pa-lg">
          <div class="receipt-line">
            <span>Mã tham chiếu</span>
            <strong>{{ receipt.reference }}</strong>
          </div>
          <div class="receipt-line">
            <span>Người nhận</span>
            <strong>{{ receipt.destinationName }}</strong>
          </div>
          <div class="receipt-line">
            <span>Ngân hàng</span>
            <strong>{{ receipt.externalBankName }}</strong>
          </div>
          <div class="receipt-line">
            <span>Tổng trích</span>
            <strong>{{ formatCurrency(receipt.totalDebitAmount, receipt.currency) }}</strong>
          </div>
          <div class="receipt-line">
            <span>Số dư còn lại</span>
            <strong>{{ formatCurrency(receipt.remainingBalance, receipt.currency) }}</strong>
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
import { computed, onMounted, reactive, ref } from 'vue'
import AppAlert from '~/components/common/AppAlert.vue'
import { useAccountService } from '~/services/accountService'
import { useNapasService } from '~/services/napasService'
import type { AccountSummary } from '~/types/account'
import type { NapasBank, NapasLookupResult, NapasTransferListItem, NapasTransferReceipt } from '~/types/napas'
import { formatCurrency } from '~/utils/currency'
import { formatDateTime } from '~/utils/date'

definePageMeta({
  middleware: ['customer']
})

const accountService = useAccountService()
const napasService = useNapasService()

const accounts = ref<AccountSummary[]>([])
const banks = ref<NapasBank[]>([])
const recentTransfers = ref<NapasTransferListItem[]>([])
const lookupResult = ref<NapasLookupResult | null>(null)
const receipt = ref<NapasTransferReceipt | null>(null)
const isLoading = ref(true)
const isLookingUp = ref(false)
const isSubmitting = ref(false)
const receiptDialog = ref(false)
const errorMessage = ref('')
const successMessage = ref('')

const form = reactive({
  sourceAccountId: '',
  bankCode: '',
  destinationNumber: '',
  destinationType: 'ACCOUNT' as 'ACCOUNT' | 'CARD',
  amount: 100000,
  description: 'Chuyen tien Napas 24/7'
})

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

const bankOptions = computed(() =>
  banks.value
    .filter(bank => form.destinationType === 'ACCOUNT' ? bank.supportsAccountTransfer : bank.supportsCardTransfer)
    .map(bank => ({
      label: `${bank.shortName} (${bank.code})`,
      value: bank.code
    }))
)

const selectedAccount = computed(() =>
  accounts.value.find(account => account.id === form.sourceAccountId)
)

const selectedAccountBalance = computed(() => selectedAccount.value?.balance ?? 0)

const estimatedFee = computed(() => calculateFee(Number(form.amount || 0)))
const totalDebit = computed(() => Number(form.amount || 0) + estimatedFee.value)

const canLookup = computed(() =>
  !!form.bankCode && normalizeDigits(form.destinationNumber).length >= 6
)

const canSubmit = computed(() =>
  !!form.sourceAccountId &&
  !!lookupResult.value &&
  Number(form.amount) >= 1000 &&
  totalDebit.value <= selectedAccountBalance.value
)

const loadData = async () => {
  isLoading.value = true
  errorMessage.value = ''

  try {
    const [accountData, bankData, transferData] = await Promise.all([
      accountService.getAccounts(),
      napasService.getBanks(),
      napasService.getTransfers({ page: 1, pageSize: 5 })
    ])

    accounts.value = accountData
    banks.value = bankData
    recentTransfers.value = transferData.items

    if (!form.sourceAccountId && accountOptions.value.length > 0) {
      form.sourceAccountId = accountOptions.value[0].value
    }

    if (!form.bankCode && bankOptions.value.length > 0) {
      form.bankCode = bankOptions.value[0].value
    }
  } catch (e: any) {
    errorMessage.value = e?.data?.message || e?.message || 'Không thể tải dữ liệu Napas.'
  } finally {
    isLoading.value = false
  }
}

const lookupDestination = async () => {
  if (!canLookup.value) return

  isLookingUp.value = true
  errorMessage.value = ''

  try {
    lookupResult.value = await napasService.lookup({
      bankCode: form.bankCode,
      destinationNumber: normalizeDigits(form.destinationNumber),
      destinationType: form.destinationType
    })
  } catch (e: any) {
    lookupResult.value = null
    errorMessage.value = e?.data?.message || e?.message || 'Không thể tra cứu người nhận Napas.'
  } finally {
    isLookingUp.value = false
  }
}

const submitTransfer = async () => {
  if (!canSubmit.value || !lookupResult.value) return

  isSubmitting.value = true
  errorMessage.value = ''
  successMessage.value = ''

  try {
    receipt.value = await napasService.transfer({
      sourceAccountId: form.sourceAccountId,
      bankCode: form.bankCode,
      destinationNumber: normalizeDigits(form.destinationNumber),
      destinationType: form.destinationType,
      amount: Number(form.amount),
      description: form.description
    })
    receiptDialog.value = true
    successMessage.value = `Đã chuyển Napas thành công. Mã tham chiếu ${receipt.value.reference}.`
    await loadData()
  } catch (e: any) {
    errorMessage.value = e?.data?.message || e?.message || 'Không thể thực hiện chuyển Napas.'
  } finally {
    isSubmitting.value = false
  }
}

const clearLookup = () => {
  lookupResult.value = null
}

const normalizeDigits = (value: string) => value.replace(/\D/g, '')

const calculateFee = (amount: number) => {
  if (amount >= 100000000) return 11000
  if (amount >= 10000000) return 5500
  return amount > 0 ? 3300 : 0
}

onMounted(loadData)
</script>

<style scoped>
.napas-form-card {
  border-top: 4px solid #1f6fd1;
}

.napas-section-title {
  display: flex;
  align-items: center;
  gap: 10px;
  color: #1d2939;
  font-size: 20px;
  font-weight: 850;
}

.napas-toggle {
  height: 56px;
}

.lookup-btn {
  height: 56px;
  min-width: 132px;
}

.lookup-card {
  border: 1px solid #abefc6;
  border-radius: 8px;
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 14px;
}

.lookup-ok {
  background: #f0fdf4;
}

.lookup-name {
  color: #1d2939;
  font-size: 17px;
  font-weight: 850;
}

.lookup-meta {
  color: #667085;
}

.napas-preview {
  border: 1px solid #d0d5dd;
  border-radius: 8px;
  background: #f9fafb;
  padding: 16px;
}

.napas-preview > div,
.receipt-line {
  display: flex;
  justify-content: space-between;
  gap: 16px;
  padding: 8px 0;
}

.napas-preview span,
.receipt-line span {
  color: #667085;
  font-weight: 700;
}

.napas-preview strong,
.receipt-line strong {
  color: #1d2939;
  text-align: right;
}

.empty-state {
  text-align: center;
  padding: 40px 16px;
}

.recent-list {
  display: grid;
  gap: 12px;
}

.recent-row {
  border: 1px solid #eaecf0;
  border-radius: 8px;
  display: flex;
  justify-content: space-between;
  gap: 16px;
  padding: 14px;
}

.recent-title {
  color: #1d2939;
  font-weight: 850;
}

.recent-meta {
  color: #667085;
  font-size: 13px;
  margin-top: 4px;
}

.recent-amount {
  color: #1f6fd1;
  font-weight: 850;
  text-align: right;
}

.recent-amount span {
  color: #667085;
  display: block;
  font-size: 12px;
  margin-top: 4px;
}

.receipt-card {
  width: min(560px, calc(100vw - 32px));
}

@media (max-width: 720px) {
  .lookup-btn {
    width: 100%;
  }

  .recent-row,
  .napas-preview > div,
  .receipt-line {
    flex-direction: column;
  }

  .recent-amount,
  .napas-preview strong,
  .receipt-line strong {
    text-align: left;
  }
}
</style>
