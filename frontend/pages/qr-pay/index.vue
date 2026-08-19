<template>
  <div class="bank-page">
    <div class="bank-page-header">
      <div>
        <div class="bank-page-kicker">Thanh toán mã QR</div>
        <div class="bank-page-title">QR Pay</div>
        <div class="bank-page-subtitle">Tạo QR nhận tiền LocalLink và đọc nhanh payload QR/VietQR để điền form chuyển khoản.</div>
      </div>
      <q-btn unelevated color="primary" icon="refresh" label="Cập nhật" class="bank-action-btn" :loading="isLoading" @click="loadAccounts" />
    </div>

    <AppAlert v-if="errorMessage" type="error" :message="errorMessage" class="q-mb-md" @dismiss="errorMessage = ''" />
    <AppAlert v-if="successMessage" type="success" :message="successMessage" class="q-mb-md" @dismiss="successMessage = ''" />

    <div class="row q-col-gutter-lg">
      <div class="col-12 col-lg-6">
        <q-card flat class="bank-card">
          <q-card-section class="q-pa-lg">
            <div class="qr-section-title">
              <q-icon name="qr_code_2" color="primary" size="24px" />
              <span>Tạo QR nhận tiền</span>
            </div>

            <q-form class="q-mt-lg" @submit.prevent="createPayload">
              <div class="bank-field-label">Tài khoản nhận tiền</div>
              <q-select
                v-model="receiveForm.accountId"
                outlined
                emit-value
                map-options
                :options="accountOptions"
                option-label="label"
                option-value="value"
                label="Chọn tài khoản nhận"
                :loading="isLoading"
                :disable="isCreating"
                class="q-mb-md"
                :rules="[val => !!val || 'Vui lòng chọn tài khoản nhận']"
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
                  <div class="bank-field-label">Số tiền</div>
                  <q-input
                    v-model.number="receiveForm.amount"
                    outlined
                    type="number"
                    min="0"
                    label="Tùy chọn"
                    :disable="isCreating"
                    class="q-mb-md"
                    @update:model-value="receiveForm.amount = Number($event) || null"
                  />
                </div>
                <div class="col-12 col-md-7">
                  <div class="bank-field-label">Nội dung</div>
                  <q-input
                    v-model="receiveForm.description"
                    outlined
                    label="Tùy chọn"
                    maxlength="200"
                    counter
                    :disable="isCreating"
                    class="q-mb-md"
                  />
                </div>
              </div>

              <div class="qr-receive-preview q-mb-lg">
                <div>
                  <span>Người nhận</span>
                  <strong>{{ selectedReceiveAccount?.accountName || '-' }}</strong>
                </div>
                <div>
                  <span>Số tài khoản</span>
                  <strong>{{ selectedReceiveAccount?.accountNumber || '-' }}</strong>
                </div>
                <div>
                  <span>Số tiền</span>
                  <strong>{{ receiveForm.amount ? formatCurrency(receiveForm.amount) : 'Người chuyển nhập' }}</strong>
                </div>
              </div>

              <q-btn
                unelevated
                color="primary"
                icon="qr_code"
                label="Tạo QR"
                type="submit"
                size="lg"
                class="bank-action-btn full-width"
                :loading="isCreating"
                :disable="!receiveForm.accountId"
              />
            </q-form>

            <div v-if="generatedPayload" class="qr-output q-mt-lg">
              <div class="qr-visual" aria-label="QR Pay demo matrix">
                <span
                  v-for="cell in qrCells"
                  :key="cell.index"
                  :class="{ active: cell.active }"
                />
              </div>

              <div class="qr-output-info">
                <div class="qr-mini-label">Payload QR</div>
                <div class="qr-payload-text">{{ generatedPayload.payload }}</div>
                <div class="row q-col-gutter-sm q-mt-md">
                  <div class="col-12 col-sm-6">
                    <q-btn outline color="primary" icon="content_copy" label="Sao chép" class="full-width" @click="copyPayload(generatedPayload.payload)" />
                  </div>
                  <div class="col-12 col-sm-6">
                    <q-btn flat color="primary" icon="input" label="Thử đọc QR này" class="full-width" @click="parseGeneratedPayload" />
                  </div>
                </div>
              </div>
            </div>
          </q-card-section>
        </q-card>
      </div>

      <div class="col-12 col-lg-6">
        <q-card flat class="bank-card">
          <q-card-section class="q-pa-lg">
            <div class="qr-section-title">
              <q-icon name="qr_code_scanner" color="primary" size="24px" />
              <span>Quét hoặc nhập QR</span>
            </div>

            <q-form class="q-mt-lg" @submit.prevent="parsePayload">
              <div class="bank-field-label">Payload QR / VietQR</div>
              <q-input
                v-model="scanPayload"
                outlined
                type="textarea"
                autogrow
                label="Dán nội dung QR tại đây"
                :disable="isParsing"
                class="q-mb-md"
                :rules="[val => !!val || 'Vui lòng nhập payload QR']"
              />

              <div class="row q-col-gutter-sm">
                <div class="col-12 col-sm-6">
                  <q-btn
                    unelevated
                    color="primary"
                    icon="fact_check"
                    label="Đọc thông tin"
                    type="submit"
                    class="bank-action-btn full-width"
                    :loading="isParsing"
                  />
                </div>
                <div class="col-12 col-sm-6">
                  <q-btn outline color="primary" icon="science" label="Payload mẫu" class="full-width" @click="useSamplePayload" />
                </div>
              </div>
            </q-form>

            <div v-if="parsedPayload" class="parsed-box q-mt-lg">
              <div class="parsed-head">
                <div>
                  <div class="qr-mini-label">Kết quả đọc QR</div>
                  <div class="parsed-title">{{ parsedPayload.paymentRail === 'LOCALBANK' ? 'LocalLink nội bộ' : 'VietQR / ngân hàng ngoài' }}</div>
                </div>
                <q-badge color="positive" outline>{{ parsedPayload.paymentRail }}</q-badge>
              </div>

              <div class="parsed-lines">
                <div>
                  <span>Ngân hàng</span>
                  <strong>{{ parsedPayload.bankName || parsedPayload.bankCode }}</strong>
                </div>
                <div>
                  <span>Tài khoản nhận</span>
                  <strong>{{ parsedPayload.accountNumber }}</strong>
                </div>
                <div>
                  <span>Chủ tài khoản</span>
                  <strong>{{ parsedPayload.accountName || 'Chưa có trong payload' }}</strong>
                </div>
                <div>
                  <span>Số tiền</span>
                  <strong>{{ parsedPayload.amount ? formatCurrency(parsedPayload.amount) : 'Người chuyển nhập' }}</strong>
                </div>
                <div>
                  <span>Nội dung</span>
                  <strong>{{ parsedPayload.description || 'Không có nội dung' }}</strong>
                </div>
              </div>

              <AppAlert v-if="parsedPayload.warning" type="warning" :message="parsedPayload.warning" class="q-mt-md" />

              <q-btn
                unelevated
                color="primary"
                :icon="parsedPayload.paymentRail === 'LOCALBANK' ? 'swap_horiz' : 'public'"
                :label="parsedPayload.paymentRail === 'LOCALBANK' ? 'Chuyển khoản LocalLink' : 'Chuyển qua Napas'"
                size="lg"
                class="bank-action-btn full-width q-mt-lg"
                @click="continuePayment"
              />
            </div>

            <div v-else class="empty-state q-mt-lg">
              <q-icon name="qr_code_scanner" size="56px" color="grey-4" />
              <div class="text-grey-7 q-mt-md">Chưa có QR nào được đọc trong phiên này.</div>
            </div>
          </q-card-section>
        </q-card>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import AppAlert from '~/components/common/AppAlert.vue'
import { useAccountService } from '~/services/accountService'
import { useQrPayService } from '~/services/qrPayService'
import { formatCurrency } from '~/utils/currency'
import type { AccountSummary, ParsedQrPay, QrPayPayload } from '~/types'

definePageMeta({
  layout: 'default',
  middleware: ['customer']
})

const router = useRouter()
const accountService = useAccountService()
const qrPayService = useQrPayService()

const isLoading = ref(false)
const isCreating = ref(false)
const isParsing = ref(false)
const errorMessage = ref('')
const successMessage = ref('')
const accounts = ref<AccountSummary[]>([])
const generatedPayload = ref<QrPayPayload | null>(null)
const parsedPayload = ref<ParsedQrPay | null>(null)
const scanPayload = ref('')

const receiveForm = ref({
  accountId: '',
  amount: null as number | null,
  description: ''
})

const activeAccounts = computed(() => accounts.value.filter(account => account.status === 'ACTIVE' || account.status === 'Active'))

const accountOptions = computed(() => activeAccounts.value.map(account => ({
  ...account,
  label: `${account.accountNumber} - ${account.accountName}`,
  value: account.id
})))

const selectedReceiveAccount = computed(() => activeAccounts.value.find(account => account.id === receiveForm.value.accountId))

function hashString(value: string) {
  let hash = 17
  for (let i = 0; i < value.length; i += 1) {
    hash = ((hash << 5) - hash + value.charCodeAt(i)) | 0
  }
  return Math.abs(hash)
}

function isFinder(x: number, y: number, size: number) {
  const inTopLeft = x < 7 && y < 7
  const inTopRight = x >= size - 7 && y < 7
  const inBottomLeft = x < 7 && y >= size - 7
  if (!inTopLeft && !inTopRight && !inBottomLeft) {
    return false
  }

  const localX = x < 7 ? x : x - (size - 7)
  const localY = y < 7 ? y : y - (size - 7)
  return localX === 0 || localY === 0 || localX === 6 || localY === 6 || (localX >= 2 && localX <= 4 && localY >= 2 && localY <= 4)
}

const qrCells = computed(() => {
  const size = 29
  const payload = generatedPayload.value?.payload || ''
  const seed = hashString(payload)
  const cells: Array<{ index: number, active: boolean }> = []

  for (let y = 0; y < size; y += 1) {
    for (let x = 0; x < size; x += 1) {
      cells.push({
        index: y * size + x,
        active: isFinder(x, y, size) || (((x * 37 + y * 19 + seed + payload.charCodeAt((x + y) % Math.max(payload.length, 1))) % 7) < 3)
      })
    }
  }

  return cells
})

async function loadAccounts() {
  isLoading.value = true
  errorMessage.value = ''

  try {
    accounts.value = await accountService.getAccounts()
    if (!receiveForm.value.accountId && activeAccounts.value.length > 0) {
      receiveForm.value.accountId = activeAccounts.value[0].id
    }
  } catch (error) {
    errorMessage.value = 'Không thể tải danh sách tài khoản.'
  } finally {
    isLoading.value = false
  }
}

onMounted(loadAccounts)

const createPayload = async () => {
  errorMessage.value = ''
  successMessage.value = ''
  isCreating.value = true

  try {
    generatedPayload.value = await qrPayService.createReceivePayload({
      accountId: receiveForm.value.accountId,
      amount: receiveForm.value.amount && receiveForm.value.amount > 0 ? receiveForm.value.amount : null,
      description: receiveForm.value.description || null
    })
    successMessage.value = 'Đã tạo payload QR nhận tiền.'
  } catch (error: any) {
    errorMessage.value = error.response?._data?.detail || 'Không thể tạo QR nhận tiền.'
  } finally {
    isCreating.value = false
  }
}

const parsePayload = async () => {
  errorMessage.value = ''
  successMessage.value = ''
  isParsing.value = true

  try {
    parsedPayload.value = await qrPayService.parsePayload({ payload: scanPayload.value })
    successMessage.value = 'Đã đọc thông tin QR.'
  } catch (error: any) {
    parsedPayload.value = null
    errorMessage.value = error.response?._data?.detail || 'Không thể đọc payload QR.'
  } finally {
    isParsing.value = false
  }
}

const parseGeneratedPayload = async () => {
  if (!generatedPayload.value) return
  scanPayload.value = generatedPayload.value.payload
  await parsePayload()
}

const useSamplePayload = () => {
  scanPayload.value = 'https://vietqr.local/pay?bank=970436&bankName=Vietcombank&account=1029384756&name=TRAN%20MINH%20KHOA&amount=250000&content=Thanh%20toan%20don%20hang'
}

const continuePayment = () => {
  if (!parsedPayload.value) return

  const query = {
    destinationAccount: parsedPayload.value.accountNumber,
    amount: parsedPayload.value.amount ? String(parsedPayload.value.amount) : undefined,
    description: parsedPayload.value.description || undefined,
    bankCode: parsedPayload.value.bankCode || undefined
  }

  if (parsedPayload.value.paymentRail === 'LOCALBANK') {
    router.push({ path: '/transfer', query })
    return
  }

  router.push({ path: '/napas', query })
}

const copyPayload = async (payload: string) => {
  try {
    await navigator.clipboard.writeText(payload)
    successMessage.value = 'Đã sao chép payload QR.'
  } catch (error) {
    errorMessage.value = 'Không thể sao chép payload.'
  }
}
</script>

<style scoped>
.qr-section-title {
  display: flex;
  align-items: center;
  gap: 10px;
  color: #1d2939;
  font-size: 20px;
  font-weight: 800;
}

.qr-receive-preview,
.parsed-lines {
  display: grid;
  gap: 12px;
  padding: 16px;
  background: #f8fafc;
  border: 1px solid #d9e2ef;
  border-radius: 8px;
}

.qr-receive-preview > div,
.parsed-lines > div {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
}

.qr-receive-preview span,
.parsed-lines span,
.qr-mini-label {
  color: #667085;
  font-size: 13px;
  font-weight: 700;
  text-transform: uppercase;
}

.qr-receive-preview strong,
.parsed-lines strong {
  color: #1d2939;
  text-align: right;
}

.qr-output {
  display: grid;
  grid-template-columns: 220px minmax(0, 1fr);
  gap: 18px;
  align-items: center;
  padding: 18px;
  background: #ffffff;
  border: 1px solid #d9e2ef;
  border-radius: 8px;
  box-shadow: 0 14px 32px rgba(16, 24, 40, 0.08);
}

.qr-visual {
  display: grid;
  grid-template-columns: repeat(29, 1fr);
  gap: 2px;
  width: 220px;
  aspect-ratio: 1;
  padding: 12px;
  background: #ffffff;
  border: 1px solid #c7dcff;
  border-radius: 8px;
}

.qr-visual span {
  background: #eef4ff;
  border-radius: 1px;
}

.qr-visual span.active {
  background: #1f6fd1;
}

.qr-output-info {
  min-width: 0;
}

.qr-payload-text {
  overflow-wrap: anywhere;
  padding: 12px;
  margin-top: 8px;
  color: #1d2939;
  background: #f8fafc;
  border: 1px dashed #b2ddff;
  border-radius: 8px;
  font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace;
  font-size: 13px;
  line-height: 1.5;
}

.parsed-box {
  padding: 18px;
  background: #ffffff;
  border: 1px solid #d9e2ef;
  border-radius: 8px;
}

.parsed-head {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 16px;
  margin-bottom: 16px;
}

.parsed-title {
  color: #1d2939;
  font-size: 22px;
  font-weight: 800;
}

.empty-state {
  min-height: 220px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  text-align: center;
  background: #f8fafc;
  border: 1px dashed #d9e2ef;
  border-radius: 8px;
}

@media (max-width: 700px) {
  .qr-output {
    grid-template-columns: 1fr;
  }

  .qr-visual {
    width: min(100%, 260px);
    margin: 0 auto;
  }

  .qr-receive-preview > div,
  .parsed-lines > div {
    align-items: flex-start;
    flex-direction: column;
    gap: 4px;
  }

  .qr-receive-preview strong,
  .parsed-lines strong {
    text-align: left;
  }
}
</style>
