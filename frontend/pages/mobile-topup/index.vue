<template>
  <div class="bank-page">
    <div class="bank-page-header">
      <div>
        <div class="bank-page-kicker">Dịch vụ di động</div>
        <div class="bank-page-title">Nạp điện thoại & mua mã thẻ</div>
        <div class="bank-page-subtitle">Giả lập kết nối nhà mạng để nạp tiền, mua mã thẻ và đăng ký data ngay trong SuperBanking.</div>
      </div>
      <q-btn unelevated color="primary" icon="refresh" label="Cập nhật" class="bank-action-btn" :loading="isLoading" @click="loadData" />
    </div>

    <AppAlert v-if="errorMessage" type="error" :message="errorMessage" class="q-mb-md" @dismiss="errorMessage = ''" />
    <AppAlert v-if="successMessage" type="success" :message="successMessage" class="q-mb-md" @dismiss="successMessage = ''" />

    <div class="row q-col-gutter-lg">
      <div class="col-12 col-lg-7">
        <q-card flat class="bank-card">
          <q-card-section class="q-pa-lg">
            <div class="topup-section-title">
              <q-icon name="phone_iphone" color="primary" size="24px" />
              <span>Thông tin dịch vụ</span>
            </div>

            <q-form class="q-mt-lg" @submit.prevent="submitPurchase">
              <div class="bank-field-label">Tài khoản thanh toán</div>
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
                :rules="[val => !!val || 'Vui lòng chọn tài khoản thanh toán']"
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
                <div class="col-12 col-md-5">
                  <div class="bank-field-label">Nhà mạng</div>
                  <q-select
                    v-model="form.providerCode"
                    outlined
                    emit-value
                    map-options
                    :options="providerOptions"
                    option-label="label"
                    option-value="value"
                    label="Chọn nhà mạng"
                    :disable="isSubmitting"
                    class="q-mb-md"
                    @update:model-value="selectFirstProduct"
                  />
                </div>
                <div class="col-12 col-md-7">
                  <div class="bank-field-label">Loại dịch vụ</div>
                  <q-btn-toggle
                    v-model="productTypeFilter"
                    spread
                    unelevated
                    toggle-color="primary"
                    color="grey-3"
                    text-color="dark"
                    :options="productTypeOptions"
                    class="topup-toggle q-mb-md"
                    @update:model-value="selectFirstProduct"
                  />
                </div>
              </div>

              <div v-if="requiresPhone" class="bank-field-label">Số điện thoại</div>
              <q-input
                v-if="requiresPhone"
                v-model="form.phoneNumber"
                outlined
                mask="###########"
                label="Nhập số điện thoại nhận dịch vụ"
                :disable="isSubmitting"
                class="q-mb-md"
                :rules="[
                  val => !!val || 'Vui lòng nhập số điện thoại',
                  val => normalizePhone(val).length >= 9 || 'Số điện thoại phải có ít nhất 9 chữ số'
                ]"
              />

              <div class="bank-field-label">Gói dịch vụ</div>
              <div class="product-grid q-mb-lg">
                <button
                  v-for="product in filteredProducts"
                  :key="product.code"
                  type="button"
                  class="product-tile"
                  :class="{ 'product-tile-active': form.productCode === product.code }"
                  :disabled="isSubmitting"
                  @click="form.productCode = product.code"
                >
                  <span>{{ product.name }}</span>
                  <strong>{{ formatCurrency(product.amount, product.currency) }}</strong>
                </button>
              </div>

              <div class="topup-preview q-mb-lg">
                <div>
                  <span>Dịch vụ</span>
                  <strong>{{ selectedProvider?.name || '-' }}</strong>
                </div>
                <div>
                  <span>Gói đã chọn</span>
                  <strong>{{ selectedProduct?.name || '-' }}</strong>
                </div>
                <div>
                  <span>Tổng thanh toán</span>
                  <strong>{{ formatCurrency(selectedProduct?.amount || 0) }}</strong>
                </div>
                <div>
                  <span>Số dư sau giao dịch</span>
                  <strong>{{ formatCurrency(remainingBalance) }}</strong>
                </div>
              </div>

              <q-btn
                unelevated
                color="primary"
                icon="payments"
                label="Xác nhận thanh toán"
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
        <q-card flat class="bank-card q-mb-lg">
          <q-card-section class="q-pa-lg">
            <div class="topup-section-title">
              <q-icon name="receipt_long" color="primary" size="24px" />
              <span>Kết quả gần nhất</span>
            </div>

            <div v-if="lastReceipt" class="receipt-box q-mt-md">
              <div class="receipt-line">
                <span>Mã giao dịch</span>
                <strong>{{ lastReceipt.reference }}</strong>
              </div>
              <div class="receipt-line">
                <span>Dịch vụ</span>
                <strong>{{ lastReceipt.productName }}</strong>
              </div>
              <div v-if="lastReceipt.phoneNumber" class="receipt-line">
                <span>Số điện thoại</span>
                <strong>{{ lastReceipt.phoneNumber }}</strong>
              </div>
              <div v-if="lastReceipt.cardSerial" class="receipt-line">
                <span>Serial</span>
                <strong>{{ lastReceipt.cardSerial }}</strong>
              </div>
              <div v-if="lastReceipt.cardPin" class="receipt-line pin-line">
                <span>Mã thẻ</span>
                <strong>{{ lastReceipt.cardPin }}</strong>
              </div>
              <div class="receipt-line">
                <span>Số tiền</span>
                <strong>{{ formatCurrency(lastReceipt.amount, lastReceipt.currency) }}</strong>
              </div>
            </div>

            <div v-else class="empty-state">
              <q-icon name="phone_iphone" size="56px" color="grey-4" />
              <div class="text-grey-7 q-mt-md">Chưa có giao dịch trong phiên này.</div>
            </div>
          </q-card-section>
        </q-card>

        <q-card flat class="bank-card">
          <q-card-section class="q-pa-lg">
            <div class="topup-section-title">
              <q-icon name="history" color="primary" size="24px" />
              <span>Lịch sử gần đây</span>
            </div>

            <div v-if="isLoading" class="q-gutter-md q-mt-md">
              <q-skeleton v-for="i in 4" :key="i" type="rect" height="82px" />
            </div>

            <div v-else-if="history.length === 0" class="empty-state">
              <q-icon name="history" size="56px" color="grey-4" />
              <div class="text-grey-7 q-mt-md">Chưa có lịch sử nạp điện thoại.</div>
            </div>

            <div v-else class="recent-list q-mt-md">
              <div v-for="item in history" :key="item.id" class="recent-row">
                <div>
                  <div class="recent-title">{{ item.productName }}</div>
                  <div class="recent-meta">{{ item.providerName }} · {{ item.phoneNumber || 'Mã thẻ' }} · {{ formatDateTime(item.createdAtUtc) }}</div>
                </div>
                <div class="recent-amount">
                  {{ formatCurrency(item.amount, item.currency) }}
                  <span>{{ statusLabel(item.status) }}</span>
                </div>
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
import { useMobileTopupService } from '~/services/mobileTopupService'
import type { AccountSummary } from '~/types/account'
import type { MobileProvider, MobileTopUpListItem, MobileTopUpReceipt } from '~/types/mobileTopup'
import { formatCurrency } from '~/utils/currency'
import { formatDateTime } from '~/utils/date'

definePageMeta({
  middleware: ['customer']
})

const accountService = useAccountService()
const mobileTopupService = useMobileTopupService()

const accounts = ref<AccountSummary[]>([])
const providers = ref<MobileProvider[]>([])
const history = ref<MobileTopUpListItem[]>([])
const lastReceipt = ref<MobileTopUpReceipt | null>(null)
const isLoading = ref(true)
const isSubmitting = ref(false)
const errorMessage = ref('')
const successMessage = ref('')
const productTypeFilter = ref('PhoneTopUp')

const form = reactive({
  sourceAccountId: '',
  providerCode: '',
  productCode: '',
  phoneNumber: ''
})

const productTypeOptions = [
  { label: 'Nạp tiền', value: 'PhoneTopUp' },
  { label: 'Mã thẻ', value: 'CardCode' },
  { label: 'Data', value: 'DataPackage' }
]

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

const providerOptions = computed(() =>
  providers.value.map(provider => ({
    label: provider.name,
    value: provider.code
  }))
)

const selectedAccount = computed(() => accounts.value.find(account => account.id === form.sourceAccountId))
const selectedProvider = computed(() => providers.value.find(provider => provider.code === form.providerCode))
const filteredProducts = computed(() =>
  selectedProvider.value?.products.filter(product => product.productType === productTypeFilter.value) || []
)
const selectedProduct = computed(() => filteredProducts.value.find(product => product.code === form.productCode))
const requiresPhone = computed(() => productTypeFilter.value !== 'CardCode')
const remainingBalance = computed(() => Math.max((selectedAccount.value?.balance || 0) - (selectedProduct.value?.amount || 0), 0))
const canSubmit = computed(() =>
  !!form.sourceAccountId
  && !!form.providerCode
  && !!form.productCode
  && !!selectedProduct.value
  && (selectedAccount.value?.balance || 0) >= (selectedProduct.value?.amount || 0)
  && (!requiresPhone.value || normalizePhone(form.phoneNumber).length >= 9)
)

const loadData = async () => {
  isLoading.value = true
  errorMessage.value = ''

  try {
    const [accountResult, providerResult, historyResult] = await Promise.all([
      accountService.getAccounts(),
      mobileTopupService.getProviders(),
      mobileTopupService.getPurchases({ page: 1, pageSize: 8 })
    ])
    accounts.value = accountResult
    providers.value = providerResult
    history.value = historyResult.items

    if (!form.sourceAccountId && accountOptions.value.length > 0) {
      form.sourceAccountId = accountOptions.value[0].value
    }

    if (!form.providerCode && providerOptions.value.length > 0) {
      form.providerCode = providerOptions.value[0].value
    }

    selectFirstProduct()
  } catch (e: any) {
    errorMessage.value = e?.data?.message || e?.message || 'Không thể tải dữ liệu nạp điện thoại.'
  } finally {
    isLoading.value = false
  }
}

const selectFirstProduct = () => {
  const available = filteredProducts.value
  form.productCode = available[0]?.code || ''
}

const submitPurchase = async () => {
  if (!canSubmit.value) return

  isSubmitting.value = true
  errorMessage.value = ''
  successMessage.value = ''

  try {
    const receipt = await mobileTopupService.purchase({
      sourceAccountId: form.sourceAccountId,
      providerCode: form.providerCode,
      productCode: form.productCode,
      phoneNumber: requiresPhone.value ? normalizePhone(form.phoneNumber) : null
    })

    lastReceipt.value = receipt
    successMessage.value = `Thanh toán thành công ${receipt.productName}.`
    await loadData()
  } catch (e: any) {
    errorMessage.value = e?.data?.message || e?.message || 'Không thể hoàn tất giao dịch di động.'
  } finally {
    isSubmitting.value = false
  }
}

const normalizePhone = (value: string) => value.replace(/\D/g, '')

const statusLabel = (status: string) => {
  switch (status) {
    case 'Completed':
    case 'COMPLETED':
      return 'Thành công'
    case 'Pending':
    case 'PENDING':
      return 'Chờ xử lý'
    case 'Failed':
    case 'FAILED':
      return 'Thất bại'
    default:
      return status
  }
}

onMounted(loadData)
</script>

<style scoped>
.topup-section-title {
  align-items: center;
  color: #1d2939;
  display: flex;
  font-size: 18px;
  font-weight: 850;
  gap: 10px;
}

.topup-toggle {
  min-height: 40px;
}

.product-grid {
  display: grid;
  gap: 12px;
  grid-template-columns: repeat(auto-fit, minmax(170px, 1fr));
}

.product-tile {
  background: #ffffff;
  border: 1px solid #d9e2ef;
  border-radius: 8px;
  color: #1d2939;
  cursor: pointer;
  min-height: 92px;
  padding: 14px;
  text-align: left;
}

.product-tile span {
  color: #344054;
  display: block;
  font-weight: 750;
  min-height: 38px;
}

.product-tile strong {
  color: #1f6fd1;
  display: block;
  font-size: 18px;
  font-weight: 900;
  margin-top: 8px;
}

.product-tile-active {
  background: #e7f0ff;
  border-color: #1f6fd1;
  box-shadow: 0 12px 28px rgba(31, 111, 209, 0.14);
}

.topup-preview,
.receipt-box {
  border-radius: 8px;
  display: grid;
  gap: 12px;
  padding: 16px;
}

.topup-preview {
  background: #f8fafc;
  border: 1px solid #d9e2ef;
}

.receipt-box {
  background: #ecfdf3;
  border: 1px solid #abefc6;
}

.topup-preview div,
.receipt-line,
.recent-row {
  display: flex;
  gap: 12px;
  justify-content: space-between;
}

.topup-preview span,
.receipt-line span {
  color: #667085;
  font-weight: 700;
}

.topup-preview strong,
.receipt-line strong {
  color: #1d2939;
  text-align: right;
}

.pin-line strong {
  color: #1f6fd1;
  font-size: 20px;
  letter-spacing: 1px;
}

.recent-list {
  display: grid;
  gap: 10px;
}

.recent-row {
  align-items: center;
  border: 1px solid #eaecf0;
  border-radius: 8px;
  padding: 12px;
}

.recent-title {
  color: #1d2939;
  font-weight: 850;
}

.recent-meta {
  color: #667085;
  font-size: 12px;
  font-weight: 650;
  margin-top: 4px;
}

.recent-amount {
  color: #1f6fd1;
  font-weight: 900;
  text-align: right;
}

.recent-amount span {
  color: #667085;
  display: block;
  font-size: 12px;
  font-weight: 700;
  margin-top: 4px;
}

.empty-state {
  padding: 36px 12px;
  text-align: center;
}

@media (max-width: 720px) {
  .topup-preview div,
  .receipt-line,
  .recent-row {
    align-items: flex-start;
    flex-direction: column;
  }

  .topup-preview strong,
  .receipt-line strong,
  .recent-amount {
    text-align: left;
  }
}
</style>
