<template>
  <div class="q-pa-md">
    <div class="row items-center q-mb-md">
      <div class="text-h6 text-primary">Chuyển tiền nội bộ</div>
    </div>

    <q-stepper
      v-model="step"
      ref="stepper"
      color="primary"
      animated
      flat
      bordered
    >
      <!-- STEP 1: INFORMATION -->
      <q-step
        :name="1"
        title="Thông tin chuyển tiền"
        icon="account_balance_wallet"
        :done="step > 1"
      >
        <AppAlert v-if="errorMessage" :message="errorMessage" type="error" @dismiss="errorMessage = ''" class="q-mb-md" />

        <q-form ref="form1" @submit="onValidateStep1" class="q-gutter-md">
          <div class="text-subtitle2 text-grey-8">Tài khoản nguồn</div>
          <q-select
            v-model="sourceAccountId"
            :options="activeAccounts"
            option-value="id"
            :option-label="opt => `${opt.accountNumber} - ${opt.accountName}`"
            emit-value
            map-options
            outlined
            label="Chọn tài khoản trích tiền *"
            :rules="[val => !!val || 'Vui lòng chọn tài khoản nguồn']"
          >
            <template v-slot:option="scope">
              <q-item v-bind="scope.itemProps">
                <q-item-section>
                  <q-item-label>{{ scope.opt.accountNumber }} - {{ scope.opt.accountName }}</q-item-label>
                  <q-item-label caption>
                    Số dư: <span class="text-weight-bold text-primary">{{ formatCurrency(scope.opt.balance) }}</span>
                  </q-item-label>
                </q-item-section>
              </q-item>
            </template>
          </q-select>

          <div class="text-subtitle2 text-grey-8 q-mt-md">Tài khoản đích</div>
          <div class="row q-col-gutter-sm items-start">
            <div class="col-12 col-md-8">
              <q-input
                v-model.trim="destinationAccountNumber"
                label="Số tài khoản người nhận *"
                outlined
                :rules="[
                  val => !!val || 'Số tài khoản không được để trống',
                  val => val !== selectedSourceAccount?.accountNumber || 'Không thể chuyển tiền đến chính tài khoản nguồn'
                ]"
                @update:model-value="onDestinationChanged"
              >
                <template v-slot:append>
                  <q-btn icon="search" flat round color="primary" @click="lookupDestination" :loading="isLookingUp">
                    <q-tooltip>Kiểm tra tài khoản</q-tooltip>
                  </q-btn>
                </template>
              </q-input>
            </div>
            <div class="col-12 col-md-4">
              <q-btn
                outline
                color="primary"
                icon="contacts"
                label="Danh bạ"
                class="full-width"
                style="height: 56px"
                @click="showBeneficiaries = true"
              />
            </div>
          </div>

          <!-- Lookup Result -->
          <q-card v-if="destinationName" flat bordered class="bg-blue-1 q-mb-md">
            <q-card-section class="q-py-sm">
              <div class="text-caption text-grey-7">Chủ tài khoản:</div>
              <div class="text-subtitle1 text-weight-bold text-primary">{{ destinationName }}</div>
            </q-card-section>
          </q-card>
          <q-card v-if="lookupError" flat bordered class="bg-red-1 q-mb-md">
            <q-card-section class="q-py-sm text-negative">
              {{ lookupError }}
            </q-card-section>
          </q-card>

          <div class="text-subtitle2 text-grey-8 q-mt-md">Số tiền & Nội dung</div>
          <q-input
            v-model.number="amount"
            type="number"
            label="Số tiền chuyển (VND) *"
            outlined
            :rules="[
              val => !!val || 'Số tiền không được để trống',
              val => val > 0 || 'Số tiền phải lớn hơn 0',
              val => !selectedSourceAccount || val <= selectedSourceAccount.balance || 'Số dư không đủ để thực hiện giao dịch'
            ]"
            @update:model-value="amount = Number($event)"
          />
          <div v-if="amount > 0" class="text-caption text-primary text-weight-bold q-mb-md">
            Bằng chữ: ({{ formatCurrency(amount) }})
          </div>

          <q-input
            v-model="description"
            label="Nội dung chuyển tiền"
            outlined
            autogrow
            counter
            maxlength="200"
          />

          <q-stepper-navigation>
            <q-btn type="submit" color="primary" label="Tiếp tục" />
          </q-stepper-navigation>
        </q-form>
      </q-step>

      <!-- STEP 2: CONFIRMATION -->
      <q-step
        :name="2"
        title="Xác nhận"
        icon="verified"
        :done="step > 2"
      >
        <AppAlert v-if="errorMessage" :message="errorMessage" type="error" @dismiss="errorMessage = ''" class="q-mb-md" />

        <q-list bordered separator class="rounded-borders q-mb-md bg-white">
          <q-item>
            <q-item-section>
              <q-item-label caption>Tài khoản nguồn</q-item-label>
              <q-item-label class="text-weight-medium">{{ selectedSourceAccount?.accountNumber }}</q-item-label>
            </q-item-section>
          </q-item>
          <q-item>
            <q-item-section>
              <q-item-label caption>Tài khoản nhận</q-item-label>
              <q-item-label class="text-weight-medium">{{ destinationAccountNumber }}</q-item-label>
              <q-item-label class="text-primary text-weight-bold">{{ destinationName }}</q-item-label>
            </q-item-section>
          </q-item>
          <q-item>
            <q-item-section>
              <q-item-label caption>Số tiền</q-item-label>
              <q-item-label class="text-h6 text-positive">{{ formatCurrency(amount || 0) }}</q-item-label>
            </q-item-section>
          </q-item>
          <q-item>
            <q-item-section>
              <q-item-label caption>Nội dung</q-item-label>
              <q-item-label>{{ description || 'Không có nội dung' }}</q-item-label>
            </q-item-section>
          </q-item>
          <q-item>
            <q-item-section>
              <q-item-label caption>Phí chuyển tiền</q-item-label>
              <q-item-label>0 ₫</q-item-label>
            </q-item-section>
          </q-item>
        </q-list>

        <q-stepper-navigation>
          <q-btn @click="submitTransfer" color="primary" label="Xác nhận chuyển tiền" :loading="isSubmitting" />
          <q-btn flat @click="step = 1" color="primary" label="Quay lại" class="q-ml-sm" :disable="isSubmitting" />
        </q-stepper-navigation>
      </q-step>

      <!-- STEP 3: SUCCESS -->
      <q-step
        :name="3"
        title="Kết quả"
        icon="check_circle"
      >
        <div class="text-center q-pa-md">
          <q-icon name="check_circle" color="positive" size="4rem" />
          <div class="text-h5 text-positive q-mt-md">Chuyển tiền thành công</div>
          <div class="text-subtitle1 text-grey-7 q-mb-lg">Giao dịch đã được xử lý</div>
        </div>

        <q-card flat bordered class="q-mb-md">
          <q-card-section>
            <div class="row q-mb-sm">
              <div class="col-5 text-grey-7">Mã giao dịch</div>
              <div class="col-7 text-right text-weight-bold">
                {{ receipt?.reference }}
                <q-btn flat dense round icon="content_copy" size="xs" color="grey-7" @click="copyToClipboard(receipt?.reference || '')" />
              </div>
            </div>
            <div class="row q-mb-sm">
              <div class="col-5 text-grey-7">Tài khoản nguồn</div>
              <div class="col-7 text-right">{{ receipt?.sourceAccountNumber }}</div>
            </div>
            <div class="row q-mb-sm">
              <div class="col-5 text-grey-7">Tài khoản nhận</div>
              <div class="col-7 text-right">{{ receipt?.destinationAccountNumber }}<br/><span class="text-primary">{{ receipt?.destinationAccountName }}</span></div>
            </div>
            <div class="row q-mb-sm">
              <div class="col-5 text-grey-7">Số tiền</div>
              <div class="col-7 text-right text-positive text-weight-bold">{{ formatCurrency(receipt?.amount || 0) }}</div>
            </div>
            <div class="row q-mb-sm">
              <div class="col-5 text-grey-7">Nội dung</div>
              <div class="col-7 text-right">{{ receipt?.description }}</div>
            </div>
            <div class="row q-mb-sm">
              <div class="col-5 text-grey-7">Thời gian</div>
              <div class="col-7 text-right">{{ receipt?.completedAtUtc ? formatDateTime(receipt.completedAtUtc) : '' }}</div>
            </div>
          </q-card-section>
        </q-card>

        <q-checkbox v-if="!isBeneficiarySaved" v-model="saveBeneficiary" label="Lưu người nhận vào danh bạ" class="q-mb-md" />

        <q-stepper-navigation>
          <q-btn @click="startNewTransfer" color="primary" label="Chuyển khoản mới" outline />
          <q-btn to="/dashboard" color="primary" label="Về trang chủ" class="q-ml-sm" flat />
          <q-btn to="/transactions" color="primary" label="Lịch sử giao dịch" class="q-ml-sm" flat />
        </q-stepper-navigation>
      </q-step>
    </q-stepper>

    <!-- Beneficiaries Dialog -->
    <q-dialog v-model="showBeneficiaries">
      <q-card style="width: 400px; max-width: 90vw;">
        <q-card-section class="row items-center q-pb-none">
          <div class="text-h6">Danh bạ thụ hưởng</div>
          <q-space />
          <q-btn icon="close" flat round dense v-close-popup />
        </q-card-section>

        <q-card-section v-if="beneficiaries.length === 0" class="text-center text-grey-7 q-pa-lg">
          Bạn chưa lưu người nhận nào.
        </q-card-section>

        <q-list v-else separator>
          <q-item v-for="b in beneficiaries" :key="b.id" clickable v-ripple @click="selectBeneficiary(b)">
            <q-item-section avatar>
              <q-avatar color="primary" text-color="white" icon="person" />
            </q-item-section>
            <q-item-section>
              <q-item-label class="text-weight-bold">{{ b.nickname || b.accountName }}</q-item-label>
              <q-item-label caption>{{ b.accountNumber }} - {{ b.accountName }}</q-item-label>
            </q-item-section>
          </q-item>
        </q-list>
      </q-card>
    </q-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { useQuasar } from 'quasar'
import AppAlert from '~/components/common/AppAlert.vue'
import { formatCurrency } from '~/utils/currency'
import { formatDateTime } from '~/utils/date'
import type { AccountSummary, Beneficiary, TransferReceipt } from '~/types'
import { useAccountService } from '~/services/accountService'
import { useTransferService } from '~/services/transferService'
import { useBeneficiaryService } from '~/services/beneficiaryService'

definePageMeta({
  layout: 'default',
  middleware: ['customer']
})

const $q = useQuasar()
const route = useRoute()
const accountService = useAccountService()
const transferService = useTransferService()
const beneficiaryService = useBeneficiaryService()

const step = ref(1)
const form1 = ref<any>(null)
const errorMessage = ref('')

// Data
const accounts = ref<AccountSummary[]>([])
const beneficiaries = ref<Beneficiary[]>([])
const activeAccounts = computed(() => accounts.value.filter(a => a.status === 'ACTIVE'))

// Form Fields
const sourceAccountId = ref<string | null>(null)
const destinationAccountNumber = ref('')
const destinationName = ref('')
const amount = ref<number | null>(null)
const description = ref('')

// State
const isLookingUp = ref(false)
const lookupError = ref('')
const showBeneficiaries = ref(false)
const isSubmitting = ref(false)
const idempotencyKey = ref('')
const receipt = ref<TransferReceipt | null>(null)
const saveBeneficiary = ref(false)
const isBeneficiarySaved = ref(false)

const selectedSourceAccount = computed(() => {
  return accounts.value.find(a => a.id === sourceAccountId.value)
})

onMounted(async () => {
  await loadAccounts()
  await loadBeneficiaries()

  const querySource = route.query.sourceAccount as string
  if (querySource && activeAccounts.value.find(a => a.id === querySource)) {
    sourceAccountId.value = querySource
  }
})

const loadAccounts = async () => {
  try {
    accounts.value = await accountService.getAccounts()
  } catch (error) {
    errorMessage.value = 'Không thể tải danh sách tài khoản'
  }
}

const loadBeneficiaries = async () => {
  try {
    beneficiaries.value = await beneficiaryService.getBeneficiaries()
  } catch (error) {
    // Ignore error silently for beneficiaries
  }
}

const onDestinationChanged = () => {
  destinationName.value = ''
  lookupError.value = ''
}

const lookupDestination = async () => {
  if (!destinationAccountNumber.value) return
  
  isLookingUp.value = true
  lookupError.value = ''
  destinationName.value = ''

  if (selectedSourceAccount.value?.accountNumber === destinationAccountNumber.value) {
    lookupError.value = 'Không thể chuyển tiền đến chính tài khoản nguồn.'
    isLookingUp.value = false
    return
  }

  try {
    const res = await accountService.lookupAccount(destinationAccountNumber.value)
    destinationName.value = res.accountName
  } catch (error: any) {
    if (error.response?.status === 404) {
      lookupError.value = 'Không tìm thấy tài khoản người nhận.'
    } else {
      lookupError.value = 'Đã có lỗi xảy ra khi tra cứu.'
    }
  } finally {
    isLookingUp.value = false
  }
}

const selectBeneficiary = (b: Beneficiary) => {
  destinationAccountNumber.value = b.accountNumber
  destinationName.value = b.accountName
  showBeneficiaries.value = false
}

const onValidateStep1 = async () => {
  errorMessage.value = ''
  if (!destinationName.value) {
    errorMessage.value = 'Vui lòng kiểm tra tài khoản người nhận trước khi tiếp tục.'
    return
  }
  
  // Generate Idempotency Key
  idempotencyKey.value = crypto.randomUUID()
  step.value = 2
}

const submitTransfer = async () => {
  errorMessage.value = ''
  isSubmitting.value = true

  try {
    receipt.value = await transferService.createTransfer({
      sourceAccountId: sourceAccountId.value!,
      destinationAccountNumber: destinationAccountNumber.value,
      amount: amount.value!,
      description: description.value
    }, idempotencyKey.value)

    // Check if beneficiary is already saved
    isBeneficiarySaved.value = beneficiaries.value.some(b => b.accountNumber === destinationAccountNumber.value)
    
    // Refresh accounts balance in background
    loadAccounts()
    
    step.value = 3
  } catch (error: any) {
    // Parse problem details
    if (error.response?.status === 400 || error.response?.status === 404 || error.response?.status === 409) {
      const detail = error.response._data?.detail || ''
      if (detail.includes('Insufficient funds')) {
        errorMessage.value = 'Số dư không đủ để thực hiện giao dịch.'
      } else if (detail.includes('Source account is LOCKED')) {
        errorMessage.value = 'Tài khoản nguồn hiện không thể giao dịch.'
      } else if (detail.includes('Destination account is LOCKED')) {
        errorMessage.value = 'Tài khoản người nhận hiện không thể nhận tiền.'
      } else if (detail.includes('same bank account')) {
        errorMessage.value = 'Không thể chuyển tiền đến cùng tài khoản.'
      } else if (detail.includes('Idempotency key')) {
        errorMessage.value = 'Yêu cầu giao dịch bị xung đột. Vui lòng tạo giao dịch mới.'
      } else if (detail.includes('concurrency conflict')) {
        errorMessage.value = 'Số dư tài khoản vừa thay đổi. Vui lòng kiểm tra lại.'
      } else {
        errorMessage.value = 'Không thể thực hiện giao dịch. Vui lòng thử lại.'
      }
    } else if (error.response?.status === 429) {
      errorMessage.value = 'Bạn đang thao tác quá nhanh. Vui lòng thử lại sau.'
    } else if (error.message.includes('fetch failed') || error.message.includes('Network Error')) {
      errorMessage.value = 'Không thể xác định trạng thái giao dịch. Vui lòng thử lại với cùng yêu cầu hoặc kiểm tra lịch sử giao dịch.'
    } else {
      errorMessage.value = 'Đã có lỗi xảy ra. Vui lòng thử lại sau.'
    }
  } finally {
    isSubmitting.value = false
  }
}

const startNewTransfer = async () => {
  if (saveBeneficiary.value && !isBeneficiarySaved.value) {
    try {
      await beneficiaryService.addBeneficiary({
        accountNumber: destinationAccountNumber.value,
        nickname: destinationName.value
      })
      await loadBeneficiaries()
    } catch (e) {
      // Ignore if beneficiary save fails
    }
  }

  destinationAccountNumber.value = ''
  destinationName.value = ''
  amount.value = null
  description.value = ''
  saveBeneficiary.value = false
  receipt.value = null
  idempotencyKey.value = ''
  step.value = 1
}

const copyToClipboard = async (text: string) => {
  try {
    await navigator.clipboard.writeText(text)
    $q.notify({
      message: 'Đã sao chép vào khay nhớ tạm',
      color: 'positive',
      position: 'bottom'
    })
  } catch (err) {
    // Ignore
  }
}
</script>
