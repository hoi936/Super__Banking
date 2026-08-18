<template>
  <div class="q-pa-md">
    <div class="row items-center q-mb-md">
      <div class="text-h6 text-primary">Danh sách hóa đơn</div>
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
                { label: 'Chưa thanh toán', value: 'UNPAID' },
                { label: 'Đã thanh toán', value: 'PAID' },
                { label: 'Quá hạn', value: 'OVERDUE' },
                { label: 'Đã hủy', value: 'CANCELLED' }
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
              v-model="filters.type"
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
            <q-input v-model="filters.fromDueDate" outlined dense type="date" label="Hạn từ ngày" />
          </div>
          <div class="col-12 col-md-2">
            <q-input v-model="filters.toDueDate" outlined dense type="date" label="Hạn đến ngày" />
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
      :rows="bills"
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
        <q-td :props="props" class="text-weight-bold">
          {{ formatCurrency(props.row.amount) }}
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
          <q-btn flat round dense icon="visibility" color="primary" :to="`/bills/${props.row.id}`" />
        </q-td>
      </template>
      <template v-slot:no-data>
        <div class="full-width row flex-center text-grey-7 q-pa-lg">
          Bạn chưa có hóa đơn nào phù hợp.
        </div>
      </template>
    </q-table>

    <!-- List for Mobile -->
    <div v-else>
      <div v-if="isLoading" class="text-center q-pa-md">
        <q-spinner color="primary" size="2em" />
      </div>
      <div v-else-if="bills.length === 0" class="text-center text-grey-7 q-pa-lg bg-white rounded-borders">
        Bạn chưa có hóa đơn nào phù hợp.
      </div>
      <q-list v-else bordered separator class="bg-white rounded-borders">
        <q-item v-for="bill in bills" :key="bill.id" clickable :to="`/bills/${bill.id}`">
          <q-item-section>
            <q-item-label class="text-weight-bold">{{ bill.providerName }}</q-item-label>
            <q-item-label caption>Hạn: {{ formatDate(bill.dueDate) }}</q-item-label>
            <q-item-label caption>Mã HĐ: {{ bill.billNumber }}</q-item-label>
          </q-item-section>
          <q-item-section side>
            <q-item-label class="text-weight-bold text-primary">
              {{ formatCurrency(bill.amount) }}
            </q-item-label>
            <q-item-label>
              <q-chip :color="getStatusColor(bill.status)" text-color="white" size="xs" dense>
                {{ getStatusLabel(bill.status) }}
              </q-chip>
            </q-item-label>
          </q-item-section>
        </q-item>
      </q-list>

      <div class="row justify-center q-mt-md" v-if="bills.length > 0">
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
import { formatDate } from '~/utils/date'
import { useBillService } from '~/services/billService'
import type { BillListItemDto } from '~/types/bill'

definePageMeta({
  layout: 'default',
  middleware: ['customer']
})

const route = useRoute()
const router = useRouter()
const billService = useBillService()

const bills = ref<BillListItemDto[]>([])
const isLoading = ref(false)

const filters = ref({
  status: (route.query.status as string) || '',
  type: (route.query.type as string) || '',
  fromDueDate: (route.query.fromDueDate as string) || '',
  toDueDate: (route.query.toDueDate as string) || ''
})

const pagination = ref({
  page: 1,
  rowsPerPage: 10,
  rowsNumber: 0
})

const columns = [
  { name: 'provider', label: 'Nhà cung cấp', field: 'providerName', align: 'left' as const },
  { name: 'billNumber', label: 'Mã HĐ', field: 'billNumber', align: 'left' as const },
  { name: 'amount', label: 'Số tiền', field: 'amount', align: 'right' as const },
  { name: 'dueDate', label: 'Hạn thanh toán', field: 'dueDate', format: (val: string) => formatDate(val), align: 'center' as const },
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
  if (filters.value.type) query.type = filters.value.type
  if (filters.value.fromDueDate) query.fromDueDate = filters.value.fromDueDate
  if (filters.value.toDueDate) query.toDueDate = filters.value.toDueDate

  router.replace({ query })
  loadData()
}

const loadData = async (props?: any) => {
  const { page, rowsPerPage } = props?.pagination || pagination.value
  isLoading.value = true

  try {
    const res = await billService.getBills({
      page,
      pageSize: rowsPerPage,
      status: filters.value.status || undefined,
      type: filters.value.type || undefined,
      fromDueDate: filters.value.fromDueDate || undefined,
      toDueDate: filters.value.toDueDate || undefined
    })

    bills.value = res.items
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

</script>
