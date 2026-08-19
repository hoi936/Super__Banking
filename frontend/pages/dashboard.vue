<template>
  <div class="bank-page">
    <div class="bank-page-header">
      <div>
        <div class="bank-page-kicker">Tổng quan tài chính</div>
        <div class="bank-page-title">
          Xin chào, <span v-if="isLoading"><q-skeleton type="text" width="160px" inline /></span>
          <span v-else>{{ profile?.fullName || authStore.user?.fullName }}</span>
        </div>
        <div class="bank-page-subtitle">Theo dõi số dư, tài khoản, hóa đơn và giao dịch gần đây trong một nơi.</div>
      </div>
      <q-btn unelevated color="primary" icon="refresh" label="Cập nhật" class="bank-action-btn" @click="fetchDashboardData" :loading="isLoading" />
    </div>

    <AppAlert v-if="hasError" type="error" :message="errorMessage" class="q-mb-md" />

    <div class="row q-col-gutter-md">
      <div class="col-12 col-lg-8">
        <q-card flat class="bank-card balance-card q-mb-md">
          <q-card-section>
            <div class="row items-start justify-between no-wrap">
              <div>
                <div class="balance-label">Tổng số dư khả dụng</div>
                <div class="balance-value">
                  <span v-if="isLoading"><q-skeleton type="text" width="220px" dark /></span>
                  <span v-else>{{ showBalance ? formatCurrency(totalBalance) : '******' }}</span>
                </div>
                <div class="balance-note">{{ accounts.length }} tài khoản đang liên kết</div>
              </div>
              <q-btn flat round :icon="showBalance ? 'visibility' : 'visibility_off'" color="white" @click="showBalance = !showBalance">
                <q-tooltip>{{ showBalance ? 'Ẩn số dư' : 'Hiện số dư' }}</q-tooltip>
              </q-btn>
            </div>
          </q-card-section>
        </q-card>

        <div class="row q-col-gutter-md q-mb-md">
          <div class="col-12 col-sm-4">
            <q-btn unelevated class="quick-action full-width" to="/transfer">
              <q-icon name="send" />
              <span>Chuyển tiền</span>
            </q-btn>
          </div>
          <div class="col-12 col-sm-4">
            <q-btn unelevated class="quick-action full-width" to="/bills">
              <q-icon name="receipt" />
              <span>Thanh toán hóa đơn</span>
            </q-btn>
          </div>
          <div class="col-12 col-sm-4">
            <q-btn unelevated class="quick-action full-width" to="/transactions">
              <q-icon name="history" />
              <span>Lịch sử giao dịch</span>
            </q-btn>
          </div>
        </div>

        <div class="bank-section-title">
          <q-icon name="account_balance_wallet" />
          <span>Tài khoản của bạn</span>
        </div>

        <div v-if="isLoading" class="row q-col-gutter-md">
          <div class="col-12 col-sm-6" v-for="i in 2" :key="i">
            <q-card flat class="bank-card"><q-card-section><q-skeleton type="rect" height="120px" /></q-card-section></q-card>
          </div>
        </div>

        <div v-else class="row q-col-gutter-md">
          <div class="col-12 col-sm-6" v-for="account in accounts" :key="account.id">
            <q-card flat class="bank-card account-tile cursor-pointer" @click="goToAccount(account.id)">
              <q-card-section>
                <div class="row items-center justify-between q-mb-sm">
                  <div class="account-name">{{ account.accountName }}</div>
                  <q-badge :color="isActiveStatus(account.status) ? 'positive' : 'grey'" class="bank-chip">{{ account.status }}</q-badge>
                </div>
                <div class="account-number font-mono">{{ account.accountNumber }}</div>
                <div class="account-balance">{{ showBalance ? formatCurrency(account.balance, account.currency) : '******' }}</div>
              </q-card-section>
            </q-card>
          </div>
          <div v-if="accounts.length === 0" class="col-12">
            <q-card flat class="bank-card empty-card">
              <q-card-section class="text-center">
                <q-icon name="account_balance_wallet" size="44px" color="grey-5" />
                <div class="text-weight-bold q-mt-sm">Chưa có tài khoản nào</div>
              </q-card-section>
            </q-card>
          </div>
        </div>
      </div>

      <div class="col-12 col-lg-4">
        <q-card flat class="bank-card q-mb-md">
          <q-card-section class="row items-center justify-between q-pb-sm">
            <div class="side-title">Hóa đơn chờ thanh toán</div>
            <q-badge color="orange" v-if="unreadBillsCount > 0" class="bank-chip">{{ unreadBillsCount }}</q-badge>
          </q-card-section>

          <q-card-section class="q-pt-sm">
            <div v-if="isLoading">
              <q-skeleton type="text" width="100%" class="q-mb-sm" />
              <q-skeleton type="text" width="80%" />
            </div>
            <q-list v-else-if="bills.length > 0" separator>
              <q-item v-for="bill in bills.slice(0, 3)" :key="bill.id" class="q-px-none">
                <q-item-section>
                  <q-item-label class="text-weight-bold">{{ bill.providerName }}</q-item-label>
                  <q-item-label caption>Hạn: {{ formatDate(bill.dueDate) }}</q-item-label>
                </q-item-section>
                <q-item-section side>
                  <div class="text-weight-bold text-orange-8">{{ formatCurrency(bill.amount) }}</div>
                </q-item-section>
              </q-item>
            </q-list>
            <div v-else class="empty-side">Không có hóa đơn nợ.</div>
            <q-btn v-if="bills.length > 0" flat color="primary" label="Xem tất cả" to="/bills" class="q-mt-sm full-width" />
          </q-card-section>
        </q-card>

        <q-card flat class="bank-card">
          <q-card-section class="row items-center justify-between q-pb-sm">
            <div class="side-title">Giao dịch gần đây</div>
            <q-btn flat color="primary" label="Tất cả" to="/transactions" size="sm" />
          </q-card-section>

          <q-card-section class="q-pt-sm">
            <div v-if="isLoading">
              <q-item v-for="i in 3" :key="i" class="q-pa-none q-mb-md">
                <q-item-section avatar><q-skeleton type="QAvatar" /></q-item-section>
                <q-item-section>
                  <q-item-label><q-skeleton type="text" /></q-item-label>
                  <q-item-label caption><q-skeleton type="text" width="50%" /></q-item-label>
                </q-item-section>
              </q-item>
            </div>
            <q-list v-else-if="recentTransactions.length > 0" separator>
              <q-item v-for="tx in recentTransactions" :key="tx.id" class="q-px-none">
                <q-item-section avatar>
                  <q-avatar :color="getTxIconColor(tx)" text-color="white" size="40px">
                    <q-icon :name="getTxIcon(tx)" size="sm" />
                  </q-avatar>
                </q-item-section>
                <q-item-section>
                  <q-item-label class="text-weight-bold text-body2 ellipsis">
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
            <div v-else class="empty-side">Chưa có giao dịch nào.</div>
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

const isActiveStatus = (status: string) => status === 'ACTIVE' || status === 'Active'

const isIncoming = (tx: TransactionListItem) => {
  return accounts.value.some(a => a.id === tx.destinationAccountId)
}

const getTxIcon = (tx: TransactionListItem) => {
  if (tx.transactionType === 'Payment' || tx.transactionType === 'BillPayment' || tx.transactionType === 'PAYMENT') return 'receipt'
  if (tx.transactionType === 'Transfer' || tx.transactionType === 'TRANSFER') {
    return isIncoming(tx) ? 'arrow_downward' : 'arrow_upward'
  }
  return 'sync_alt'
}

const getTxIconColor = (tx: TransactionListItem) => {
  if (tx.transactionType === 'Payment' || tx.transactionType === 'BillPayment' || tx.transactionType === 'PAYMENT') return 'orange'
  return isIncoming(tx) ? 'positive' : 'blue-8'
}

const getTxAmountColor = (tx: TransactionListItem) => {
  if (tx.transactionType === 'Payment' || tx.transactionType === 'BillPayment' || tx.transactionType === 'PAYMENT') return 'text-negative'
  return isIncoming(tx) ? 'text-positive' : 'text-negative'
}

const getTxAmountPrefix = (tx: TransactionListItem) => {
  if (tx.transactionType === 'Payment' || tx.transactionType === 'BillPayment' || tx.transactionType === 'PAYMENT') return '-'
  return isIncoming(tx) ? '+' : '-'
}
</script>

<style scoped>
.balance-card {
  background: linear-gradient(135deg, #1458a8 0%, #101828 100%);
  color: #ffffff;
  overflow: hidden;
}

.balance-card .q-card__section {
  padding: 26px;
}

.balance-label {
  color: #bfdbfe;
  font-size: 13px;
  font-weight: 800;
  text-transform: uppercase;
}

.balance-value {
  font-size: clamp(34px, 6vw, 52px);
  font-weight: 850;
  line-height: 1.08;
  margin-top: 10px;
}

.balance-note {
  color: #dbeafe;
  font-size: 14px;
  font-weight: 650;
  margin-top: 10px;
}

.quick-action {
  align-items: center;
  background: #ffffff !important;
  border: 1px solid #d9e2ef;
  border-radius: 8px;
  color: #1d2939 !important;
  display: flex;
  font-weight: 800;
  gap: 10px;
  justify-content: flex-start;
  min-height: 64px;
  padding: 0 18px;
}

.quick-action .q-icon {
  color: #1f6fd1;
  font-size: 24px;
}

.account-tile {
  transition: border-color 0.16s ease, transform 0.16s ease, box-shadow 0.16s ease;
}

.account-tile:hover {
  border-color: #9dc3ff;
  box-shadow: 0 16px 34px rgba(16, 24, 40, 0.08) !important;
  transform: translateY(-2px);
}

.account-name,
.side-title {
  color: #1d2939;
  font-size: 16px;
  font-weight: 850;
}

.account-number {
  color: #667085;
  font-size: 13px;
  margin-bottom: 18px;
}

.account-balance {
  color: #1f6fd1;
  font-size: 24px;
  font-weight: 850;
}

.empty-card,
.empty-side {
  color: #667085;
}

.empty-side {
  padding: 20px 0;
  text-align: center;
}
</style>
