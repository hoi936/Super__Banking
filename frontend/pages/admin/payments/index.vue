<template>
  <div class="admin-page">
    <div class="admin-page-header">
      <div>
        <div class="admin-page-kicker">Dịch vụ tiện ích</div>
        <div class="admin-page-title">Quản lý thanh toán hóa đơn</div>
        <div class="admin-page-subtitle">Theo dõi thanh toán, nhà cung cấp, mã hóa đơn và trạng thái xử lý dịch vụ.</div>
      </div>
    </div>

    <q-card flat class="admin-filter-card q-mb-lg">
      <q-card-section class="q-pa-lg">
        <div class="row q-col-gutter-md items-center">
          <div class="col-12 col-sm-6 col-md-3">
            <div class="admin-field-label">Mã thanh toán</div>
            <q-input 
              v-model="filters.reference" 
              outlined 
              dense 
              placeholder="Nhập mã thanh toán..." 
              bg-color="grey-1"
              clearable
              @keyup.enter="fetchPayments"
            >
              <template v-slot:prepend>
                <q-icon name="search" color="primary" />
              </template>
            </q-input>
          </div>
          <div class="col-12 col-sm-6 col-md-2">
            <div class="admin-field-label">Dịch vụ</div>
            <q-select
              popup-content-class="text-dark bg-white shadow-2"
              options-selected-class="text-primary text-weight-bold"
              v-model="filters.billType"
              outlined
              dense
              :options="billTypeOptions"
              bg-color="grey-1"
              clearable
              emit-value
              map-options
              @update:model-value="fetchPayments"
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
              @update:model-value="fetchPayments"
            />
          </div>
          <div class="col-12 col-sm-4 col-md-2">
            <div class="admin-field-label">Từ ngày</div>
            <q-input outlined dense v-model="filters.fromDate" type="date" bg-color="grey-1" clearable @change="fetchPayments" />
          </div>
          <div class="col-12 col-sm-4 col-md-2">
            <div class="admin-field-label">Đến ngày</div>
            <q-input outlined dense v-model="filters.toDate" type="date" bg-color="grey-1" clearable @change="fetchPayments" />
          </div>
          <div class="col-12 col-md-1 flex items-center justify-end" style="margin-top: 32px">
             <q-btn unelevated color="primary" icon="refresh" @click="fetchPayments" class="admin-action-btn full-width" padding="8px" />
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
        :rows="payments"
        :columns="columns"
        row-key="id"
        :loading="isLoading"
        v-model:pagination="pagination"
        @request="onRequest"
        flat
        :rows-per-page-options="[10, 20, 50]"
      >
        <template v-slot:body-cell-paymentReference="props">
          <q-td :props="props">
            <span class="text-weight-bold text-dark">{{ props.row.paymentReference }}</span>
          </q-td>
        </template>

        <template v-slot:body-cell-amount="props">
          <q-td :props="props" class="text-weight-bold text-orange-8">
            {{ formatCurrency(props.row.amount, props.row.currency) }}
          </q-td>
        </template>

        <template v-slot:body-cell-billType="props">
          <q-td :props="props">
            <div class="row items-center no-wrap">
              <q-avatar size="28px" color="blue-1" text-color="primary" class="q-mr-sm">
                <q-icon :name="getBillTypeIcon(props.row.billType)" size="xs" />
              </q-avatar>
              <span class="text-weight-medium">{{ getBillTypeLabel(props.row.billType) }}</span>
            </div>
          </q-td>
        </template>

        <template v-slot:body-cell-billNumber="props">
          <q-td :props="props" class="text-grey-8 font-monospace">
            {{ props.row.billNumber }}
          </q-td>
        </template>

        <template v-slot:body-cell-accountNumber="props">
          <q-td :props="props" class="text-grey-8 font-monospace">
            {{ props.row.accountNumber }}
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
              :to="`/admin/payments/${props.row.id}`"
            >
              <q-tooltip class="bg-dark">Xem chi tiết</q-tooltip>
            </q-btn>
          </q-td>
        </template>

        <template v-slot:no-data>
          <div class="full-width row flex-center text-grey-5 q-pa-xl">
            <div class="text-center">
              <q-icon size="4em" name="account_balance_wallet" class="q-mb-md opacity-30" />
              <div class="text-h6 text-weight-medium">Không có dữ liệu thanh toán</div>
              <div class="text-caption">Chưa có giao dịch thanh toán hóa đơn nào phù hợp</div>
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
import { useAdminPaymentService } from '~/services/adminPaymentService'
import type { AdminPaymentListItemDto } from '~/types/adminPayment'
import { formatCurrency } from '~/utils/currency'
import { formatDateTime } from '~/utils/date'
import { getTransactionStatusColor as getStatusColor, getTransactionStatusLabel as getStatusLabel } from '~/utils/status'

definePageMeta({
  layout: 'admin',
  middleware: ['admin']
})

const router = useRouter()
const route = useRoute()
const paymentService = useAdminPaymentService()

const isLoading = ref(false)
const payments = ref<AdminPaymentListItemDto[]>([])

const billTypeOptions = [
  { label: 'Tất cả dịch vụ', value: null },
  { label: 'Điện lực', value: 'ELECTRICITY' },
  { label: 'Cấp nước', value: 'WATER' },
  { label: 'Internet', value: 'INTERNET' },
  { label: 'Giáo dục', value: 'EDUCATION' },
  { label: 'Khác', value: 'OTHER' }
]

const statusOptions = [
  { label: 'Tất cả trạng thái', value: null },
  { label: 'Thành công', value: 'COMPLETED' },
  { label: 'Chờ xử lý', value: 'PENDING' },
  { label: 'Thất bại', value: 'FAILED' }
]

const filters = ref({
  reference: route.query.reference?.toString() || '',
  billType: route.query.billType?.toString() || null,
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
  { name: 'paymentReference', label: 'Mã TT', field: 'paymentReference', align: 'left' as const, sortable: false },
  { name: 'billType', label: 'Dịch vụ', field: 'billType', align: 'left' as const, sortable: false },
  { name: 'billNumber', label: 'Mã Hóa đơn', field: 'billNumber', align: 'left' as const, sortable: false },
  { name: 'accountNumber', label: 'Tài khoản', field: 'accountNumber', align: 'left' as const, sortable: false },
  { name: 'amount', label: 'Số tiền', field: 'amount', align: 'right' as const, sortable: false },
  { name: 'status', label: 'Trạng thái', field: 'status', align: 'center' as const, sortable: false },
  { name: 'createdAtUtc', label: 'Thời gian', field: 'createdAtUtc', align: 'left' as const, sortable: false },
  { name: 'actions', label: '', field: 'actions', align: 'right' as const, sortable: false }
]

const fetchPayments = async (props?: any) => {
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
  if (filters.value.reference) query.reference = filters.value.reference
  if (filters.value.billType) query.billType = filters.value.billType
  if (filters.value.status) query.status = filters.value.status
  if (filters.value.fromDate) query.fromDate = filters.value.fromDate
  if (filters.value.toDate) query.toDate = filters.value.toDate
  router.replace({ query })

  try {
    const result = await paymentService.getPayments({
      page: pagination.value.page,
      pageSize: pagination.value.rowsPerPage,
      reference: filters.value.reference || undefined,
      billType: filters.value.billType || undefined,
      status: filters.value.status || undefined,
      fromDate: filters.value.fromDate ? new Date(filters.value.fromDate).toISOString() : undefined,
      toDate: filters.value.toDate ? new Date(new Date(filters.value.toDate).setHours(23, 59, 59, 999)).toISOString() : undefined
    })
    
    payments.value = result.items
    pagination.value.rowsNumber = result.totalItems
  } catch (error) {
    console.error('Failed to fetch payments', error)
  } finally {
    isLoading.value = false
  }
}

const onRequest = (props: any) => {
  fetchPayments(props)
}

const getBillTypeIcon = (type: string) => {
  switch (type) {
    case 'ELECTRICITY': return 'bolt'
    case 'WATER': return 'water_drop'
    case 'INTERNET': return 'wifi'
    case 'EDUCATION': return 'school'
    default: return 'receipt'
  }
}

const getBillTypeLabel = (type: string) => {
  switch (type) {
    case 'ELECTRICITY': return 'Điện lực'
    case 'WATER': return 'Cấp nước'
    case 'INTERNET': return 'Internet/Viễn thông'
    case 'EDUCATION': return 'Giáo dục'
    default: return 'Khác'
  }
}

onMounted(() => {
  fetchPayments()
})
</script>

<style scoped>
.font-monospace {
  font-family: 'SFMono-Regular', Consolas, 'Liberation Mono', Menlo, monospace;
}
</style>
