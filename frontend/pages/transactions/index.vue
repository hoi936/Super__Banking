<template>
  <div class="q-pa-md">
    <div class="row items-center q-mb-md">
      <div class="text-h6 text-primary">Lịch sử giao dịch</div>
    </div>

    <!-- Filters -->
    <q-card flat bordered class="q-mb-md bg-grey-1">
      <q-card-section>
        <div class="row q-col-gutter-md">
          <div class="col-12 col-md-3">
            <q-select
              v-model="filters.accountId"
              :options="accounts"
              option-value="id"
              :option-label="opt => `${opt.accountNumber}`"
              emit-value
              map-options
              outlined
              dense
              label="Tài khoản"
              clearable
            />
          </div>
          <div class="col-12 col-md-3">
            <q-select
              v-model="filters.type"
              :options="[
                { label: 'Tất cả', value: '' },
                { label: 'Chuyển tiền', value: 'TRANSFER' },
                { label: 'Thanh toán', value: 'PAYMENT' },
                { label: 'Nạp tiền', value: 'DEPOSIT' },
                { label: 'Rút tiền', value: 'WITHDRAWAL' }
              ]"
              emit-value
              map-options
              outlined
              dense
              label="Loại giao dịch"
            />
          </div>
          <div class="col-12 col-md-2">
            <q-input v-model="filters.fromDate" outlined dense type="date" label="Từ ngày" />
          </div>
          <div class="col-12 col-md-2">
            <q-input v-model="filters.toDate" outlined dense type="date" label="Đến ngày" />
          </div>
          <div class="col-12 col-md-2 flex flex-center">
            <q-btn color="primary" label="Lọc" @click="applyFilters" class="full-width" />
          </div>
        </div>
        <div v-if="filterError" class="text-negative text-caption q-mt-sm">{{ filterError }}</div>
      </q-card-section>
    </q-card>

    <!-- Table for Desktop -->
    <q-table
      v-if="$q.screen.gt.sm"
      :rows="transactions"
      :columns="columns"
      row-key="id"
      :loading="isLoading"
      flat
      bordered
      :pagination="pagination"
      @request="onRequest"
      class="bg-white"
    >
      <template v-slot:body-cell-amount="props">
        <q-td :props="props" :class="getAmountColor(props.row)">
          {{ getAmountPrefix(props.row) }}{{ formatCurrency(props.row.amount) }}
        </q-td>
      </template>
      <template v-slot:body-cell-status="props">
        <q-td :props="props">
          <q-chip :color="getStatusColor(props.row.status)" text-color="white" size="sm" dense>
            {{ props.row.status }}
          </q-chip>
        </q-td>
      </template>
      <template v-slot:body-cell-action="props">
        <q-td :props="props">
          <q-btn flat round dense icon="visibility" color="primary" :to="`/transactions/${props.row.id}`" />
        </q-td>
      </template>
      <template v-slot:no-data>
        <div class="full-width row flex-center text-grey-7 q-pa-lg">
          Không tìm thấy giao dịch phù hợp.
        </div>
      </template>
    </q-table>

    <!-- List for Mobile -->
    <div v-else>
      <div v-if="isLoading" class="text-center q-pa-md">
        <q-spinner color="primary" size="2em" />
      </div>
      <div v-else-if="transactions.length === 0" class="text-center text-grey-7 q-pa-lg bg-white rounded-borders">
        Không tìm thấy giao dịch phù hợp.
      </div>
      <q-list v-else bordered separator class="bg-white rounded-borders">
        <q-item v-for="tx in transactions" :key="tx.id" clickable :to="`/transactions/${tx.id}`">
          <q-item-section>
            <q-item-label class="text-weight-bold">{{ tx.transactionType }}</q-item-label>
            <q-item-label caption>{{ formatDateTime(tx.createdAtUtc) }}</q-item-label>
            <q-item-label caption>{{ tx.referenceNumber }}</q-item-label>
          </q-item-section>
          <q-item-section side>
            <q-item-label :class="['text-weight-bold', getAmountColor(tx)]">
              {{ getAmountPrefix(tx) }}{{ formatCurrency(tx.amount) }}
            </q-item-label>
            <q-item-label>
              <q-chip :color="getStatusColor(tx.status)" text-color="white" size="xs" dense>
                {{ tx.status }}
              </q-chip>
            </q-item-label>
          </q-item-section>
        </q-item>
      </q-list>

      <div class="row justify-center q-mt-md" v-if="transactions.length > 0">
        <q-pagination
          v-model="pagination.page"
          :max="pagination.rowsNumber ? Math.ceil(pagination.rowsNumber / pagination.rowsPerPage) : 1"
          @update:model-value="onMobilePageChange"
          color="primary"
          boundary-links
          max-pages="5"
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { formatCurrency } from '~/utils/currency'
import { formatDateTime } from '~/utils/date'
import { useTransactionService } from '~/services/transactionService'
import { useAccountService } from '~/services/accountService'
import type { TransactionListItem, AccountSummary } from '~/types'

definePageMeta({
  layout: 'default',
  middleware: ['customer']
})

const route = useRoute()
const router = useRouter()
const transactionService = useTransactionService()
const accountService = useAccountService()

const accounts = ref<AccountSummary[]>([])
const transactions = ref<TransactionListItem[]>([])
const isLoading = ref(false)
const filterError = ref('')

const filters = ref({
  accountId: route.query.accountId as string || null,
  type: (route.query.type as string) || '',
  fromDate: (route.query.fromDate as string) || '',
  toDate: (route.query.toDate as string) || ''
})

const pagination = ref({
  page: 1,
  rowsPerPage: 10,
  rowsNumber: 0
})

const columns = [
  { name: 'reference', label: 'Mã GD', field: 'referenceNumber', align: 'left' as const },
  { name: 'type', label: 'Loại GD', field: 'transactionType', align: 'left' as const },
  { name: 'amount', label: 'Số tiền', field: 'amount', align: 'right' as const },
  { name: 'date', label: 'Thời gian', field: 'createdAtUtc', format: (val: string) => formatDateTime(val), align: 'center' as const },
  { name: 'status', label: 'Trạng thái', field: 'status', align: 'center' as const },
  { name: 'action', label: 'Chi tiết', field: 'id', align: 'center' as const }
]

onMounted(async () => {
  try {
    accounts.value = await accountService.getAccounts()
  } catch (error) {
    console.error('Failed to load accounts')
  }
  loadData()
})

const applyFilters = () => {
  filterError.value = ''
  if (filters.value.fromDate && filters.value.toDate) {
    if (new Date(filters.value.fromDate) > new Date(filters.value.toDate)) {
      filterError.value = 'Ngày bắt đầu không được lớn hơn ngày kết thúc.'
      return
    }
  }

  pagination.value.page = 1
  
  // Update URL query
  const query: any = {}
  if (filters.value.accountId) query.accountId = filters.value.accountId
  if (filters.value.type) query.type = filters.value.type
  if (filters.value.fromDate) query.fromDate = filters.value.fromDate
  if (filters.value.toDate) query.toDate = filters.value.toDate

  router.replace({ query })
  loadData()
}

const loadData = async (props?: any) => {
  const { page, rowsPerPage } = props?.pagination || pagination.value
  isLoading.value = true

  try {
    const res = await transactionService.getTransactions({
      page,
      pageSize: rowsPerPage,
      accountId: filters.value.accountId || undefined,
      type: filters.value.type || undefined,
      fromDate: filters.value.fromDate ? new Date(filters.value.fromDate).toISOString() : undefined,
      toDate: filters.value.toDate ? new Date(new Date(filters.value.toDate).setHours(23,59,59,999)).toISOString() : undefined
    })

    transactions.value = res.items
    pagination.value.page = res.page
    pagination.value.rowsPerPage = res.pageSize
    pagination.value.rowsNumber = res.totalItems
  } catch (error) {
    console.error(error)
  } finally {
    isLoading.value = false
  }
}

const onRequest = (props: any) => {
  loadData(props)
}

const onMobilePageChange = () => {
  loadData()
}

const getAmountColor = (tx: TransactionListItem) => {
  if (tx.transactionType === 'TRANSFER' && tx.sourceAccountId === filters.value.accountId) return 'text-negative'
  if (tx.transactionType === 'TRANSFER' && tx.destinationAccountId === filters.value.accountId) return 'text-positive'
  if (tx.transactionType === 'DEPOSIT') return 'text-positive'
  if (tx.transactionType === 'WITHDRAWAL' || tx.transactionType === 'PAYMENT') return 'text-negative'
  return 'text-primary'
}

const getAmountPrefix = (tx: TransactionListItem) => {
  if (tx.transactionType === 'TRANSFER' && tx.sourceAccountId === filters.value.accountId) return '-'
  if (tx.transactionType === 'TRANSFER' && tx.destinationAccountId === filters.value.accountId) return '+'
  if (tx.transactionType === 'DEPOSIT') return '+'
  if (tx.transactionType === 'WITHDRAWAL' || tx.transactionType === 'PAYMENT') return '-'
  return ''
}

const getStatusColor = (status: string) => {
  switch (status) {
    case 'COMPLETED': return 'positive'
    case 'PENDING': return 'warning'
    case 'FAILED': 
    case 'CANCELLED': return 'negative'
    default: return 'grey'
  }
}
</script>
