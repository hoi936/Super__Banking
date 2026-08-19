<template>
  <div class="deposit-page">
    <div class="page-header">
      <h1 class="page-title">Nạp tiền vào tài khoản</h1>
      <p class="page-subtitle">Thực hiện nạp tiền từ các nguồn thẻ tín dụng, ví điện tử vào tài khoản LocalLink.</p>
    </div>

    <div class="row q-col-gutter-md">
      <div class="col-12 col-md-8">
        <q-card flat class="bank-card deposit-card">
          <q-card-section>
            <div class="text-subtitle1 text-weight-medium q-mb-md">Chi tiết giao dịch</div>
            
            <q-form @submit="onSubmit">
              <!-- Select Account -->
              <div class="form-group q-mb-md">
                <label class="form-label">Nạp vào tài khoản</label>
                <q-select
                  v-model="selectedAccountId"
                  :options="activeAccounts"
                  option-value="id"
                  option-label="accountName"
                  emit-value
                  map-options
                  outlined
                  dense
                  :loading="isLoadingAccounts"
                  :rules="[val => !!val || 'Vui lòng chọn tài khoản']"
                  popup-content-class="text-dark bg-white shadow-2"
                >
                  <template v-slot:selected-item="scope">
                    <div class="row items-center" v-if="scope.opt">
                      <div class="text-weight-medium">{{ scope.opt.accountNumber }}</div>
                      <div class="q-ml-sm text-grey-8">- {{ scope.opt.accountType }}</div>
                    </div>
                  </template>
                  <template v-slot:option="scope">
                    <q-item v-bind="scope.itemProps" class="text-dark bg-white">
                      <q-item-section>
                        <q-item-label>{{ scope.opt.accountNumber }} - {{ scope.opt.accountName }}</q-item-label>
                        <q-item-label caption class="text-grey-7">{{ scope.opt.accountType }} - Số dư: {{ formatCurrency(scope.opt.balance) }}</q-item-label>
                      </q-item-section>
                    </q-item>
                  </template>
                </q-select>
              </div>

              <!-- Amount -->
              <div class="form-group q-mb-md">
                <label class="form-label">Số tiền cần nạp (VND)</label>
                <q-input
                  v-model="amount"
                  outlined
                  dense
                  type="number"
                  placeholder="0"
                  :rules="[
                    val => !!val || 'Vui lòng nhập số tiền',
                    val => Number(val) >= 50000 || 'Số tiền tối thiểu là 50.000 VND',
                    val => Number(val) <= 1000000000 || 'Số tiền tối đa là 1.000.000.000 VND'
                  ]"
                />
              </div>

              <!-- Provider -->
              <div class="form-group q-mb-xl">
                <label class="form-label">Chọn phương thức thanh toán</label>
                <div class="row q-col-gutter-md q-mt-sm">
                  <div class="col-4" v-for="provider in providers" :key="provider.id">
                    <q-card 
                      flat 
                      bordered 
                      class="provider-card cursor-pointer"
                      :class="{'provider-selected': selectedProvider === provider.id}"
                      @click="selectedProvider = provider.id"
                    >
                      <q-card-section class="text-center column items-center justify-center">
                        <q-icon :name="provider.icon" size="32px" :color="provider.color" />
                        <div class="q-mt-sm text-weight-medium">{{ provider.name }}</div>
                      </q-card-section>
                    </q-card>
                  </div>
                </div>
                <div v-if="!selectedProvider" class="text-negative text-caption q-mt-sm">Vui lòng chọn phương thức thanh toán.</div>
              </div>

              <div class="flex justify-end">
                <q-btn
                  unelevated
                  color="primary"
                  type="submit"
                  label="Xác nhận nạp tiền"
                  class="bank-action-btn"
                  :loading="isSubmitting"
                  :disable="!isValid"
                />
              </div>
            </q-form>
          </q-card-section>
        </q-card>
      </div>

      <div class="col-12 col-md-4">
        <q-card flat class="bank-card info-card">
          <q-card-section>
            <div class="row items-center q-mb-md">
              <q-icon name="info" size="24px" color="primary" />
              <div class="text-subtitle1 text-weight-bold q-ml-sm">Thông tin Nạp tiền</div>
            </div>
            
            <q-list class="info-list">
              <q-item>
                <q-item-section>
                  <q-item-label class="text-grey-8">Hạn mức tối thiểu</q-item-label>
                  <q-item-label class="text-weight-bold">50.000 VND</q-item-label>
                </q-item-section>
              </q-item>
              
              <q-item>
                <q-item-section>
                  <q-item-label class="text-grey-8">Hạn mức tối đa</q-item-label>
                  <q-item-label class="text-weight-bold">1.000.000.000 VND</q-item-label>
                </q-item-section>
              </q-item>
              
              <q-item>
                <q-item-section>
                  <q-item-label class="text-grey-8">Phí giao dịch</q-item-label>
                  <q-item-label class="text-weight-bold text-positive">Miễn phí</q-item-label>
                </q-item-section>
              </q-item>
              
              <q-item>
                <q-item-section>
                  <q-item-label class="text-grey-8">Thời gian xử lý</q-item-label>
                  <q-item-label class="text-weight-bold">Tức thì</q-item-label>
                </q-item-section>
              </q-item>
            </q-list>
          </q-card-section>
        </q-card>
      </div>
    </div>
    
    <!-- Giả lập Cổng thanh toán (Demo Modal) -->
    <q-dialog v-model="showGatewayMock" persistent>
      <q-card class="gateway-mock-card">
        <q-card-section class="bg-primary text-white row items-center">
          <div class="text-h6">Cổng thanh toán {{ receipt?.provider }}</div>
          <q-space />
          <q-btn icon="close" flat round dense v-close-popup @click="cancelPayment" />
        </q-card-section>

        <q-card-section class="q-pt-md">
          <div class="text-center q-mb-lg">
            <q-icon :name="getProviderIcon(receipt?.provider || '')" size="48px" :color="getProviderColor(receipt?.provider || '')" />
            <div class="text-h5 q-mt-sm">{{ formatCurrency(receipt?.amount || 0) }}</div>
            <div class="text-grey-7">Ref: {{ receipt?.referenceNumber }}</div>
          </div>
          
          <AppAlert type="info" message="Đây là môi trường thử nghiệm. Bạn có thể nhấn 'Thanh toán thành công' để giả lập quá trình nạp tiền vào tài khoản." />
        </q-card-section>

        <q-card-actions align="center" class="q-pb-md">
          <q-btn flat label="Hủy giao dịch" color="negative" @click="simulateCallback('Failed')" :loading="isSimulating" />
          <q-btn unelevated color="positive" label="Thanh toán thành công" @click="simulateCallback('Success')" :loading="isSimulating" />
        </q-card-actions>
      </q-card>
    </q-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useQuasar } from 'quasar'
import { useRouter } from 'vue-router'
import { useAccountService } from '~/services/accountService'
import { usePaymentGatewayService } from '~/services/paymentGatewayService'
import type { AccountSummary } from '~/types/account'
import type { GatewayDepositReceiptDto } from '~/types/paymentGateway'
import { useApiClient } from '~/services/api' // Cho việc gọi callback giả lập

definePageMeta({
  layout: 'default',
  middleware: ['auth']
})

const $q = useQuasar()
const router = useRouter()
const accountService = useAccountService()
const gatewayService = usePaymentGatewayService()
const { baseUrl } = useApiClient()

const accounts = ref<AccountSummary[]>([])
const isLoadingAccounts = ref(false)
const selectedAccountId = ref<string>('')
const amount = ref<number | null>(null)
const selectedProvider = ref<string>('')

const isSubmitting = ref(false)
const showGatewayMock = ref(false)
const receipt = ref<GatewayDepositReceiptDto | null>(null)
const isSimulating = ref(false)

const providers = [
  { id: 'VNPay', name: 'VNPay', icon: 'account_balance', color: 'blue' },
  { id: 'Momo', name: 'Momo', icon: 'account_balance_wallet', color: 'pink' },
  { id: 'Stripe', name: 'Thẻ Visa/Master', icon: 'credit_card', color: 'indigo' }
]

const activeAccounts = computed(() => accounts.value.filter(a => a.status === 'ACTIVE' && a.currency === 'VND'))
const isValid = computed(() => !!selectedAccountId.value && !!amount.value && amount.value >= 50000 && !!selectedProvider.value)

const formatCurrency = (val: number) => {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val)
}

const getProviderIcon = (providerId: string) => {
  return providers.find(p => p.id === providerId)?.icon || 'payment'
}

const getProviderColor = (providerId: string) => {
  return providers.find(p => p.id === providerId)?.color || 'grey'
}

const fetchAccounts = async () => {
  isLoadingAccounts.value = true
  try {
    accounts.value = await accountService.getAccounts()
    if (activeAccounts.value.length > 0) {
      selectedAccountId.value = activeAccounts.value[0].id
    }
  } catch (error) {
    $q.notify({ type: 'negative', message: 'Không thể tải danh sách tài khoản.' })
  } finally {
    isLoadingAccounts.value = false
  }
}

const onSubmit = async () => {
  if (!isValid.value) return
  
  isSubmitting.value = true
  try {
    receipt.value = await gatewayService.createDeposit({
      accountId: selectedAccountId.value,
      amount: Number(amount.value),
      provider: selectedProvider.value
    })
    
    // Mở Modal giả lập
    showGatewayMock.value = true
  } catch (err: any) {
    const msg = err.data?.title || 'Đã có lỗi xảy ra khi tạo giao dịch nạp tiền.'
    $q.notify({ type: 'negative', message: msg })
  } finally {
    isSubmitting.value = false
  }
}

const cancelPayment = () => {
  // Người dùng đóng form giả lập
  receipt.value = null
  selectedProvider.value = ''
}

const simulateCallback = async (status: string) => {
  if (!receipt.value) return
  
  isSimulating.value = true
  try {
    // Gọi thẳng lên API callback backend để giả lập cổng thanh toán gọi Webhook
    await $fetch(`${baseUrl}/api/v1/payment-gateway/callback`, {
      method: 'POST',
      body: {
        referenceNumber: receipt.value.referenceNumber,
        provider: receipt.value.provider,
        status: status,
        gatewayTransactionId: `GW-${Date.now()}`
      }
    })
    
    showGatewayMock.value = false
    
    if (status === 'Success') {
      $q.notify({ type: 'positive', message: 'Nạp tiền thành công! Số dư của bạn đã được cập nhật.' })
      // Reset form
      amount.value = null
      selectedProvider.value = ''
      await fetchAccounts() // reload balances
    } else {
      $q.notify({ type: 'negative', message: 'Giao dịch nạp tiền đã bị hủy hoặc thất bại.' })
    }
  } catch (error) {
    $q.notify({ type: 'negative', message: 'Không thể xử lý callback từ hệ thống.' })
  } finally {
    isSimulating.value = false
  }
}

onMounted(() => {
  fetchAccounts()
})
</script>

<style scoped>
.deposit-card {
  height: 100%;
}
.info-card {
  height: 100%;
  background: linear-gradient(145deg, #ffffff, #f8f9fa);
}
.provider-card {
  transition: all 0.2s ease;
  border: 2px solid transparent;
}
.provider-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0,0,0,0.05);
}
.provider-selected {
  border-color: var(--q-primary);
  background-color: #f0f7ff;
}
.gateway-mock-card {
  min-width: 400px;
  border-radius: 12px;
}
</style>
