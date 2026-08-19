<template>
  <div class="admin-page">
    <div class="admin-page-header">
      <div>
        <div class="admin-page-kicker">Hồ sơ khách hàng</div>
        <div class="admin-page-title">Quản lý khách hàng</div>
        <div class="admin-page-subtitle">Tra cứu, kiểm tra trạng thái và mở hồ sơ tài khoản của từng khách hàng.</div>
      </div>
    </div>

    <q-card flat class="admin-filter-card q-mb-lg">
      <q-card-section class="q-pa-lg">
        <div class="row q-col-gutter-md items-center">
          <div class="col-12 col-sm-5 col-md-4">
            <div class="admin-field-label">Tìm kiếm</div>
            <q-input 
              v-model="filters.search" 
              outlined 
              dense 
              placeholder="Nhập mã, tên, email, sđt..." 
              clearable
              bg-color="grey-1"
              @keyup.enter="fetchCustomers"
            >
              <template v-slot:prepend>
                <q-icon name="search" color="primary" />
              </template>
            </q-input>
          </div>
          <div class="col-12 col-sm-4 col-md-3">
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
              @update:model-value="fetchCustomers"
            >
              <template v-slot:prepend>
                <q-icon name="filter_alt" color="grey-7" />
              </template>
            </q-select>
          </div>
          <div class="col-12 col-sm-3 col-md-5 flex justify-end" style="margin-top: 32px">
             <q-btn unelevated color="primary" icon="refresh" label="Làm mới" @click="fetchCustomers" class="admin-action-btn" padding="8px 18px" />
          </div>
        </div>
      </q-card-section>
    </q-card>

    <q-card flat class="admin-card admin-table-card">
      <q-table
        class="premium-table"
        :rows="customers"
        :columns="columns"
        row-key="id"
        :loading="isLoading"
        v-model:pagination="pagination"
        @request="onRequest"
        flat
        :rows-per-page-options="[10, 20, 50]"
      >
        <template v-slot:body-cell-customerCode="props">
          <q-td :props="props">
            <span class="text-weight-bold text-primary">{{ props.row.customerCode }}</span>
          </q-td>
        </template>

        <template v-slot:body-cell-fullName="props">
          <q-td :props="props">
            <div class="row items-center">
              <q-avatar size="32px" color="blue-1" text-color="primary" class="q-mr-sm text-weight-bold">
                {{ (props.row.fullName || props.row.customerCode || 'C').charAt(0).toUpperCase() }}
              </q-avatar>
              <span class="text-weight-medium text-dark">{{ props.row.fullName }}</span>
            </div>
          </q-td>
        </template>

        <template v-slot:body-cell-email="props">
          <q-td :props="props" class="text-grey-8">
            {{ props.row.email }}
          </q-td>
        </template>

        <template v-slot:body-cell-phoneNumber="props">
          <q-td :props="props" class="text-grey-8">
            {{ props.row.phoneNumber }}
          </q-td>
        </template>

        <template v-slot:body-cell-customerStatus="props">
          <q-td :props="props">
            <q-chip 
              :color="getStatusColor(props.row.customerStatus) + '-1'" 
              :text-color="getStatusColor(props.row.customerStatus)"
              size="sm"
              class="admin-chip"
            >
              {{ getStatusLabel(props.row.customerStatus) }}
            </q-chip>
          </q-td>
        </template>
        
        <template v-slot:body-cell-accountsCount="props">
          <q-td :props="props">
            <q-badge color="grey-3" text-color="dark" class="text-weight-bold q-pa-sm">
              {{ props.row.accountsCount }}
            </q-badge>
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
              :to="`/admin/customers/${props.row.id}`"
            >
              <q-tooltip class="bg-dark">Xem hồ sơ chi tiết</q-tooltip>
            </q-btn>
          </q-td>
        </template>

        <template v-slot:no-data>
          <div class="full-width row flex-center text-grey-5 q-pa-xl">
            <div class="text-center">
              <q-icon size="4em" name="search_off" class="q-mb-md opacity-30" />
              <div class="text-h6 text-weight-medium">Không tìm thấy khách hàng nào</div>
              <div class="text-caption">Hãy thử thay đổi từ khóa hoặc bộ lọc</div>
            </div>
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
import { getUserStatusColor as getStatusColor, getUserStatusLabel as getStatusLabel } from '~/utils/status'

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
  { label: 'Tất cả trạng thái', value: null },
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
  { name: 'email', label: 'Email liên hệ', field: 'email', align: 'left' as const, sortable: false },
  { name: 'phoneNumber', label: 'Điện thoại', field: 'phoneNumber', align: 'left' as const, sortable: false },
  { name: 'customerStatus', label: 'Trạng thái', field: 'customerStatus', align: 'center' as const, sortable: false },
  { name: 'accountsCount', label: 'Tài khoản', field: 'accountsCount', align: 'center' as const, sortable: false },
  { name: 'createdAtUtc', label: 'Ngày đăng ký', field: 'createdAtUtc', align: 'left' as const, sortable: false },
  { name: 'actions', label: '', field: 'actions', align: 'right' as const, sortable: false }
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

onMounted(() => {
  fetchCustomers()
})
</script>
