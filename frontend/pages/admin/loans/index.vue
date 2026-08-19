<template>
  <div class="admin-page">
    <div class="admin-page-header">
      <div>
        <div class="admin-page-kicker">Tín dụng tiêu dùng</div>
        <div class="admin-page-title">Phê duyệt hồ sơ vay</div>
        <div class="admin-page-subtitle">Kiểm tra hồ sơ khách hàng, duyệt giải ngân hoặc từ chối theo quy trình vận hành.</div>
      </div>
      <q-btn unelevated color="primary" icon="refresh" label="Cập nhật" class="admin-action-btn" :loading="isLoading" @click="fetchApplications" />
    </div>

    <AppAlert v-if="errorMessage" type="error" :message="errorMessage" class="q-mb-md" @dismiss="errorMessage = ''" />
    <AppAlert v-if="successMessage" type="success" :message="successMessage" class="q-mb-md" @dismiss="successMessage = ''" />

    <q-card flat class="admin-filter-card q-mb-lg">
      <q-card-section class="q-pa-lg">
        <div class="row q-col-gutter-md items-end">
          <div class="col-12 col-md-5">
            <div class="admin-field-label">Tìm hồ sơ</div>
            <q-input v-model="filters.keyword" outlined dense bg-color="grey-1" clearable placeholder="Mã hồ sơ, khách hàng, tài khoản..." @keyup.enter="fetchApplications">
              <template #prepend>
                <q-icon name="search" color="primary" />
              </template>
            </q-input>
          </div>
          <div class="col-12 col-md-3">
            <div class="admin-field-label">Trạng thái</div>
            <q-select
              v-model="filters.status"
              outlined
              dense
              emit-value
              map-options
              bg-color="grey-1"
              :options="statusOptions"
              @update:model-value="fetchApplications"
            />
          </div>
          <div class="col-12 col-md-2">
            <q-btn unelevated color="primary" icon="filter_alt" label="Lọc" class="admin-action-btn full-width" @click="fetchApplications" />
          </div>
        </div>
      </q-card-section>
    </q-card>

    <q-card flat class="admin-card admin-table-card">
      <q-table
        class="premium-table"
        :rows="applications"
        :columns="columns"
        row-key="id"
        :loading="isLoading"
        v-model:pagination="pagination"
        @request="onRequest"
        flat
        :rows-per-page-options="[10, 20, 50]"
      >
        <template #body-cell-applicationNumber="props">
          <q-td :props="props">
            <div class="text-weight-bold text-dark">{{ props.row.applicationNumber }}</div>
            <div class="text-caption text-grey-7">{{ formatDateTime(props.row.submittedAtUtc) }}</div>
          </q-td>
        </template>

        <template #body-cell-customer="props">
          <q-td :props="props">
            <div class="text-weight-bold">{{ props.row.customerFullName }}</div>
            <div class="text-caption text-grey-7">{{ props.row.customerCode }}</div>
          </q-td>
        </template>

        <template #body-cell-requestedAmount="props">
          <q-td :props="props" class="text-right text-weight-bold text-primary">
            {{ formatCurrency(props.row.requestedAmount, props.row.currency) }}
          </q-td>
        </template>

        <template #body-cell-monthlyIncome="props">
          <q-td :props="props" class="text-right">
            {{ formatCurrency(props.row.monthlyIncome, props.row.currency) }}
          </q-td>
        </template>

        <template #body-cell-status="props">
          <q-td :props="props">
            <q-chip :color="statusColor(props.row.status) + '-1'" :text-color="statusColor(props.row.status)" size="sm" class="admin-chip">
              {{ statusLabel(props.row.status) }}
            </q-chip>
          </q-td>
        </template>

        <template #body-cell-actions="props">
          <q-td :props="props" class="text-right">
            <q-btn
              v-if="isPending(props.row.status)"
              unelevated
              round
              color="positive"
              icon="check"
              size="sm"
              class="q-mr-sm"
              @click="openApproveDialog(props.row)"
            >
              <q-tooltip>Duyệt giải ngân</q-tooltip>
            </q-btn>
            <q-btn
              v-if="isPending(props.row.status)"
              unelevated
              round
              color="negative"
              icon="close"
              size="sm"
              @click="openRejectDialog(props.row)"
            >
              <q-tooltip>Từ chối</q-tooltip>
            </q-btn>
          </q-td>
        </template>

        <template #no-data>
          <div class="full-width row flex-center text-grey-5 q-pa-xl">
            <div class="text-center">
              <q-icon size="4em" name="request_quote" class="q-mb-md opacity-30" />
              <div class="text-h6 text-weight-medium">Không có hồ sơ vay</div>
              <div class="text-caption">Các hồ sơ khách hàng nộp sẽ xuất hiện tại đây.</div>
            </div>
          </div>
        </template>
      </q-table>
    </q-card>

    <q-dialog v-model="approveDialog">
      <q-card class="loan-dialog-card">
        <q-card-section>
          <div class="text-h6 text-weight-bold">Duyệt giải ngân</div>
          <div class="text-grey-7 q-mt-xs">{{ selectedApplication?.applicationNumber }} · {{ selectedApplication?.customerFullName }}</div>
        </q-card-section>
        <q-card-section class="q-pt-none">
          <q-input v-model.number="approveForm.approvedAmount" outlined type="number" suffix="VND" label="Số tiền duyệt" class="q-mb-md" />
          <q-input v-model.number="approveForm.annualInterestRate" outlined type="number" suffix="%/năm" label="Lãi suất năm" class="q-mb-md" />
          <q-input v-model="approveForm.reviewNote" outlined type="textarea" autogrow maxlength="500" label="Ghi chú phê duyệt" />
        </q-card-section>
        <q-card-actions align="right" class="q-pa-md">
          <q-btn flat color="grey-7" label="Hủy" v-close-popup />
          <q-btn unelevated color="positive" icon="check" label="Duyệt và giải ngân" :loading="isSaving" @click="approveApplication" />
        </q-card-actions>
      </q-card>
    </q-dialog>

    <q-dialog v-model="rejectDialog">
      <q-card class="loan-dialog-card">
        <q-card-section>
          <div class="text-h6 text-weight-bold">Từ chối hồ sơ</div>
          <div class="text-grey-7 q-mt-xs">{{ selectedApplication?.applicationNumber }} · {{ selectedApplication?.customerFullName }}</div>
        </q-card-section>
        <q-card-section class="q-pt-none">
          <q-input v-model="rejectReason" outlined type="textarea" autogrow maxlength="500" label="Lý do từ chối" />
        </q-card-section>
        <q-card-actions align="right" class="q-pa-md">
          <q-btn flat color="grey-7" label="Hủy" v-close-popup />
          <q-btn unelevated color="negative" icon="close" label="Từ chối" :loading="isSaving" :disable="!rejectReason.trim()" @click="rejectApplication" />
        </q-card-actions>
      </q-card>
    </q-dialog>
  </div>
</template>

<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import AppAlert from '~/components/common/AppAlert.vue'
import { useAdminLoanService } from '~/services/adminLoanService'
import type { LoanApplicationDto } from '~/types/loan'
import { formatCurrency } from '~/utils/currency'
import { formatDateTime } from '~/utils/date'

definePageMeta({
  layout: 'admin',
  middleware: ['admin']
})

const loanService = useAdminLoanService()

const applications = ref<LoanApplicationDto[]>([])
const selectedApplication = ref<LoanApplicationDto | null>(null)
const isLoading = ref(false)
const isSaving = ref(false)
const errorMessage = ref('')
const successMessage = ref('')
const approveDialog = ref(false)
const rejectDialog = ref(false)
const rejectReason = ref('')

const filters = reactive({
  keyword: '',
  status: ''
})

const approveForm = reactive({
  approvedAmount: 0,
  annualInterestRate: 12.5,
  reviewNote: ''
})

const pagination = ref({
  page: 1,
  rowsPerPage: 10,
  rowsNumber: 0
})

const statusOptions = [
  { label: 'Tất cả trạng thái', value: '' },
  { label: 'Chờ duyệt', value: 'Pending' },
  { label: 'Đã giải ngân', value: 'Disbursed' },
  { label: 'Từ chối', value: 'Rejected' }
]

const columns = [
  { name: 'applicationNumber', label: 'Mã hồ sơ', field: 'applicationNumber', align: 'left' as const },
  { name: 'customer', label: 'Khách hàng', field: 'customerFullName', align: 'left' as const },
  { name: 'disbursementAccountNumber', label: 'Tài khoản nhận', field: 'disbursementAccountNumber', align: 'left' as const },
  { name: 'requestedAmount', label: 'Số tiền vay', field: 'requestedAmount', align: 'right' as const },
  { name: 'termMonths', label: 'Kỳ hạn', field: (row: LoanApplicationDto) => `${row.termMonths} tháng`, align: 'center' as const },
  { name: 'monthlyIncome', label: 'Thu nhập', field: 'monthlyIncome', align: 'right' as const },
  { name: 'status', label: 'Trạng thái', field: 'status', align: 'center' as const },
  { name: 'actions', label: '', field: 'actions', align: 'right' as const }
]

const fetchApplications = async (props?: any) => {
  isLoading.value = true
  errorMessage.value = ''

  if (props?.pagination) {
    pagination.value.page = props.pagination.page
    pagination.value.rowsPerPage = props.pagination.rowsPerPage
  }

  try {
    const result = await loanService.getApplications({
      page: pagination.value.page,
      pageSize: pagination.value.rowsPerPage,
      status: filters.status || undefined,
      keyword: filters.keyword || undefined
    })
    applications.value = result.items
    pagination.value.rowsNumber = result.totalItems
  } catch (e: any) {
    errorMessage.value = e?.data?.message || e?.message || 'Không thể tải danh sách hồ sơ vay.'
  } finally {
    isLoading.value = false
  }
}

const onRequest = (props: any) => {
  fetchApplications(props)
}

const openApproveDialog = (application: LoanApplicationDto) => {
  selectedApplication.value = application
  approveForm.approvedAmount = application.requestedAmount
  approveForm.annualInterestRate = application.annualInterestRate || 12.5
  approveForm.reviewNote = ''
  approveDialog.value = true
}

const openRejectDialog = (application: LoanApplicationDto) => {
  selectedApplication.value = application
  rejectReason.value = ''
  rejectDialog.value = true
}

const approveApplication = async () => {
  if (!selectedApplication.value) return

  isSaving.value = true
  errorMessage.value = ''
  successMessage.value = ''

  try {
    await loanService.approve(selectedApplication.value.id, {
      approvedAmount: Number(approveForm.approvedAmount),
      annualInterestRate: Number(approveForm.annualInterestRate),
      reviewNote: approveForm.reviewNote || null
    })
    approveDialog.value = false
    successMessage.value = `Đã duyệt và giải ngân hồ sơ ${selectedApplication.value.applicationNumber}.`
    await fetchApplications()
  } catch (e: any) {
    errorMessage.value = e?.data?.message || e?.message || 'Không thể duyệt hồ sơ vay.'
  } finally {
    isSaving.value = false
  }
}

const rejectApplication = async () => {
  if (!selectedApplication.value || !rejectReason.value.trim()) return

  isSaving.value = true
  errorMessage.value = ''
  successMessage.value = ''

  try {
    await loanService.reject(selectedApplication.value.id, { reason: rejectReason.value.trim() })
    rejectDialog.value = false
    successMessage.value = `Đã từ chối hồ sơ ${selectedApplication.value.applicationNumber}.`
    await fetchApplications()
  } catch (e: any) {
    errorMessage.value = e?.data?.message || e?.message || 'Không thể từ chối hồ sơ vay.'
  } finally {
    isSaving.value = false
  }
}

const isPending = (status: string) => status === 'Pending' || status === 'PENDING'

const statusColor = (status: string) => {
  switch (status) {
    case 'Pending':
    case 'PENDING':
      return 'warning'
    case 'Disbursed':
    case 'DISBURSED':
      return 'positive'
    case 'Rejected':
    case 'REJECTED':
      return 'negative'
    default:
      return 'grey'
  }
}

const statusLabel = (status: string) => {
  switch (status) {
    case 'Pending':
    case 'PENDING':
      return 'Chờ duyệt'
    case 'Disbursed':
    case 'DISBURSED':
      return 'Đã giải ngân'
    case 'Rejected':
    case 'REJECTED':
      return 'Từ chối'
    case 'Cancelled':
    case 'CANCELLED':
      return 'Đã hủy'
    default:
      return status
  }
}

onMounted(fetchApplications)
</script>

<style scoped>
.loan-dialog-card {
  width: min(560px, calc(100vw - 32px));
}
</style>
