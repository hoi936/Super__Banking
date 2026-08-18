<template>
  <div class="q-pa-md">
    <div class="row items-center justify-between q-mb-md">
      <div class="text-h5 text-weight-bold">Audit Logs</div>
    </div>

    <q-card flat bordered class="q-mb-md">
      <q-card-section>
        <div class="row q-col-gutter-md">
          <div class="col-12 col-sm-4 col-md-3">
            <q-input 
              v-model="filters.search" 
              outlined 
              dense 
              placeholder="Mô tả, IP..." 
              clearable
              @keyup.enter="fetchAuditLogs"
            >
              <template v-slot:append>
                <q-icon name="search" @click="fetchAuditLogs" class="cursor-pointer" />
              </template>
            </q-input>
          </div>
          <div class="col-12 col-sm-4 col-md-2">
            <q-input 
              v-model="filters.action" 
              outlined 
              dense 
              placeholder="Hành động (VD: USER_SUSPEND)" 
              clearable
              @keyup.enter="fetchAuditLogs"
            />
          </div>
          <div class="col-12 col-sm-4 col-md-2">
            <q-input 
              v-model="filters.userId" 
              outlined 
              dense 
              placeholder="User ID" 
              clearable
              @keyup.enter="fetchAuditLogs"
            />
          </div>
          <div class="col-12 col-sm-6 col-md-2">
            <q-input outlined dense v-model="filters.fromDate" label="Từ ngày" type="date" clearable @change="fetchAuditLogs" />
          </div>
          <div class="col-12 col-sm-6 col-md-2">
            <q-input outlined dense v-model="filters.toDate" label="Đến ngày" type="date" clearable @change="fetchAuditLogs" />
          </div>
          <div class="col-12 col-md-1 flex items-center justify-end">
             <q-btn color="primary" icon="refresh" label="Lọc" @click="fetchAuditLogs" class="full-width" />
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
        :rows="auditLogs"
        :columns="columns"
        row-key="id"
        :loading="isLoading"
        v-model:pagination="pagination"
        @request="onRequest"
        flat
        bordered
      >
        <template v-slot:body-cell-action="props">
          <q-td :props="props">
            <q-badge color="grey-8" outline>
              {{ props.row.action }}
            </q-badge>
          </q-td>
        </template>
        
        <template v-slot:body-cell-createdAtUtc="props">
          <q-td :props="props">
            {{ formatDateTime(props.row.createdAtUtc) }}
          </q-td>
        </template>
        
        <template v-slot:body-cell-entityInfo="props">
          <q-td :props="props">
            <div v-if="props.row.entityType">
              <span class="text-weight-bold">{{ props.row.entityType }}</span>
              <div class="text-caption text-grey-7">{{ props.row.entityId }}</div>
            </div>
            <span v-else class="text-grey">-</span>
          </q-td>
        </template>

        <template v-slot:body-cell-description="props">
          <q-td :props="props">
            <div class="ellipsis" style="max-width: 300px">
              {{ props.row.description || '-' }}
              <q-tooltip v-if="props.row.description">{{ props.row.description }}</q-tooltip>
            </div>
          </q-td>
        </template>

        <template v-slot:no-data>
          <div class="full-width row flex-center text-accent q-pa-md">
            <q-icon size="2em" name="policy" />
            <span class="q-ml-sm">Không tìm thấy nhật ký phù hợp.</span>
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
  { name: 'action', label: 'Action', field: 'action', align: 'left' as const, sortable: false },
  { name: 'userId', label: 'User ID', field: 'userId', align: 'left' as const, sortable: false },
  { name: 'entityInfo', label: 'Entity (Loại/ID)', field: 'entityInfo', align: 'left' as const, sortable: false },
  { name: 'description', label: 'Mô tả', field: 'description', align: 'left' as const, sortable: false },
  { name: 'ipAddress', label: 'IP Address', field: 'ipAddress', align: 'left' as const, sortable: false }
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
