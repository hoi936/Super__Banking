<template>
  <div class="admin-page">
    <div class="admin-page-header">
      <div>
        <div class="admin-page-kicker">Nhân sự vận hành</div>
        <div class="admin-page-title">Quản lý người dùng hệ thống</div>
        <div class="admin-page-subtitle">Kiểm soát tài khoản Staff/Admin, quyền truy cập và trạng thái đăng nhập.</div>
      </div>
    </div>

    <q-card flat class="admin-filter-card q-mb-lg">
      <q-card-section class="q-pa-lg">
        <div class="row q-col-gutter-md items-center">
          <div class="col-12 col-sm-6 col-md-4">
            <div class="admin-field-label">Tìm kiếm</div>
            <q-input 
              v-model="filters.search" 
              outlined 
              dense 
              placeholder="Nhập email, tên nhân sự..." 
              bg-color="grey-1"
              clearable
              @keyup.enter="fetchUsers"
            >
              <template v-slot:prepend>
                <q-icon name="search" color="primary" />
              </template>
            </q-input>
          </div>
          <div class="col-12 col-sm-6 col-md-3">
            <div class="admin-field-label">Vai trò</div>
            <q-select
              popup-content-class="text-dark bg-white shadow-2"
              options-selected-class="text-primary text-weight-bold"
              v-model="filters.role"
              outlined
              dense
              :options="roleOptions"
              bg-color="grey-1"
              clearable
              emit-value
              map-options
              @update:model-value="fetchUsers"
            >
              <template v-slot:prepend>
                <q-icon name="admin_panel_settings" color="grey-7" />
              </template>
            </q-select>
          </div>
          <div class="col-12 col-sm-8 col-md-3">
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
              @update:model-value="fetchUsers"
            />
          </div>
          <div class="col-12 col-sm-4 col-md-2 flex items-center justify-end" style="margin-top: 32px">
             <q-btn unelevated color="primary" icon="refresh" @click="fetchUsers" class="admin-action-btn full-width" padding="8px" />
          </div>
        </div>
      </q-card-section>
    </q-card>

    <q-card flat class="admin-card admin-table-card">
      <q-table
        class="premium-table"
        :rows="users"
        :columns="columns"
        row-key="id"
        :loading="isLoading"
        v-model:pagination="pagination"
        @request="onRequest"
        flat
        :rows-per-page-options="[10, 20, 50]"
      >
        <template v-slot:body-cell-email="props">
          <q-td :props="props">
            <div class="row items-center">
              <q-avatar size="32px" color="blue-1" text-color="primary" class="q-mr-sm text-weight-bold">
                {{ (props.row.email || 'U').charAt(0).toUpperCase() }}
              </q-avatar>
              <span class="text-weight-bold text-dark">{{ props.row.email }}</span>
            </div>
          </q-td>
        </template>
        
        <template v-slot:body-cell-fullName="props">
          <q-td :props="props" class="text-weight-medium text-grey-8">
            {{ props.row.fullName || 'Chưa cập nhật' }}
          </q-td>
        </template>

        <template v-slot:body-cell-roles="props">
          <q-td :props="props">
            <q-chip 
              v-for="role in props.row.roles" 
              :key="role" 
              dense 
              :color="role === 'ADMIN' ? 'red-1' : (role === 'STAFF' ? 'blue-1' : 'grey-2')"
              :text-color="role === 'ADMIN' ? 'red-7' : (role === 'STAFF' ? 'blue-8' : 'grey-8')"
              class="text-weight-bold q-px-sm"
            >
              {{ role }}
            </q-chip>
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
              :to="`/admin/users/${props.row.id}`"
            >
              <q-tooltip class="bg-dark">Quản lý tài khoản</q-tooltip>
            </q-btn>
          </q-td>
        </template>

        <template v-slot:no-data>
          <div class="full-width row flex-center text-grey-5 q-pa-xl">
            <div class="text-center">
              <q-icon size="4em" name="manage_accounts" class="q-mb-md opacity-30" />
              <div class="text-h6 text-weight-medium">Không tìm thấy người dùng</div>
              <div class="text-caption">Chưa có người dùng nào khớp với bộ lọc</div>
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
import { useAdminUserService } from '~/services/adminUserService'
import type { AdminUserListItemDto } from '~/types/adminUser'
import { formatDateTime } from '~/utils/date'
import { getUserStatusColor as getStatusColor, getUserStatusLabel as getStatusLabel } from '~/utils/status'

definePageMeta({
  layout: 'admin',
  middleware: ['admin'] // Will be caught by specific ADMIN check in middleware
})

const router = useRouter()
const route = useRoute()
const userService = useAdminUserService()

const isLoading = ref(false)
const users = ref<AdminUserListItemDto[]>([])

const roleOptions = [
  { label: 'Tất cả vai trò', value: null },
  { label: 'Quản trị viên (ADMIN)', value: 'ADMIN' },
  { label: 'Nhân viên (STAFF)', value: 'STAFF' },
  { label: 'Khách hàng (CUSTOMER)', value: 'CUSTOMER' }
]

const statusOptions = [
  { label: 'Tất cả trạng thái', value: null },
  { label: 'Đang hoạt động', value: 'ACTIVE' },
  { label: 'Bị tạm khóa', value: 'SUSPENDED' },
  { label: 'Bị khóa cứng', value: 'LOCKED' }
]

const filters = ref({
  search: route.query.search?.toString() || '',
  role: route.query.role?.toString() || null,
  status: route.query.status?.toString() || null
})

const pagination = ref({
  page: Number(route.query.page) || 1,
  rowsPerPage: Number(route.query.pageSize) || 10,
  rowsNumber: 0
})

const columns = [
  { name: 'email', label: 'Tài khoản Email', field: 'email', align: 'left' as const, sortable: false },
  { name: 'fullName', label: 'Họ tên', field: 'fullName', align: 'left' as const, sortable: false },
  { name: 'roles', label: 'Vai trò', field: 'roles', align: 'left' as const, sortable: false },
  { name: 'status', label: 'Trạng thái', field: 'status', align: 'center' as const, sortable: false },
  { name: 'createdAtUtc', label: 'Ngày tạo', field: 'createdAtUtc', align: 'left' as const, sortable: false },
  { name: 'actions', label: '', field: 'actions', align: 'right' as const, sortable: false }
]

const fetchUsers = async (props?: any) => {
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
  if (filters.value.role) query.role = filters.value.role
  if (filters.value.status) query.status = filters.value.status
  router.replace({ query })

  try {
    const result = await userService.getUsers({
      page: pagination.value.page,
      pageSize: pagination.value.rowsPerPage,
      search: filters.value.search || undefined,
      role: filters.value.role || undefined,
      status: filters.value.status || undefined
    })
    
    users.value = result.items
    pagination.value.rowsNumber = result.totalItems
  } catch (error) {
    console.error('Failed to fetch users', error)
  } finally {
    isLoading.value = false
  }
}

const onRequest = (props: any) => {
  fetchUsers(props)
}

onMounted(() => {
  fetchUsers()
})
</script>
