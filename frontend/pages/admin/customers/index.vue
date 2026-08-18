<template>
  <div class="q-pa-md">
    <div class="row items-center justify-between q-mb-md">
      <div class="text-h5 text-weight-bold">Quản lý Khách hàng</div>
    </div>

    <q-card flat bordered class="q-mb-md">
      <q-card-section>
        <div class="row q-col-gutter-md">
          <div class="col-12 col-sm-6 col-md-4">
            <q-input 
              v-model="filters.search" 
              outlined 
              dense 
              placeholder="Tìm theo Mã, Tên, Email, SĐT" 
              clearable
              @keyup.enter="fetchCustomers"
            >
              <template v-slot:append>
                <q-icon name="search" @click="fetchCustomers" class="cursor-pointer" />
              </template>
            </q-input>
          </div>
          <div class="col-12 col-sm-6 col-md-4">
            <q-select
              v-model="filters.status"
              outlined
              dense
              :options="statusOptions"
              label="Trạng thái"
              clearable
              emit-value
              map-options
              @update:model-value="fetchCustomers"
            />
          </div>
          <div class="col-12 col-md-4 flex items-center justify-end">
             <q-btn color="primary" icon="refresh" label="Làm mới" @click="fetchCustomers" />
          </div>
        </div>
      </q-card-section>
    </q-card>

    <q-card flat bordered>
      <q-table
        :rows="customers"
        :columns="columns"
        row-key="id"
        :loading="isLoading"
        v-model:pagination="pagination"
        @request="onRequest"
        flat
        bordered
      >
        <template v-slot:body-cell-customerStatus="props">
          <q-td :props="props">
            <q-badge :color="getStatusColor(props.row.customerStatus)">
              {{ getStatusLabel(props.row.customerStatus) }}
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
              :to="`/admin/customers/${props.row.id}`"
            >
              <q-tooltip>Xem chi tiết</q-tooltip>
            </q-btn>
          </q-td>
        </template>

        <template v-slot:no-data>
          <div class="full-width row flex-center text-accent q-pa-md">
            <q-icon size="2em" name="sentiment_dissatisfied" />
            <span class="q-ml-sm">Không tìm thấy khách hàng phù hợp.</span>
          </div>
        </template>
      </q-table>
    </q-card>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAdminCustomerService } from '~/services/adminCustomerService'
import type { AdminCustomerListItemDto } from '~/types/adminCustomer'
import { formatDateTime } from '~/utils/date'

definePageMeta({
  layout: 'admin',
  middleware: ['admin']
})

const router = useRouter()
const route = useRoute()
const customerService = useAdminCustomerService()

const isLoading = ref(false)
const customers = ref<AdminCustomerListItemDto[]>([])

const statusOptions = [
  { label: 'Tất cả', value: null },
  { label: 'Đang hoạt động', value: 'ACTIVE' },
  { label: 'Bị tạm khóa', value: 'SUSPENDED' },
  { label: 'Đã đóng', value: 'CLOSED' }
]

const filters = ref({
  search: route.query.search?.toString() || '',
  status: route.query.status?.toString() || null
})

const pagination = ref({
  page: Number(route.query.page) || 1,
  rowsPerPage: Number(route.query.pageSize) || 10,
  rowsNumber: 0
})

const columns = [
  { name: 'customerCode', label: 'Mã KH', field: 'customerCode', align: 'left' as const, sortable: false },
  { name: 'fullName', label: 'Họ và tên', field: 'fullName', align: 'left' as const, sortable: false },
  { name: 'email', label: 'Email', field: 'email', align: 'left' as const, sortable: false },
  { name: 'phoneNumber', label: 'Số điện thoại', field: 'phoneNumber', align: 'left' as const, sortable: false },
  { name: 'customerStatus', label: 'Trạng thái', field: 'customerStatus', align: 'center' as const, sortable: false },
  { name: 'accountsCount', label: 'Số TK', field: 'accountsCount', align: 'center' as const, sortable: false },
  { name: 'createdAtUtc', label: 'Ngày tạo', field: 'createdAtUtc', align: 'left' as const, sortable: false },
  { name: 'actions', label: 'Thao tác', field: 'actions', align: 'right' as const, sortable: false }
]

const fetchCustomers = async (props?: any) => {
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
  if (filters.value.status) query.status = filters.value.status
  router.replace({ query })

  try {
    const result = await customerService.getCustomers({
      page: pagination.value.page,
      pageSize: pagination.value.rowsPerPage,
      search: filters.value.search || undefined,
      status: filters.value.status || undefined
    })
    
    customers.value = result.items
    pagination.value.rowsNumber = result.totalItems
  } catch (error) {
    console.error('Failed to fetch customers', error)
  } finally {
    isLoading.value = false
  }
}

const onRequest = (props: any) => {
  fetchCustomers(props)
}

const getStatusColor = (status: string) => {
  switch (status) {
    case 'ACTIVE': return 'positive'
    case 'SUSPENDED': return 'negative'
    case 'CLOSED': return 'grey-6'
    default: return 'grey'
  }
}

const getStatusLabel = (status: string) => {
  switch (status) {
    case 'ACTIVE': return 'Hoạt động'
    case 'SUSPENDED': return 'Tạm khóa'
    case 'CLOSED': return 'Đã đóng'
    default: return status
  }
}

onMounted(() => {
  fetchCustomers()
})
</script>
