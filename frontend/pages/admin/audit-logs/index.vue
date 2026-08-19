<template>
  <div class="admin-page">
    <div class="admin-page-header">
      <div>
        <div class="admin-page-kicker">Kiểm toán hệ thống</div>
        <div class="admin-page-title">Nhật ký truy cập</div>
        <div class="admin-page-subtitle">Lưu vết hành động, đối tượng tác động, người dùng và địa chỉ IP phục vụ kiểm soát rủi ro.</div>
      </div>
    </div>

    <q-card flat class="admin-filter-card q-mb-lg">
      <q-card-section class="q-pa-lg">
        <div class="row q-col-gutter-md items-center">
          <div class="col-12 col-sm-6 col-md-3">
            <div class="admin-field-label">Tìm kiếm chung</div>
            <q-input 
              v-model="filters.search" 
              outlined 
              dense 
              placeholder="Nhập mô tả, địa chỉ IP..." 
              bg-color="grey-1"
              clearable
              @keyup.enter="fetchAuditLogs"
            >
              <template v-slot:prepend>
                <q-icon name="search" color="primary" />
              </template>
            </q-input>
          </div>
          <div class="col-12 col-sm-6 col-md-2">
            <div class="admin-field-label">Loại hành động</div>
            <q-input 
              v-model="filters.action" 
              outlined 
              dense 
              placeholder="VD: LOGIN, UPDATE..." 
              bg-color="grey-1"
              clearable
              @keyup.enter="fetchAuditLogs"
            >
              <template v-slot:prepend>
                <q-icon name="code" color="grey-7" />
              </template>
            </q-input>
          </div>
          <div class="col-12 col-sm-4 col-md-2">
            <div class="admin-field-label">User ID</div>
            <q-input 
              v-model="filters.userId" 
              outlined 
              dense 
              placeholder="Nhập User ID..." 
              bg-color="grey-1"
              clearable
              @keyup.enter="fetchAuditLogs"
            >
              <template v-slot:prepend>
                <q-icon name="person" color="grey-7" />
              </template>
            </q-input>
          </div>
          <div class="col-12 col-sm-4 col-md-2">
            <div class="admin-field-label">Từ ngày</div>
            <q-input outlined dense v-model="filters.fromDate" type="date" bg-color="grey-1" clearable @change="fetchAuditLogs" />
          </div>
          <div class="col-12 col-sm-4 col-md-2">
            <div class="admin-field-label">Đến ngày</div>
            <q-input outlined dense v-model="filters.toDate" type="date" bg-color="grey-1" clearable @change="fetchAuditLogs" />
          </div>
          <div class="col-12 col-md-1 flex items-center justify-end" style="margin-top: 32px">
             <q-btn unelevated color="primary" icon="refresh" @click="fetchAuditLogs" class="admin-action-btn full-width" padding="8px" />
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
        :rows="auditLogs"
        :columns="columns"
        row-key="id"
        :loading="isLoading"
        v-model:pagination="pagination"
        @request="onRequest"
        flat
        :rows-per-page-options="[20, 50, 100]"
      >
        <template v-slot:body-cell-action="props">
          <q-td :props="props">
            <q-chip color="grey-2" text-color="dark" class="admin-chip font-monospace" size="sm">
              {{ props.row.action }}
            </q-chip>
          </q-td>
        </template>
        
        <template v-slot:body-cell-createdAtUtc="props">
          <q-td :props="props" class="text-grey-8 text-weight-medium">
            {{ formatDateTime(props.row.createdAtUtc) }}
          </q-td>
        </template>

        <template v-slot:body-cell-userId="props">
          <q-td :props="props" class="text-grey-7 font-monospace">
            {{ props.row.userId || '-' }}
          </q-td>
        </template>
        
        <template v-slot:body-cell-entityInfo="props">
          <q-td :props="props">
            <div v-if="props.row.entityType">
              <span class="text-weight-bold text-primary">{{ props.row.entityType }}</span>
              <div class="text-caption text-grey-6 font-monospace">{{ props.row.entityId }}</div>
            </div>
            <span v-else class="text-grey-5">-</span>
          </q-td>
        </template>

        <template v-slot:body-cell-description="props">
          <q-td :props="props">
            <div class="ellipsis text-dark" style="max-width: 350px">
              {{ props.row.description || '-' }}
              <q-tooltip v-if="props.row.description" class="bg-dark text-white text-body2">{{ props.row.description }}</q-tooltip>
            </div>
          </q-td>
        </template>

        <template v-slot:body-cell-ipAddress="props">
          <q-td :props="props">
            <div class="row items-center no-wrap text-grey-8">
              <q-icon name="lan" size="xs" class="q-mr-xs text-grey-5" v-if="props.row.ipAddress" />
              <span class="font-monospace">{{ props.row.ipAddress || '-' }}</span>
            </div>
          </q-td>
        </template>

        <template v-slot:no-data>
          <div class="full-width row flex-center text-grey-5 q-pa-xl">
            <div class="text-center">
              <q-icon size="4em" name="policy" class="q-mb-md opacity-30" />
              <div class="text-h6 text-weight-medium">Không tìm thấy nhật ký</div>
              <div class="text-caption">Chưa có hoạt động nào được lưu vết theo điều kiện tìm kiếm</div>
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
import { useAdminAuditService } from '~/services/adminAuditService'
import type { AuditLogDto } from '~/types/auditLog'
import { formatDateTime } from '~/utils/date'

definePageMeta({
  layout: 'admin',
  middleware: ['admin'] // Checked via specific logic for ADMIN
})

const router = useRouter()
const route = useRoute()
const auditService = useAdminAuditService()

const isLoading = ref(false)
const auditLogs = ref<AuditLogDto[]>([])

const filters = ref({
  search: route.query.search?.toString() || '',
  action: route.query.action?.toString() || '',
  userId: route.query.userId?.toString() || '',
  fromDate: route.query.fromDate?.toString() || '',
  toDate: route.query.toDate?.toString() || ''
})

const pagination = ref({
  page: Number(route.query.page) || 1,
  rowsPerPage: Number(route.query.pageSize) || 20,
  rowsNumber: 0
})

const dateValidationError = computed(() => {
  if (filters.value.fromDate && filters.value.toDate) {
    return new Date(filters.value.fromDate) > new Date(filters.value.toDate)
  }
  return false
})

const columns = [
  { name: 'createdAtUtc', label: 'Thời gian', field: 'createdAtUtc', align: 'left' as const, sortable: false },
  { name: 'action', label: 'Hành động', field: 'action', align: 'left' as const, sortable: false },
  { name: 'userId', label: 'Mã tài khoản (User ID)', field: 'userId', align: 'left' as const, sortable: false },
  { name: 'entityInfo', label: 'Đối tượng tác động', field: 'entityInfo', align: 'left' as const, sortable: false },
  { name: 'description', label: 'Mô tả chi tiết', field: 'description', align: 'left' as const, sortable: false },
  { name: 'ipAddress', label: 'Địa chỉ IP', field: 'ipAddress', align: 'right' as const, sortable: false }
]

const fetchAuditLogs = async (props?: any) => {
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
  if (filters.value.action) query.action = filters.value.action
  if (filters.value.userId) query.userId = filters.value.userId
  if (filters.value.fromDate) query.fromDate = filters.value.fromDate
  if (filters.value.toDate) query.toDate = filters.value.toDate
  router.replace({ query })

  try {
    const result = await auditService.getAuditLogs({
      page: pagination.value.page,
      pageSize: pagination.value.rowsPerPage,
      search: filters.value.search || undefined,
      action: filters.value.action || undefined,
      userId: filters.value.userId || undefined,
      fromDate: filters.value.fromDate ? new Date(filters.value.fromDate).toISOString() : undefined,
      toDate: filters.value.toDate ? new Date(new Date(filters.value.toDate).setHours(23, 59, 59, 999)).toISOString() : undefined
    })
    
    auditLogs.value = result.items
    pagination.value.rowsNumber = result.totalItems
  } catch (error) {
    console.error('Failed to fetch audit logs', error)
  } finally {
    isLoading.value = false
  }
}

const onRequest = (props: any) => {
  fetchAuditLogs(props)
}

onMounted(() => {
  fetchAuditLogs()
})
</script>

<style scoped>
.font-monospace {
  font-family: 'SFMono-Regular', Consolas, 'Liberation Mono', Menlo, monospace;
}
</style>
