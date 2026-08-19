<template>
  <div class="bank-page-narrow">
    <div class="bank-page-header">
      <div class="row items-center">
        <q-btn flat round dense icon="arrow_back" class="bank-soft-btn q-mr-sm" @click="goBack" />
        <div>
          <div class="bank-page-kicker">Dòng tiền cá nhân</div>
          <div class="bank-page-title">Chi tiết giao dịch</div>
          <div class="bank-page-subtitle">Thông tin tham chiếu, tài khoản liên quan và thời gian xử lý.</div>
        </div>
      </div>
    </div>

    <div v-if="isLoading" class="text-center q-pa-xl">
      <q-spinner color="primary" size="3em" />
    </div>

    <AppAlert v-else-if="error" :message="error" type="error" />

    <q-card v-else-if="transaction" flat class="bank-card">
      <q-card-section>
        <div class="bank-money-panel">
        <div class="text-subtitle1 text-grey-8">{{ transaction.transactionType }}</div>
        <div :class="['text-h4 text-weight-bold q-my-md', getAmountColor(transaction)]">
          {{ getAmountPrefix(transaction) }}{{ formatCurrency(transaction.amount) }}
        </div>
        <q-chip :color="getStatusColor(transaction.status)" text-color="white" class="bank-chip">
          {{ getStatusLabel(transaction.status) }}
        </q-chip>
        </div>
      </q-card-section>

      <q-card-section>
        <q-list separator class="bank-kv-list">
          <q-item>
            <q-item-section>
              <q-item-label caption>Mã tham chiếu</q-item-label>
              <q-item-label class="text-weight-medium">
                {{ transaction.referenceNumber }}
                <q-btn flat dense round icon="content_copy" size="xs" color="grey-7" @click="copyToClipboard(transaction.referenceNumber)" />
              </q-item-label>
            </q-item-section>
          </q-item>
          
          <q-item v-if="transaction.sourceAccountNumber">
            <q-item-section>
              <q-item-label caption>Tài khoản nguồn</q-item-label>
              <q-item-label class="text-weight-medium">{{ transaction.sourceAccountNumber }}</q-item-label>
              <q-item-label v-if="transaction.sourceAccountName" class="text-grey-8">{{ transaction.sourceAccountName }}</q-item-label>
            </q-item-section>
          </q-item>

          <q-item v-if="transaction.destinationAccountNumber">
            <q-item-section>
              <q-item-label caption>Tài khoản đích</q-item-label>
              <q-item-label class="text-weight-medium">{{ transaction.destinationAccountNumber }}</q-item-label>
              <q-item-label v-if="transaction.destinationAccountName" class="text-grey-8">{{ transaction.destinationAccountName }}</q-item-label>
            </q-item-section>
          </q-item>

          <q-item v-if="transaction.description">
            <q-item-section>
              <q-item-label caption>Nội dung</q-item-label>
              <q-item-label>{{ transaction.description }}</q-item-label>
            </q-item-section>
          </q-item>

          <q-item>
            <q-item-section>
              <q-item-label caption>Thời gian tạo</q-item-label>
              <q-item-label>{{ formatDateTime(transaction.createdAtUtc) }}</q-item-label>
            </q-item-section>
          </q-item>

          <q-item v-if="transaction.completedAtUtc">
            <q-item-section>
              <q-item-label caption>Thời gian hoàn tất</q-item-label>
              <q-item-label>{{ formatDateTime(transaction.completedAtUtc) }}</q-item-label>
            </q-item-section>
          </q-item>
        </q-list>
      </q-card-section>
    </q-card>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useQuasar } from 'quasar'
import AppAlert from '~/components/common/AppAlert.vue'
import { formatCurrency } from '~/utils/currency'
import { formatDateTime } from '~/utils/date'
import { getTransactionStatusColor as getStatusColor, getTransactionStatusLabel as getStatusLabel } from '~/utils/status'
import { useTransactionService } from '~/services/transactionService'
import { useAuthStore } from '~/stores/auth'
import type { TransactionDetail } from '~/types'

definePageMeta({
  layout: 'default',
  middleware: ['customer']
})

const route = useRoute()
const router = useRouter()
const $q = useQuasar()
const transactionService = useTransactionService()
const authStore = useAuthStore()

const transaction = ref<TransactionDetail | null>(null)
const isLoading = ref(true)
const error = ref('')

onMounted(async () => {
  const id = route.params.id as string
  if (!id) {
    error.value = 'Mã giao dịch không hợp lệ'
    isLoading.value = false
    return
  }

  try {
    transaction.value = await transactionService.getTransaction(id)
  } catch (err: any) {
    if (err.response?.status === 404) {
      error.value = 'Không tìm thấy giao dịch.'
    } else {
      error.value = 'Đã có lỗi xảy ra khi tải dữ liệu giao dịch.'
    }
  } finally {
    isLoading.value = false
  }
})

const goBack = () => {
  if (window.history.length > 2) {
    router.back()
  } else {
    router.push('/transactions')
  }
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

// Logic to determine color, same as index but simpler since we don't have accountId filter context
// We can use the current user's customerId to determine if they are sender or receiver if needed
// But since the API doesn't expose customerId on transaction easily, we just show generic
// or if the authStore has customer info, we could try to guess, but we don't have accounts loaded here.
// Let's just use generic logic for Transfer without filter context:
const getAmountColor = (tx: TransactionDetail) => {
  if (tx.transactionType === 'DEPOSIT') return 'text-positive'
  if (tx.transactionType === 'WITHDRAWAL' || tx.transactionType === 'PAYMENT') return 'text-negative'
  return 'text-primary' // For transfer, we just leave it primary if we don't know the context
}

const getAmountPrefix = (tx: TransactionDetail) => {
  if (tx.transactionType === 'DEPOSIT') return '+'
  if (tx.transactionType === 'WITHDRAWAL' || tx.transactionType === 'PAYMENT') return '-'
  return ''
}

</script>

<style scoped>
</style>
