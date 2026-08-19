<template>
  <div class="bank-page">
    <div class="bank-page-header">
      <div class="row items-center">
        <q-btn flat round dense icon="arrow_back" class="bank-soft-btn q-mr-sm" to="/accounts" />
        <div>
          <div class="bank-page-kicker">Tài khoản ngân hàng</div>
          <div class="bank-page-title">Chi tiết tài khoản</div>
          <div class="bank-page-subtitle">Thông tin tài khoản, số dư khả dụng và thao tác nhanh.</div>
        </div>
      </div>
    </div>

    <AppAlert v-if="hasError" type="error" :message="errorMessage" class="q-mb-md" />

    <div v-if="isLoading">
      <q-card flat class="bank-card q-mb-lg">
        <q-card-section>
          <q-skeleton type="rect" height="200px" />
        </q-card-section>
      </q-card>
    </div>

    <div v-else-if="account">
      <!-- Account Details Card -->
      <q-card flat class="bank-card q-mb-lg">
        <q-card-section class="q-pa-lg">
          <div class="row items-center justify-between q-mb-lg">
            <div class="account-title">{{ account.accountName }}</div>
            <q-badge :color="isActiveStatus(account.status) ? 'positive' : 'grey'" class="bank-chip">
              {{ account.status }}
            </q-badge>
          </div>
          
          <div class="row q-col-gutter-lg">
            <div class="col-12 col-md-6">
              <q-list class="bank-kv-list">
                <q-item>
                  <q-item-section>
                    <q-item-label caption>Số tài khoản</q-item-label>
                    <q-item-label class="text-subtitle1 font-mono text-weight-medium">
                      {{ formatAccountNumber(account.accountNumber) }}
                    </q-item-label>
                  </q-item-section>
                  <q-item-section side>
                    <q-btn flat round dense icon="content_copy" size="sm" @click="copyToClipboard(account.accountNumber)" />
                  </q-item-section>
                </q-item>
                
                <q-separator spaced />
                
                <q-item>
                  <q-item-section>
                    <q-item-label caption>Loại tài khoản</q-item-label>
                    <q-item-label class="text-subtitle1 text-weight-medium">
                      {{ account.accountType }}
                    </q-item-label>
                  </q-item-section>
                </q-item>

                <q-separator spaced />
                
                <q-item>
                  <q-item-section>
                    <q-item-label caption>Ngày mở</q-item-label>
                    <q-item-label class="text-subtitle1 text-weight-medium">
                      {{ formatDate(account.createdAtUtc) }}
                    </q-item-label>
                  </q-item-section>
                </q-item>
              </q-list>
            </div>
            
            <div class="col-12 col-md-6 flex column justify-center">
              <div class="bank-money-panel full-height flex column flex-center">
                <div class="text-subtitle1 text-grey-8 q-mb-sm">Số dư khả dụng</div>
                <div class="detail-balance q-mb-md">
                  {{ showBalance ? formatCurrency(account.balance, account.currency) : '******' }}
                </div>
                <q-btn outline color="primary" class="bank-action-btn"
                       :icon="showBalance ? 'visibility_off' : 'visibility'" 
                       :label="showBalance ? 'Ẩn số dư' : 'Hiện số dư'"
                       @click="showBalance = !showBalance" />
              </div>
            </div>
          </div>
        </q-card-section>
        
        <q-separator />
        
        <q-card-actions class="q-pa-md" align="around">
          <q-btn flat color="primary" icon="send" label="Chuyển tiền" class="text-weight-bold"
                 :to="`/transfer?sourceAccount=${account.id}`" />
          <q-btn flat color="primary" icon="receipt" label="Thanh toán hóa đơn" class="text-weight-bold"
                 to="/bills" />
          <q-btn flat color="primary" icon="history" label="Lịch sử giao dịch" class="text-weight-bold"
                 :to="`/transactions?accountId=${account.id}`" />
        </q-card-actions>
      </q-card>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { useQuasar } from 'quasar'
import { useAccountService } from '~/services/accountService'
import { formatCurrency } from '~/utils/currency'
import { formatDate } from '~/utils/date'
import type { AccountDetail } from '~/types/account'
import AppAlert from '~/components/common/AppAlert.vue'

definePageMeta({
  middleware: ['customer']
})

const route = useRoute()
const $q = useQuasar()
const accountService = useAccountService()

const accountId = route.params.id as string
const account = ref<AccountDetail | null>(null)
const isLoading = ref(true)
const hasError = ref(false)
const errorMessage = ref('')
const showBalance = ref(false)

const fetchAccountDetail = async () => {
  if (!accountId) return
  
  isLoading.value = true
  hasError.value = false
  errorMessage.value = ''
  
  try {
    const data = await accountService.getAccount(accountId)
    account.value = data
  } catch (e: any) {
    hasError.value = true
    errorMessage.value = e?.message || 'Không thể tải chi tiết tài khoản. Hoặc tài khoản không tồn tại.'
  } finally {
    isLoading.value = false
  }
}

onMounted(() => {
  fetchAccountDetail()
})

const formatAccountNumber = (number: string) => {
  if (number.length >= 4) {
    return number.replace(/(.{4})/g, '$1 ').trim()
  }
  return number
}

const isActiveStatus = (status: string) => status === 'ACTIVE' || status === 'Active'

const copyToClipboard = async (text: string) => {
  try {
    await navigator.clipboard.writeText(text)
    $q.notify({
      color: 'positive',
      message: 'Đã copy số tài khoản',
      icon: 'check',
      position: 'top-right',
      timeout: 2000
    })
  } catch (err) {
    $q.notify({
      color: 'negative',
      message: 'Không thể copy',
      position: 'top-right'
    })
  }
}
</script>

<style scoped>
.font-mono {
  letter-spacing: 1px;
}
.account-title {
  color: #1d2939;
  font-size: 24px;
  font-weight: 850;
}
.detail-balance {
  color: #1f6fd1;
  font-size: clamp(30px, 5vw, 42px);
  font-weight: 850;
  line-height: 1.12;
}
</style>
