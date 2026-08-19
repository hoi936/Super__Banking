<template>
  <div class="admin-page">
    <div class="admin-page-header">
      <div>
        <div class="admin-page-kicker">Giám sát dòng tiền</div>
        <div class="admin-page-title">Lịch sử giao dịch</div>
        <div class="admin-page-subtitle">Tra cứu mã tham chiếu, tài khoản nguồn/đích, trạng thái và khối lượng tiền trên toàn hệ thống.</div>
      </div>
    </div>

    <q-card flat class="admin-filter-card q-mb-lg">
      <q-card-section class="q-pa-lg">
        <div class="row q-col-gutter-md items-center">
          <div class="col-12 col-sm-6 col-md-3">
            <div class="admin-field-label">Mã tham chiếu</div>
            <q-input 
              v-model="filters.search" 
              outlined 
              dense 
              placeholder="Nhập mã tham chiếu..." 
              bg-color="grey-1"
              clearable
              @keyup.enter="fetchTransactions"
            >
              <template v-slot:prepend>
                <q-icon name="search" color="primary" />
              </template>
            </q-input>
          </div>
          <div class="col-12 col-sm-6 col-md-2">
            <div class="admin-field-label">Loại giao dịch</div>
            <q-select
              popup-content-class="text-dark bg-white shadow-2"
              options-selected-class="text-primary text-weight-bold"
              v-model="filters.type"
              outlined
              dense
              :options="typeOptions"
              bg-color="grey-1"
              clearable
              emit-value
              map-options
              @update:model-value="fetchTransactions"
            />
          </div>
          <div class="col-12 col-sm-4 col-md-2">
            <div class="admin-field-label">Trạng thái</div>
            <q-select
              popup-content-class="text-dark bg-white shadow-2"
              options-selected-class="text-primary text-weight-bold"
              v-model="filters.status"
              outlined
              dense
              :options="statusOptions"
              bg-color="grey-1"
              clearable
              emit-value
              map-options
              @update:model-value="fetchTransactions"
            />
          </div>
          <div class="col-12 col-sm-4 col-md-2">
            <div class="admin-field-label">Từ ngày</div>
            <q-input outlined dense v-model="filters.fromDate" type="date" bg-color="grey-1" clearable @change="fetchTransactions" />
          </div>
          <div class="col-12 col-sm-4 col-md-2">
            <div class="admin-field-label">Đến ngày</div>
            <q-input outlined dense v-model="filters.toDate" type="date" bg-color="grey-1" clearable @change="fetchTransactions" />
          </div>
          <div class="col-12 col-md-1 flex items-center justify-end" style="margin-top: 32px">
             <q-btn unelevated color="primary" icon="refresh" @click="fetchTransactions" class="admin-action-btn full-width" padding="8px" />
          </div>
        </div>
        
        <!-- Validation Error -->
        <div v-if="dateValidationError" class="text-negative q-mt-sm row items-center">
          <q-icon name="error" class="q-mr-xs" /> Ngày bắt đầu không được lớn hơn ngày kết thúc.
        </div>
      </q-card-section>
    </q-card>

    <q-card flat class="admin-card admin-table-card">
      <q-table
        class="premium-table"
        :rows="transactions"
        :columns="columns"
        row-key="id"
        :loading="isLoading"
        v-model:pagination="pagination"
        @request="onRequest"
        flat
        :rows-per-page-options="[10, 20, 50]"
      >
        <template v-slot:body-cell-referenceNumber="props">
          <q-td :props="props">
            <div class="row items-center no-wrap">
              <q-avatar size="32px" :color="getTypeColor(props.row.transactionType) + '-1'" :text-color="getTypeColor(props.row.transactionType)" class="q-mr-sm">
                <q-icon :name="getTypeIcon(props.row.transactionType)" size="xs" />
              </q-avatar>
              <span class="text-weight-bold text-dark">{{ props.row.referenceNumber }}</span>
            </div>
          </q-td>
        </template>

        <template v-slot:body-cell-sourceAccountNumber="props">
          <q-td :props="props" class="text-grey-8 font-monospace">
            {{ props.row.sourceAccountNumber || '--' }}
          </q-td>
        </template>

        <template v-slot:body-cell-destinationAccountNumber="props">
          <q-td :props="props" class="text-grey-8 font-monospace">
            {{ props.row.destinationAccountNumber || '--' }}
          </q-td>
        </template>

        <template v-slot:body-cell-amount="props">
          <q-td :props="props" class="text-weight-bold">
            <span :class="props.row.transactionType === 'FEE' || props.row.transactionType === 'WITHDRAWAL' ? 'text-negative' : 'text-positive'">
              {{ props.row.transactionType === 'FEE' || props.row.transactionType === 'WITHDRAWAL' ? '-' : '+' }}
              {{ formatCurrency(props.row.amount, props.row.currency) }}
            </span>
          </q-td>
        </template>

        <template v-slot:body-cell-transactionType="props">
          <q-td :props="props">
            <span :class="`text-${getTypeColor(props.row.transactionType)} text-weight-medium`">
              {{ getTypeLabel(props.row.transactionType) }}
            </span>
          </q-td>
        </template>

        <template v-slot:body-cell-status="props">
          <q-td :props="props">
            <q-chip 
              :color="getStatusColor(props.row.status) + '-1'" 
              :text-color="getStatusColor(props.row.status)"
              size="sm"
              class="admin-chip"
            >
              {{ getStatusLabel(props.row.status) }}
            </q-chip>
          </q-td>
        </template>
        
        <template v-slot:body-cell-createdAtUtc="props">
          <q-td :props="props" class="text-grey-7 text-caption text-weight-medium">
            {{ formatDateTime(props.row.createdAtUtc) }}
          </q-td>
        </template>

        <template v-slot:body-cell-actions="props">
          <q-td :props="props" class="text-right">
            <q-btn
              unelevated
              round
              color="primary"
              icon="chevron_right"
              size="sm"
              class="bg-blue-1 text-primary"
              :to="`/admin/transactions/${props.row.id}`"
            >
              <q-tooltip class="bg-dark">Xem chi tiết</q-tooltip>
            </q-btn>
          </q-td>
        </template>

        <template v-slot:no-data>
          <div class="full-width row flex-center text-grey-5 q-pa-xl">
            <div class="text-center">
              <q-icon size="4em" name="receipt_long" class="q-mb-md opacity-30" />
              <div class="text-h6 text-weight-medium">Không có dữ liệu giao dịch</div>
              <div class="text-caption">Chưa có giao dịch nào thỏa mãn điều kiện tìm kiếm</div>
            </div>
          </div>
        </template>
      </q-table>
    </q-card>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAdminTransactionService } from '~/services/adminTransactionService'
import type { AdminTransactionListItemDto } from '~/types/adminTransaction'
import { formatCurrency } from '~/utils/currency'
import { formatDateTime } from '~/utils/date'
import { getTransactionStatusColor as getStatusColor, getTransactionStatusLabel as getStatusLabel } from '~/utils/status'

definePageMeta({
  layout: 'admin',
  middleware: ['admin']
})

const router = useRouter()
const route = useRoute()
const transactionService = useAdminTransactionService()

const isLoading = ref(false)
const transactions = ref<AdminTransactionListItemDto[]>([])

const typeOptions = [
  { label: 'Tất cả', value: null },
  { label: 'Chuyển khoản', value: 'TRANSFER' },
  { label: 'Thanh toán HĐ', value: 'PAYMENT' },
  { label: 'Nạp tiền', value: 'DEPOSIT' },
  { label: 'Phí dịch vụ', value: 'FEE' }
]

const statusOptions = [
  { label: 'Tất cả', value: null },
  { label: 'Thành công', value: 'COMPLETED' },
  { label: 'Chờ xử lý', value: 'PENDING' },
  { label: 'Thất bại', value: 'FAILED' },
  { label: 'Hoàn tiền', value: 'REVERSED' }
]

const filters = ref({
  search: route.query.search?.toString() || '',
  type: route.query.type?.toString() || null,
  status: route.query.status?.toString() || null,
  fromDate: route.query.fromDate?.toString() || '',
  toDate: route.query.toDate?.toString() || ''
})

const pagination = ref({
  page: Number(route.query.page) || 1,
  rowsPerPage: Number(route.query.pageSize) || 10,
  rowsNumber: 0
})

const dateValidationError = computed(() => {
  if (filters.value.fromDate && filters.value.toDate) {
    return new Date(filters.value.fromDate) > new Date(filters.value.toDate)
  }
  return false
})

const columns = [
  { name: 'referenceNumber', label: 'Mã GD', field: 'referenceNumber', align: 'left' as const, sortable: false },
  { name: 'transactionType', label: 'Phân loại', field: 'transactionType', align: 'left' as const, sortable: false },
  { name: 'sourceAccountNumber', label: 'TK Nguồn', field: 'sourceAccountNumber', align: 'left' as const, sortable: false },
  { name: 'destinationAccountNumber', label: 'TK Đích', field: 'destinationAccountNumber', align: 'left' as const, sortable: false },
  { name: 'amount', label: 'Số tiền', field: 'amount', align: 'right' as const, sortable: false },
  { name: 'status', label: 'Trạng thái', field: 'status', align: 'center' as const, sortable: false },
  { name: 'createdAtUtc', label: 'Thời gian', field: 'createdAtUtc', align: 'left' as const, sortable: false },
  { name: 'actions', label: '', field: 'actions', align: 'right' as const, sortable: false }
]

const fetchTransactions = async (props?: any) => {
  if (dateValidationError.value) return

  isLoading.value = true
  
  if (props && props.pagination) {
    pagination.value.page = props.pagination.page
    pagination.value.rowsPerPage = props.pagination.rowsPerPage
  }

  // Update URL
  const query: any = {
    page: pagination.value.page,
    pageSize: pagination.value.rowsPerPage
  }
  if (filters.value.search) query.search = filters.value.search
  if (filters.value.type) query.type = filters.value.type
  if (filters.value.status) query.status = filters.value.status
  if (filters.value.fromDate) query.fromDate = filters.value.fromDate
  if (filters.value.toDate) query.toDate = filters.value.toDate
  router.replace({ query })

  try {
    const result = await transactionService.getTransactions({
      page: pagination.value.page,
      pageSize: pagination.value.rowsPerPage,
      search: filters.value.search || undefined,
      type: filters.value.type || undefined,
      status: filters.value.status || undefined,
      fromDate: filters.value.fromDate ? new Date(filters.value.fromDate).toISOString() : undefined,
      toDate: filters.value.toDate ? new Date(new Date(filters.value.toDate).setHours(23, 59, 59, 999)).toISOString() : undefined
    })
    
    transactions.value = result.items
    pagination.value.rowsNumber = result.totalItems
  } catch (error) {
    console.error('Failed to fetch transactions', error)
  } finally {
    isLoading.value = false
  }
}

const onRequest = (props: any) => {
  fetchTransactions(props)
}

const getTypeColor = (type: string) => {
  switch (type) {
    case 'TRANSFER': return 'primary'
    case 'PAYMENT': return 'orange'
    case 'DEPOSIT': return 'positive'
    case 'WITHDRAWAL': return 'negative'
    case 'FEE': return 'negative'
    default: return 'grey'
  }
}

const getTypeIcon = (type: string) => {
  switch (type) {
    case 'TRANSFER': return 'swap_horiz'
    case 'PAYMENT': return 'receipt_long'
    case 'DEPOSIT': return 'arrow_downward'
    case 'WITHDRAWAL': return 'arrow_upward'
    case 'FEE': return 'money_off'
    default: return 'sync'
  }
}

const getTypeLabel = (type: string) => {
  switch (type) {
    case 'TRANSFER': return 'Chuyển khoản'
    case 'PAYMENT': return 'Thanh toán'
    case 'DEPOSIT': return 'Nạp tiền'
    case 'WITHDRAWAL': return 'Rút tiền'
    case 'FEE': return 'Phí dịch vụ'
    default: return type
  }
}

onMounted(() => {
  fetchTransactions()
})
</script>

<style scoped>
.font-monospace {
  font-family: 'SFMono-Regular', Consolas, 'Liberation Mono', Menlo, monospace;
}
</style>
