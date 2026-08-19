<template>
  <div class="q-pa-md">
    <div class="row items-center justify-between q-mb-md">
      <div class="text-h5 text-weight-bold">Quản lý Thanh toán Hóa đơn</div>
    </div>

    <q-card flat bordered class="q-mb-md">
      <q-card-section>
        <div class="row q-col-gutter-md">
          <div class="col-12 col-sm-4 col-md-3">
            <q-input 
              v-model="filters.reference" 
              outlined 
              dense 
              placeholder="Tìm mã thanh toán" 
              clearable
              @keyup.enter="fetchPayments"
            >
              <template v-slot:append>
                <q-icon name="search" @click="fetchPayments" class="cursor-pointer" />
              </template>
            </q-input>
          </div>
          <div class="col-12 col-sm-4 col-md-2">
            <q-select
              v-model="filters.billType"
              outlined
              dense
              :options="billTypeOptions"
              label="Loại hóa đơn"
              clearable
              emit-value
              map-options
              @update:model-value="fetchPayments"
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
              @update:model-value="fetchPayments"
            />
          </div>
          <div class="col-12 col-sm-6 col-md-2">
            <q-input outlined dense v-model="filters.fromDate" label="Từ ngày" type="date" clearable @change="fetchPayments" />
          </div>
          <div class="col-12 col-sm-6 col-md-2">
            <q-input outlined dense v-model="filters.toDate" label="Đến ngày" type="date" clearable @change="fetchPayments" />
          </div>
          <div class="col-12 col-md-1 flex items-center justify-end">
             <q-btn color="primary" icon="refresh" label="Làm mới" @click="fetchPayments" class="full-width" />
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
        :rows="payments"
        :columns="columns"
        row-key="id"
        :loading="isLoading"
        v-model:pagination="pagination"
        @request="onRequest"
        flat
        bordered
      >
        <template v-slot:body-cell-amount="props">
          <q-td :props="props" class="text-weight-bold text-orange-8">
            {{ formatCurrency(props.row.amount, props.row.currency) }}
          </q-td>
        </template>

        <template v-slot:body-cell-billType="props">
          <q-td :props="props">
            <div class="row items-center no-wrap">
              <q-icon :name="getBillTypeIcon(props.row.billType)" size="sm" class="q-mr-xs text-grey-7" />
              <span>{{ getBillTypeLabel(props.row.billType) }}</span>
            </div>
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
              :to="`/admin/payments/${props.row.id}`"
            >
              <q-tooltip>Xem chi tiết</q-tooltip>
            </q-btn>
          </q-td>
        </template>

        <template v-slot:no-data>
          <div class="full-width row flex-center text-accent q-pa-md">
            <q-icon size="2em" name="sentiment_dissatisfied" />
            <span class="q-ml-sm">Không tìm thấy thanh toán.</span>
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
  { label: 'Tất cả', value: null },
  { label: 'Điện', value: 'ELECTRICITY' },
  { label: 'Nước', value: 'WATER' },
  { label: 'Internet', value: 'INTERNET' },
  { label: 'Giáo dục', value: 'EDUCATION' },
  { label: 'Khác', value: 'OTHER' }
]

const statusOptions = [
  { label: 'Tất cả', value: null },
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
  { name: 'actions', label: 'Thao tác', field: 'actions', align: 'right' as const, sortable: false }
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
