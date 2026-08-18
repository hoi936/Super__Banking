<template>
  <div class="q-pa-md">
    <!-- Header Greeting -->
    <div class="row items-center q-mb-lg">
      <div class="col">
        <div class="text-h5 text-weight-bold text-primary">
          Xin chào, <span v-if="isLoading"><q-skeleton type="text" width="150px" inline /></span>
          <span v-else>{{ profile?.fullName || authStore.user?.fullName }}</span>
        </div>
        <div class="text-subtitle1 text-grey-7">Tổng quan tài chính của bạn</div>
      </div>
    </div>

    <AppAlert v-if="hasError" type="error" :message="errorMessage" class="q-mb-md" />

    <div class="row q-col-gutter-md">
      <!-- Left Column (Balance, Accounts, Quick Actions) -->
      <div class="col-12 col-md-8">
        
        <!-- Total Balance Card -->
        <q-card flat bordered class="q-mb-md bg-primary text-white rounded-borders">
          <q-card-section>
            <div class="text-subtitle2 text-blue-2">Tổng số dư</div>
            <div class="text-h3 text-weight-bold q-my-sm">
              <span v-if="isLoading"><q-skeleton type="text" width="200px" dark /></span>
              <span v-else>
                {{ showBalance ? formatCurrency(totalBalance) : '******' }}
              </span>
              <q-btn flat round dense :icon="showBalance ? 'visibility' : 'visibility_off'" 
                     class="q-ml-sm" color="white" @click="showBalance = !showBalance" />
            </div>
          </q-card-section>
        </q-card>

        <!-- Quick Actions -->
        <div class="row q-col-gutter-sm q-mb-md">
          <div class="col-4">
            <q-btn outline color="primary" class="full-width q-py-sm bg-white" to="/transfer">
              <q-icon name="send" class="q-mb-xs" size="sm" />
              <div class="text-caption">Chuyển tiền</div>
            </q-btn>
          </div>
          <div class="col-4">
            <q-btn outline color="primary" class="full-width q-py-sm bg-white" to="/bills">
              <q-icon name="receipt" class="q-mb-xs" size="sm" />
              <div class="text-caption">Thanh toán</div>
            </q-btn>
          </div>
          <div class="col-4">
            <q-btn outline color="primary" class="full-width q-py-sm bg-white" to="/transactions">
              <q-icon name="history" class="q-mb-xs" size="sm" />
              <div class="text-caption">Giao dịch</div>
            </q-btn>
          </div>
        </div>

        <!-- Accounts List -->
        <div class="text-h6 q-mb-sm text-grey-9">Tài khoản của bạn</div>
        
        <div v-if="isLoading" class="row q-col-gutter-md q-mb-lg">
          <div class="col-12 col-sm-6" v-for="i in 2" :key="i">
            <q-card flat bordered><q-card-section><q-skeleton type="rect" height="100px" /></q-card-section></q-card>
          </div>
        </div>
        
        <div v-else class="row q-col-gutter-md q-mb-lg">
          <div class="col-12 col-sm-6" v-for="account in accounts" :key="account.id">
            <q-card flat bordered class="cursor-pointer bg-white account-card" @click="goToAccount(account.id)">
              <q-card-section>
                <div class="row items-center justify-between q-mb-sm">
                  <div class="text-subtitle1 text-weight-bold">{{ account.accountName }}</div>
                  <q-badge :color="account.status === 'Active' ? 'positive' : 'grey'">{{ account.status }}</q-badge>
                </div>
                <div class="text-caption text-grey-7 q-mb-md font-mono">{{ account.accountNumber }}</div>
                <div class="text-h6 text-primary text-weight-bold">
                  {{ showBalance ? formatCurrency(account.balance, account.currency) : '******' }}
                </div>
              </q-card-section>
            </q-card>
          </div>
          <div v-if="accounts.length === 0" class="col-12 text-center text-grey q-py-md">
            Chưa có tài khoản nào.
          </div>
        </div>
      </div>

      <!-- Right Column (Transactions, Bills) -->
      <div class="col-12 col-md-4">
        
        <!-- Bills Summary -->
        <q-card flat bordered class="q-mb-md bg-white">
          <q-card-section class="row items-center justify-between q-pb-none">
            <div class="text-subtitle1 text-weight-bold">Hóa đơn chờ thanh toán</div>
            <q-badge color="orange" v-if="unreadBillsCount > 0">{{ unreadBillsCount }}</q-badge>
          </q-card-section>
          
          <q-card-section>
            <div v-if="isLoading">
              <q-skeleton type="text" width="100%" class="q-mb-sm" />
              <q-skeleton type="text" width="100%" />
            </div>
            <div v-else>
              <q-list v-if="bills.length > 0">
                <q-item v-for="bill in bills.slice(0, 3)" :key="bill.id" class="q-pa-none q-mb-sm">
                  <q-item-section>
                    <q-item-label>{{ bill.providerName }}</q-item-label>
                    <q-item-label caption>Hạn: {{ formatDate(bill.dueDate) }}</q-item-label>
                  </q-item-section>
                  <q-item-section side>
                    <div class="text-weight-bold text-orange">{{ formatCurrency(bill.amount) }}</div>
                  </q-item-section>
                </q-item>
              </q-list>
              <div v-else class="text-caption text-grey text-center q-py-sm">
                Không có hóa đơn nợ.
              </div>
              <div class="text-center q-mt-sm" v-if="bills.length > 0">
                <q-btn flat color="primary" label="Xem tất cả" to="/bills" size="sm" />
              </div>
            </div>
          </q-card-section>
        </q-card>

        <!-- Recent Transactions -->
        <q-card flat bordered class="bg-white">
          <q-card-section class="row items-center justify-between q-pb-none">
            <div class="text-subtitle1 text-weight-bold">Giao dịch gần đây</div>
            <q-btn flat color="primary" label="Tất cả" to="/transactions" size="sm" />
          </q-card-section>
          
          <q-card-section>
            <div v-if="isLoading">
              <q-item v-for="i in 3" :key="i" class="q-pa-none q-mb-md">
                <q-item-section avatar><q-skeleton type="QAvatar" /></q-item-section>
                <q-item-section>
                  <q-item-label><q-skeleton type="text" /></q-item-label>
                  <q-item-label caption><q-skeleton type="text" width="50%" /></q-item-label>
                </q-item-section>
              </q-item>
            </div>
            <div v-else>
              <q-list v-if="recentTransactions.length > 0">
                <q-item v-for="tx in recentTransactions" :key="tx.id" class="q-pa-none q-mb-md">
                  <q-item-section avatar>
                    <q-avatar :color="getTxIconColor(tx)" text-color="white" size="md">
                      <q-icon :name="getTxIcon(tx)" size="sm" />
                    </q-avatar>
                  </q-item-section>
                  <q-item-section>
                    <q-item-label class="text-weight-medium text-body2 ellipsis">
                      {{ tx.description || tx.transactionType }}
                    </q-item-label>
                    <q-item-label caption>{{ formatDateTime(tx.createdAtUtc) }}</q-item-label>
                  </q-item-section>
                  <q-item-section side>
                    <div :class="['text-weight-bold', getTxAmountColor(tx)]">
                      {{ getTxAmountPrefix(tx) }}{{ formatCurrency(tx.amount, tx.currency) }}
                    </div>
                  </q-item-section>
                </q-item>
              </q-list>
              <div v-else class="text-caption text-grey text-center q-py-md">
                Chưa có giao dịch nào.
              </div>
            </div>
          </q-card-section>
        </q-card>

      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '~/stores/auth'
import { useDashboard } from '~/composables/useDashboard'
import { formatCurrency } from '~/utils/currency'
import { formatDate, formatDateTime } from '~/utils/date'
import AppAlert from '~/components/common/AppAlert.vue'
import type { TransactionListItem } from '~/types/transaction'

definePageMeta({
  middleware: ['customer']
})

const authStore = useAuthStore()
const router = useRouter()
const { 
  profile, accounts, recentTransactions, bills, 
  totalBalance, unreadBillsCount, isLoading, hasError, errorMessage, fetchDashboardData 
} = useDashboard()

const showBalance = ref(false)

onMounted(() => {
  fetchDashboardData()
})

const goToAccount = (id: string) => {
  router.push(`/accounts/${id}`)
}

// Transaction UI helpers (Neutral since we don't have enough context in list to know exact direction yet)
// Usually you'd check if tx.sourceAccountId in my accountIds
const isIncoming = (tx: TransactionListItem) => {
  // If destination is one of my accounts, it's incoming
  return accounts.value.some(a => a.id === tx.destinationAccountId)
}

const getTxIcon = (tx: TransactionListItem) => {
  if (tx.transactionType === 'Payment' || tx.transactionType === 'BillPayment') return 'receipt'
  if (tx.transactionType === 'Transfer') {
    return isIncoming(tx) ? 'arrow_downward' : 'arrow_upward'
  }
  return 'sync_alt'
}

const getTxIconColor = (tx: TransactionListItem) => {
  if (tx.transactionType === 'Payment' || tx.transactionType === 'BillPayment') return 'orange'
  return isIncoming(tx) ? 'positive' : 'blue-8'
}

const getTxAmountColor = (tx: TransactionListItem) => {
  if (tx.transactionType === 'Payment' || tx.transactionType === 'BillPayment') return 'text-negative'
  return isIncoming(tx) ? 'text-positive' : 'text-negative'
}

const getTxAmountPrefix = (tx: TransactionListItem) => {
  if (tx.transactionType === 'Payment' || tx.transactionType === 'BillPayment') return '-'
  return isIncoming(tx) ? '+' : '-'
}
</script>

<style scoped>
.font-mono {
  font-family: monospace;
  letter-spacing: 1px;
}
.account-card:hover {
  border-color: var(--q-primary);
  transition: all 0.3s ease;
}
</style>
