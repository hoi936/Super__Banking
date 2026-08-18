<template>
  <div class="q-pa-md">
    <div class="row items-center q-mb-md">
      <div class="text-h6 text-primary">Lịch sử thanh toán hóa đơn</div>
    </div>

    <!-- Filters -->
    <q-card flat bordered class="q-mb-md bg-grey-1">
      <q-card-section>
        <div class="row q-col-gutter-md">
          <div class="col-12 col-md-3">
            <q-select
              v-model="filters.status"
              :options="[
                { label: 'Tất cả trạng thái', value: '' },
                { label: 'Thành công', value: 'COMPLETED' },
                { label: 'Đang xử lý', value: 'PENDING' },
                { label: 'Thất bại', value: 'FAILED' }
              ]"
              emit-value
              map-options
              outlined
              dense
              label="Trạng thái"
            />
          </div>
          <div class="col-12 col-md-3">
            <q-select
              v-model="filters.billType"
              :options="[
                { label: 'Tất cả loại', value: '' },
                { label: 'Điện', value: 'ELECTRICITY' },
                { label: 'Nước', value: 'WATER' },
                { label: 'Internet', value: 'INTERNET' },
                { label: 'Giáo dục', value: 'EDUCATION' },
                { label: 'Khác', value: 'OTHER' }
              ]"
              emit-value
              map-options
              outlined
              dense
              label="Loại dịch vụ"
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
      </q-card-section>
    </q-card>

    <!-- Table for Desktop -->
    <q-table
      v-if="$q.screen.gt.sm"
      :rows="payments"
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
        <q-td :props="props" class="text-weight-bold text-negative">
          -{{ formatCurrency(props.row.amount) }}
        </q-td>
      </template>
      <template v-slot:body-cell-status="props">
        <q-td :props="props">
          <q-chip :color="getStatusColor(props.row.status)" text-color="white" size="sm" dense>
            {{ getStatusLabel(props.row.status) }}
          </q-chip>
        </q-td>
      </template>
      <template v-slot:body-cell-action="props">
        <q-td :props="props">
          <q-btn flat round dense icon="visibility" color="primary" :to="`/payments/${props.row.id}`" />
        </q-td>
      </template>
      <template v-slot:no-data>
        <div class="full-width row flex-center text-grey-7 q-pa-lg">
          Bạn chưa có giao dịch thanh toán nào phù hợp.
        </div>
      </template>
    </q-table>

    <!-- List for Mobile -->
    <div v-else>
      <div v-if="isLoading" class="text-center q-pa-md">
        <q-spinner color="primary" size="2em" />
      </div>
      <div v-else-if="payments.length === 0" class="text-center text-grey-7 q-pa-lg bg-white rounded-borders">
        Bạn chưa có giao dịch thanh toán nào phù hợp.
      </div>
      <q-list v-else bordered separator class="bg-white rounded-borders">
        <q-item v-for="payment in payments" :key="payment.id" clickable :to="`/payments/${payment.id}`">
          <q-item-section>
            <q-item-label class="text-weight-bold">{{ payment.providerName }}</q-item-label>
            <q-item-label caption>{{ formatDateTime(payment.createdAtUtc) }}</q-item-label>
            <q-item-label caption>Mã GD: {{ payment.referenceNumber }}</q-item-label>
          </q-item-section>
          <q-item-section side>
            <q-item-label class="text-weight-bold text-negative">
              -{{ formatCurrency(payment.amount) }}
            </q-item-label>
            <q-item-label>
              <q-chip :color="getStatusColor(payment.status)" text-color="white" size="xs" dense>
                {{ getStatusLabel(payment.status) }}
              </q-chip>
            </q-item-label>
          </q-item-section>
        </q-item>
      </q-list>

      <div class="row justify-center q-mt-md" v-if="payments.length > 0">
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
import { usePaymentService } from '~/services/paymentService'
import type { PaymentListItemDto } from '~/types/payment'

definePageMeta({
  layout: 'default',
  middleware: ['customer']
})

const route = useRoute()
const router = useRouter()
const paymentService = usePaymentService()

const payments = ref<PaymentListItemDto[]>([])
const isLoading = ref(false)

const filters = ref({
  status: (route.query.status as string) || '',
  billType: (route.query.billType as string) || '',
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
  { name: 'provider', label: 'Nhà cung cấp', field: 'providerName', align: 'left' as const },
  { name: 'amount', label: 'Số tiền', field: 'amount', align: 'right' as const },
  { name: 'createdAtUtc', label: 'Thời gian', field: 'createdAtUtc', format: (val: string) => formatDateTime(val), align: 'center' as const },
  { name: 'status', label: 'Trạng thái', field: 'status', align: 'center' as const },
  { name: 'action', label: 'Chi tiết', field: 'id', align: 'center' as const }
]

onMounted(() => {
  loadData()
})

const applyFilters = () => {
  pagination.value.page = 1
  
  const query: any = {}
  if (filters.value.status) query.status = filters.value.status
  if (filters.value.billType) query.billType = filters.value.billType
  if (filters.value.fromDate) query.fromDate = filters.value.fromDate
  if (filters.value.toDate) query.toDate = filters.value.toDate

  router.replace({ query })
  loadData()
}

const loadData = async (props?: any) => {
  const { page, rowsPerPage } = props?.pagination || pagination.value
  isLoading.value = true

  try {
    const res = await paymentService.getPayments({
      page,
      pageSize: rowsPerPage,
      status: filters.value.status || undefined,
      billType: filters.value.billType || undefined,
      fromDate: filters.value.fromDate ? new Date(filters.value.fromDate).toISOString() : undefined,
      toDate: filters.value.toDate ? new Date(new Date(filters.value.toDate).setHours(23,59,59,999)).toISOString() : undefined
    })

    payments.value = res.items
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

const getStatusColor = (status: string) => {
  switch (status) {
    case 'COMPLETED': return 'positive'
    case 'PENDING': return 'warning'
    case 'FAILED': return 'negative'
    default: return 'grey'
  }
}

const getStatusLabel = (status: string) => {
  switch (status) {
    case 'COMPLETED': return 'Thành công'
    case 'PENDING': return 'Đang xử lý'
    case 'FAILED': return 'Thất bại'
    default: return status
  }
}
</script>
