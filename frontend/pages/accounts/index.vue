<template>
  <div class="q-pa-md max-width-1200">
    <div class="row items-center q-mb-lg">
      <div class="col">
        <div class="text-h5 text-weight-bold text-primary">Danh sách Tài khoản</div>
        <div class="text-subtitle1 text-grey-7">Quản lý tất cả tài khoản ngân hàng của bạn</div>
      </div>
      <div class="col-auto">
        <q-btn flat round dense icon="refresh" color="primary" @click="fetchAccounts" :loading="isLoading" />
      </div>
    </div>

    <AppAlert v-if="hasError" type="error" :message="errorMessage" class="q-mb-md" />

    <div v-if="isLoading" class="row q-col-gutter-lg">
      <div class="col-12 col-md-6" v-for="i in 4" :key="i">
        <q-card flat bordered>
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
        <q-card flat bordered class="cursor-pointer bg-white account-card full-height" @click="goToAccount(account.id)">
          <q-card-section class="q-pa-lg">
            <div class="row items-center justify-between q-mb-md">
              <div class="text-h6 text-weight-bold">{{ account.accountName }}</div>
              <q-badge :color="account.status === 'Active' ? 'positive' : 'grey'" class="text-subtitle2 q-px-sm q-py-xs">
                {{ account.status }}
              </q-badge>
            </div>
            
            <div class="text-subtitle1 text-grey-8 q-mb-lg font-mono tracking-wider">
              {{ formatAccountNumber(account.accountNumber) }}
            </div>
            
            <div class="row items-end justify-between">
              <div>
                <div class="text-caption text-grey">Số dư khả dụng</div>
                <div class="text-h4 text-primary text-weight-bold">
                  {{ showBalance ? formatCurrency(account.balance, account.currency) : '******' }}
                </div>
              </div>
              <q-btn flat round dense :icon="showBalance ? 'visibility' : 'visibility_off'" 
                     color="grey-6" @click.stop="showBalance = !showBalance" />
            </div>
          </q-card-section>
          
          <q-separator />
          
          <q-card-actions align="right" class="q-px-md q-py-sm bg-grey-1">
            <q-btn flat color="primary" label="Chi tiết" icon-right="chevron_right" />
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

const formatAccountNumber = (number: string) => {
  // Format as XXXX XXXX 0001
  if (number.length >= 4) {
    return number.replace(/(.{4})/g, '$1 ').trim()
  }
  return number
}
</script>

<style scoped>
.max-width-1200 {
  max-width: 1200px;
  margin: 0 auto;
}
.font-mono {
  font-family: monospace;
}
.tracking-wider {
  letter-spacing: 2px;
}
.account-card {
  transition: all 0.3s ease;
}
.account-card:hover {
  border-color: var(--q-primary);
  box-shadow: 0 4px 12px rgba(0,0,0,0.05);
  transform: translateY(-2px);
}
</style>
