<template>
  <div class="q-pa-md">
    <div class="row items-center justify-between q-mb-md">
      <div class="text-h5 text-weight-bold">Quản lý Người dùng hệ thống</div>
    </div>

    <q-card flat bordered class="q-mb-md">
      <q-card-section>
        <div class="row q-col-gutter-md">
          <div class="col-12 col-sm-4 col-md-4">
            <q-input 
              v-model="filters.search" 
              outlined 
              dense 
              placeholder="Tìm theo Email, Mã NV, Tên" 
              clearable
              @keyup.enter="fetchUsers"
            >
              <template v-slot:append>
                <q-icon name="search" @click="fetchUsers" class="cursor-pointer" />
              </template>
            </q-input>
          </div>
          <div class="col-12 col-sm-4 col-md-3">
            <q-select
              v-model="filters.role"
              outlined
              dense
              :options="roleOptions"
              label="Vai trò (Role)"
              clearable
              emit-value
              map-options
              @update:model-value="fetchUsers"
            />
          </div>
          <div class="col-12 col-sm-4 col-md-3">
            <q-select
              v-model="filters.status"
              outlined
              dense
              :options="statusOptions"
              label="Trạng thái"
              clearable
              emit-value
              map-options
              @update:model-value="fetchUsers"
            />
          </div>
          <div class="col-12 col-md-2 flex items-center justify-end">
             <q-btn color="primary" icon="refresh" label="Làm mới" @click="fetchUsers" class="full-width" />
          </div>
        </div>
      </q-card-section>
    </q-card>

    <q-card flat bordered>
      <q-table
        :rows="users"
        :columns="columns"
        row-key="id"
        :loading="isLoading"
        v-model:pagination="pagination"
        @request="onRequest"
        flat
        bordered
      >
        <template v-slot:body-cell-roles="props">
          <q-td :props="props">
            <q-chip v-for="role in props.row.roles" :key="role" dense :color="role === 'ADMIN' ? 'red-2' : 'blue-1'">
              {{ role }}
            </q-chip>
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
              icon="manage_accounts"
              size="sm"
              :to="`/admin/users/${props.row.id}`"
            >
              <q-tooltip>Quản lý tài khoản</q-tooltip>
            </q-btn>
          </q-td>
        </template>

        <template v-slot:no-data>
          <div class="full-width row flex-center text-accent q-pa-md">
            <q-icon size="2em" name="sentiment_dissatisfied" />
            <span class="q-ml-sm">Không tìm thấy người dùng phù hợp.</span>
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
  { label: 'Tất cả', value: null },
  { label: 'Quản trị viên (ADMIN)', value: 'ADMIN' },
  { label: 'Nhân viên (STAFF)', value: 'STAFF' },
  { label: 'Khách hàng (CUSTOMER)', value: 'CUSTOMER' }
]

const statusOptions = [
  { label: 'Tất cả', value: null },
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
  { name: 'email', label: 'Email', field: 'email', align: 'left' as const, sortable: false },
  { name: 'fullName', label: 'Họ tên', field: 'fullName', align: 'left' as const, sortable: false },
  { name: 'roles', label: 'Vai trò', field: 'roles', align: 'left' as const, sortable: false },
  { name: 'status', label: 'Trạng thái', field: 'status', align: 'center' as const, sortable: false },
  { name: 'createdAtUtc', label: 'Ngày tạo', field: 'createdAtUtc', align: 'left' as const, sortable: false },
  { name: 'actions', label: 'Thao tác', field: 'actions', align: 'right' as const, sortable: false }
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
