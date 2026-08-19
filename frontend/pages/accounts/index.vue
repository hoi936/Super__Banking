<template>
  <div class="bank-page">
    <div class="bank-page-header">
      <div>
        <div class="bank-page-kicker">Tài khoản ngân hàng</div>
        <div class="bank-page-title">Danh sách tài khoản</div>
        <div class="bank-page-subtitle">Quản lý số dư, trạng thái và truy cập nhanh vào từng tài khoản của bạn.</div>
      </div>
      <q-btn unelevated color="primary" icon="refresh" label="Cập nhật" class="bank-action-btn" @click="fetchAccounts" :loading="isLoading" />
    </div>

    <AppAlert v-if="hasError" type="error" :message="errorMessage" class="q-mb-md" />

    <div v-if="isLoading" class="row q-col-gutter-lg">
      <div class="col-12 col-md-6" v-for="i in 4" :key="i">
        <q-card flat class="bank-card">
          <q-card-section>
            <q-skeleton type="rect" height="150px" />
          </q-card-section>
        </q-card>
      </div>
    </div>

    <div v-else-if="accounts.length === 0" class="text-center q-py-xl">
      <q-icon name="account_balance_wallet" size="64px" color="grey-4" />
      <div class="text-h6 text-grey-6 q-mt-md">Chưa có tài khoản nào</div>
    </div>

    <div v-else class="row q-col-gutter-lg">
      <div class="col-12 col-md-6" v-for="account in accounts" :key="account.id">
        <q-card flat class="bank-card cursor-pointer account-card full-height" @click="goToAccount(account.id)">
          <q-card-section class="q-pa-lg">
            <div class="row items-center justify-between q-mb-md">
              <div class="account-title">{{ account.accountName }}</div>
              <q-badge :color="isActiveStatus(account.status) ? 'positive' : 'grey'" class="bank-chip">
                {{ account.status }}
              </q-badge>
            </div>
            
            <div class="account-number font-mono">
              {{ formatAccountNumber(account.accountNumber) }}
            </div>
            
            <div class="row items-end justify-between">
              <div>
                <div class="account-label">Số dư khả dụng</div>
                <div class="account-balance">
                  {{ showBalance ? formatCurrency(account.balance, account.currency) : '******' }}
                </div>
              </div>
              <q-btn flat round dense :icon="showBalance ? 'visibility' : 'visibility_off'" 
                     color="grey-6" @click.stop="showBalance = !showBalance" />
            </div>
          </q-card-section>
          
          <q-separator />
          
          <q-card-actions align="right" class="q-px-md q-py-sm">
            <q-btn flat color="primary" label="Chi tiết" icon-right="chevron_right" class="text-weight-bold" />
          </q-card-actions>
        </q-card>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAccountService } from '~/services/accountService'
import { formatCurrency } from '~/utils/currency'
import type { AccountSummary } from '~/types/account'
import AppAlert from '~/components/common/AppAlert.vue'

definePageMeta({
  middleware: ['customer']
})

const router = useRouter()
const accountService = useAccountService()

const accounts = ref<AccountSummary[]>([])
const isLoading = ref(true)
const hasError = ref(false)
const errorMessage = ref('')
const showBalance = ref(false)

const fetchAccounts = async () => {
  isLoading.value = true
  hasError.value = false
  errorMessage.value = ''
  
  try {
    const data = await accountService.getAccounts()
    accounts.value = data
  } catch (e: any) {
    hasError.value = true
    errorMessage.value = e?.message || 'Lỗi tải danh sách tài khoản.'
  } finally {
    isLoading.value = false
  }
}

onMounted(() => {
  fetchAccounts()
})

const goToAccount = (id: string) => {
  router.push(`/accounts/${id}`)
}

const isActiveStatus = (status: string) => status === 'ACTIVE' || status === 'Active'

const formatAccountNumber = (number: string) => {
  // Format as XXXX XXXX 0001
  if (number.length >= 4) {
    return number.replace(/(.{4})/g, '$1 ').trim()
  }
  return number
}
</script>

<style scoped>
.font-mono {
  letter-spacing: 1px;
}
.account-card {
  transition: border-color 0.16s ease, transform 0.16s ease, box-shadow 0.16s ease;
}
.account-card:hover {
  border-color: #9dc3ff;
  box-shadow: 0 16px 34px rgba(16, 24, 40, 0.08) !important;
  transform: translateY(-2px);
}
.account-title {
  color: #1d2939;
  font-size: 18px;
  font-weight: 850;
}
.account-number {
  color: #667085;
  font-size: 14px;
  margin-bottom: 22px;
}
.account-label {
  color: #667085;
  font-size: 12px;
  font-weight: 800;
  text-transform: uppercase;
}
.account-balance {
  color: #1f6fd1;
  font-size: 28px;
  font-weight: 850;
  margin-top: 4px;
}
</style>
