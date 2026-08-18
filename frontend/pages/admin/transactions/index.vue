<template>
  <div class="q-pa-md">
    <div class="row items-center justify-between q-mb-md">
      <div class="text-h5 text-weight-bold">Quản lý Giao dịch</div>
    </div>

    <q-card flat bordered class="q-mb-md">
      <q-card-section>
        <div class="row q-col-gutter-md">
          <div class="col-12 col-sm-4 col-md-3">
            <q-input 
              v-model="filters.search" 
              outlined 
              dense 
              placeholder="Tìm mã tham chiếu" 
              clearable
              @keyup.enter="fetchTransactions"
            >
              <template v-slot:append>
                <q-icon name="search" @click="fetchTransactions" class="cursor-pointer" />
              </template>
            </q-input>
          </div>
          <div class="col-12 col-sm-4 col-md-2">
            <q-select
              v-model="filters.type"
              outlined
              dense
              :options="typeOptions"
              label="Loại giao dịch"
              clearable
              emit-value
              map-options
              @update:model-value="fetchTransactions"
            />
          </div>
          <div class="col-12 col-sm-4 col-md-2">
            <q-select
              v-model="filters.status"
              outlined
              dense
              :options="statusOptions"
              label="Trạng thái"
              clearable
              emit-value
              map-options
              @update:model-value="fetchTransactions"
            />
          </div>
          <div class="col-12 col-sm-6 col-md-2">
            <q-input outlined dense v-model="filters.fromDate" label="Từ ngày" type="date" clearable @change="fetchTransactions" />
          </div>
          <div class="col-12 col-sm-6 col-md-2">
            <q-input outlined dense v-model="filters.toDate" label="Đến ngày" type="date" clearable @change="fetchTransactions" />
          </div>
          <div class="col-12 col-md-1 flex items-center justify-end">
             <q-btn color="primary" icon="refresh" label="Làm mới" @click="fetchTransactions" class="full-width" />
          </div>
        </div>
        
        <!-- Validation Error -->
        <div v-if="dateValidationError" class="text-negative q-mt-sm">
          Ngày bắt đầu không được lớn hơn ngày kết thúc.
        </div>
      </q-card-section>
    </q-card>

    <q-card flat bordered>
      <q-table
        :rows="transactions"
        :columns="columns"
        row-key="id"
        :loading="isLoading"
        v-model:pagination="pagination"
        @request="onRequest"
        flat
        bordered
      >
        <template v-slot:body-cell-amount="props">
          <q-td :props="props" class="text-weight-bold">
            <span :class="props.row.transactionType === 'FEE' ? 'text-negative' : 'text-primary'">
              {{ formatCurrency(props.row.amount, props.row.currency) }}
            </span>
          </q-td>
        </template>

        <template v-slot:body-cell-transactionType="props">
          <q-td :props="props">
            <q-badge :color="getTypeColor(props.row.transactionType)" outline>
              {{ getTypeLabel(props.row.transactionType) }}
            </q-badge>
          </q-td>
        </template>

        <template v-slot:body-cell-status="props">
          <q-td :props="props">
            <q-badge :color="getStatusColor(props.row.status)">
              {{ getStatusLabel(props.row.status) }}
            </q-badge>
          </q-td>
        </template>
        
        <template v-slot:body-cell-createdAtUtc="props">
          <q-td :props="props">
            {{ formatDateTime(props.row.createdAtUtc) }}
          </q-td>
        </template>

        <template v-slot:body-cell-actions="props">
          <q-td :props="props" class="text-right">
            <q-btn
              flat
              round
              color="primary"
              icon="visibility"
              size="sm"
              :to="`/admin/transactions/${props.row.id}`"
            >
              <q-tooltip>Xem chi tiết</q-tooltip>
            </q-btn>
          </q-td>
        </template>

        <template v-slot:no-data>
          <div class="full-width row flex-center text-accent q-pa-md">
            <q-icon size="2em" name="sentiment_dissatisfied" />
            <span class="q-ml-sm">Không tìm thấy giao dịch.</span>
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
  { name: 'referenceNumber', label: 'Mã tham chiếu', field: 'referenceNumber', align: 'left' as const, sortable: false },
  { name: 'transactionType', label: 'Loại', field: 'transactionType', align: 'left' as const, sortable: false },
  { name: 'sourceAccountNumber', label: 'TK Nguồn', field: 'sourceAccountNumber', align: 'left' as const, sortable: false },
  { name: 'destinationAccountNumber', label: 'TK Đích', field: 'destinationAccountNumber', align: 'left' as const, sortable: false },
  { name: 'amount', label: 'Số tiền', field: 'amount', align: 'right' as const, sortable: false },
  { name: 'status', label: 'Trạng thái', field: 'status', align: 'center' as const, sortable: false },
  { name: 'createdAtUtc', label: 'Thời gian', field: 'createdAtUtc', align: 'left' as const, sortable: false },
  { name: 'actions', label: 'Thao tác', field: 'actions', align: 'right' as const, sortable: false }
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
    case 'FEE': return 'red'
    default: return 'grey'
  }
}

const getTypeLabel = (type: string) => {
  switch (type) {
    case 'TRANSFER': return 'Chuyển khoản'
    case 'PAYMENT': return 'Thanh toán'
    case 'FEE': return 'Phí dịch vụ'
    default: return type
  }
}

onMounted(() => {
  fetchTransactions()
})
</script>
