<template>
  <div class="q-pa-md">
    <div class="row items-center justify-between q-mb-md">
      <div class="row items-center">
        <q-btn flat round dense icon="arrow_back" @click="router.back()" class="q-mr-sm" />
        <div class="text-h5 text-weight-bold">Chi tiết Khách hàng</div>
      </div>
      <q-btn flat color="primary" icon="refresh" label="Làm mới" @click="fetchCustomerDetail" />
    </div>

    <!-- Error state -->
    <q-banner v-if="hasError" inline-actions rounded class="bg-negative text-white q-mb-md">
      Có lỗi xảy ra khi tải dữ liệu. {{ errorMessage }}
      <template v-slot:action>
        <q-btn flat label="Thử lại" @click="fetchCustomerDetail" />
      </template>
    </q-banner>

    <!-- Skeleton Loading -->
    <div v-if="isLoading">
      <q-card flat bordered class="q-mb-md">
        <q-card-section>
          <q-skeleton type="text" width="30%" class="text-h6" />
          <q-skeleton type="text" width="60%" class="q-mt-md" />
          <q-skeleton type="text" width="50%" />
        </q-card-section>
      </q-card>
    </div>

    <template v-else-if="customer">
      <div class="row q-col-gutter-md">
        <!-- Thông tin cá nhân -->
        <div class="col-12 col-md-6">
          <q-card flat bordered class="h-100">
            <q-card-section>
              <div class="text-h6 q-mb-md">Thông tin cá nhân</div>
              <q-list dense>
                <q-item>
                  <q-item-section>
                    <q-item-label caption>Mã khách hàng</q-item-label>
                    <q-item-label class="text-weight-medium">{{ customer.customerCode }}</q-item-label>
                  </q-item-section>
                </q-item>
                <q-item>
                  <q-item-section>
                    <q-item-label caption>Họ và tên</q-item-label>
                    <q-item-label class="text-weight-medium">{{ customer.fullName }}</q-item-label>
                  </q-item-section>
                </q-item>
                <q-item>
                  <q-item-section>
                    <q-item-label caption>Email</q-item-label>
                    <q-item-label>{{ customer.email }}</q-item-label>
                  </q-item-section>
                </q-item>
                <q-item>
                  <q-item-section>
                    <q-item-label caption>Số điện thoại</q-item-label>
                    <q-item-label>{{ customer.phoneNumber || 'N/A' }}</q-item-label>
                  </q-item-section>
                </q-item>
                <q-item>
                  <q-item-section>
                    <q-item-label caption>Ngày sinh</q-item-label>
                    <q-item-label>{{ formatDate(customer.dateOfBirth || '') }}</q-item-label>
                  </q-item-section>
                </q-item>
                <q-item>
                  <q-item-section>
                    <q-item-label caption>Giới tính</q-item-label>
                    <q-item-label>{{ customer.gender || 'N/A' }}</q-item-label>
                  </q-item-section>
                </q-item>
                <q-item>
                  <q-item-section>
                    <q-item-label caption>Địa chỉ</q-item-label>
                    <q-item-label>{{ customer.address || 'N/A' }}</q-item-label>
                  </q-item-section>
                </q-item>
              </q-list>
            </q-card-section>
          </q-card>
        </div>

        <!-- Trạng thái và Hệ thống -->
        <div class="col-12 col-md-6">
          <q-card flat bordered class="h-100">
            <q-card-section>
              <div class="text-h6 q-mb-md row items-center justify-between">
                <span>Trạng thái hệ thống</span>
                
                <!-- Chỉ hiển thị nút thay đổi với ADMIN -->
                <q-btn
                  v-if="authStore.hasRole('ADMIN')"
                  :color="customer.customerStatus === 'ACTIVE' ? 'negative' : 'positive'"
                  :icon="customer.customerStatus === 'ACTIVE' ? 'lock' : 'lock_open'"
                  :label="customer.customerStatus === 'ACTIVE' ? 'Tạm khóa KH' : 'Mở khóa KH'"
                  size="sm"
                  outline
                  @click="confirmToggleCustomerStatus"
                />
              </div>

              <q-list dense>
                <q-item>
                  <q-item-section>
                    <q-item-label caption>Trạng thái Khách hàng</q-item-label>
                    <q-item-label>
                      <q-badge :color="getStatusColor(customer.customerStatus)">
                        {{ getStatusLabel(customer.customerStatus) }}
                      </q-badge>
                    </q-item-label>
                  </q-item-section>
                </q-item>
                <q-item>
                  <q-item-section>
                    <q-item-label caption>Trạng thái User</q-item-label>
                    <q-item-label>
                      <q-badge :color="getStatusColor(customer.userStatus)" outline>
                        {{ getStatusLabel(customer.userStatus) }}
                      </q-badge>
                    </q-item-label>
                  </q-item-section>
                </q-item>
                <q-item>
                  <q-item-section>
                    <q-item-label caption>Vai trò (Roles)</q-item-label>
                    <q-item-label>
                      <q-chip v-for="role in customer.roles" :key="role" dense color="grey-3">
                        {{ role }}
                      </q-chip>
                    </q-item-label>
                  </q-item-section>
                </q-item>
                <q-item>
                  <q-item-section>
                    <q-item-label caption>Ngày tạo</q-item-label>
                    <q-item-label>{{ formatDateTime(customer.createdAtUtc) }}</q-item-label>
                  </q-item-section>
                </q-item>
                <q-item v-if="customer.updatedAtUtc">
                  <q-item-section>
                    <q-item-label caption>Cập nhật lần cuối</q-item-label>
                    <q-item-label>{{ formatDateTime(customer.updatedAtUtc) }}</q-item-label>
                  </q-item-section>
                </q-item>
              </q-list>
            </q-card-section>
          </q-card>
        </div>
      </div>

      <!-- Danh sách tài khoản -->
      <div class="text-h6 q-mb-md q-mt-lg">Danh sách Tài khoản ({{ customer.accounts.length }})</div>
      <q-card flat bordered>
        <q-table
          :rows="customer.accounts"
          :columns="accountColumns"
          row-key="id"
          flat
          bordered
          hide-pagination
          :pagination="{ rowsPerPage: 0 }"
        >
          <template v-slot:body-cell-balance="props">
            <q-td :props="props" class="text-weight-bold text-primary">
              {{ formatCurrency(props.row.balance, props.row.currency) }}
            </q-td>
          </template>
          
          <template v-slot:body-cell-status="props">
            <q-td :props="props">
              <q-badge :color="props.row.status === 'ACTIVE' ? 'positive' : 'negative'">
                {{ props.row.status === 'ACTIVE' ? 'Hoạt động' : 'Bị khóa' }}
              </q-badge>
            </q-td>
          </template>

          <template v-slot:body-cell-actions="props">
            <q-td :props="props" class="text-right">
              <!-- Nút khóa/mở khóa tài khoản (Chỉ ADMIN) -->
              <q-btn
                v-if="authStore.hasRole('ADMIN')"
                flat
                round
                size="sm"
                :color="props.row.status === 'ACTIVE' ? 'negative' : 'positive'"
                :icon="props.row.status === 'ACTIVE' ? 'lock' : 'lock_open'"
                @click="confirmToggleAccountStatus(props.row)"
              >
                <q-tooltip>{{ props.row.status === 'ACTIVE' ? 'Khóa tài khoản' : 'Mở khóa tài khoản' }}</q-tooltip>
              </q-btn>
            </q-td>
          </template>
        </q-table>
      </q-card>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useQuasar } from 'quasar'
import { useAuthStore } from '~/stores/auth'
import { useAdminCustomerService } from '~/services/adminCustomerService'
import type { AdminCustomerDetailDto } from '~/types/adminCustomer'
import type { AccountSummary } from '~/types/account'
import { formatCurrency } from '~/utils/currency'
import { formatDate, formatDateTime } from '~/utils/date'

definePageMeta({
  layout: 'admin',
  middleware: ['admin']
})

const $q = useQuasar()
const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const customerService = useAdminCustomerService()

const customerId = route.params.id as string
const customer = ref<AdminCustomerDetailDto | null>(null)
const isLoading = ref(true)
const hasError = ref(false)
const errorMessage = ref('')

const accountColumns = [
  { name: 'accountNumber', label: 'Số tài khoản', field: 'accountNumber', align: 'left' as const },
  { name: 'accountName', label: 'Tên tài khoản', field: 'accountName', align: 'left' as const },
  { name: 'accountType', label: 'Loại', field: 'accountType', align: 'left' as const },
  { name: 'balance', label: 'Số dư', field: 'balance', align: 'right' as const },
  { name: 'status', label: 'Trạng thái', field: 'status', align: 'center' as const },
  { name: 'actions', label: 'Thao tác', field: 'actions', align: 'right' as const }
]

const fetchCustomerDetail = async () => {
  isLoading.value = true
  hasError.value = false
  
  try {
    customer.value = await customerService.getCustomer(customerId)
  } catch (error: any) {
    console.error('Lỗi lấy chi tiết khách hàng:', error)
    hasError.value = true
    errorMessage.value = error.data?.title || 'Không thể lấy thông tin khách hàng'
  } finally {
    isLoading.value = false
  }
}

const confirmToggleCustomerStatus = () => {
  if (!customer.value) return
  
  const currentStatus = customer.value.customerStatus
  const newStatus = currentStatus === 'ACTIVE' ? 'SUSPENDED' : 'ACTIVE'
  const actionName = currentStatus === 'ACTIVE' ? 'tạm khóa' : 'mở khóa'

  $q.dialog({
    title: 'Xác nhận',
    message: `Bạn có chắc muốn ${actionName} khách hàng này? Người dùng sẽ bị thay đổi trạng thái tương ứng.`,
    cancel: true,
    persistent: true
  }).onOk(async () => {
    try {
      await customerService.updateCustomerStatus(customerId, { status: newStatus })
      $q.notify({ type: 'positive', message: `Đã ${actionName} khách hàng thành công.` })
      await fetchCustomerDetail()
    } catch (error: any) {
      $q.notify({ type: 'negative', message: error.data?.title || `Lỗi khi ${actionName} khách hàng.` })
    }
  })
}

const confirmToggleAccountStatus = (account: AccountSummary) => {
  const currentStatus = account.status
  const newStatus = currentStatus === 'ACTIVE' ? 'LOCKED' : 'ACTIVE'
  const actionName = currentStatus === 'ACTIVE' ? 'khóa' : 'mở khóa'

  $q.dialog({
    title: 'Xác nhận',
    message: `Bạn có chắc muốn ${actionName} tài khoản ${account.accountNumber}?<br/><br/><span class="text-grey-8">Tài khoản bị khóa sẽ không thể thực hiện chuyển tiền hoặc thanh toán.</span>`,
    html: true,
    cancel: true,
    persistent: true
  }).onOk(async () => {
    try {
      await customerService.updateAccountStatus(account.id, { status: newStatus })
      $q.notify({ type: 'positive', message: `Đã ${actionName} tài khoản thành công.` })
      await fetchCustomerDetail() // Reload toàn bộ để có data mới nhất
    } catch (error: any) {
      $q.notify({ type: 'negative', message: error.data?.title || `Lỗi khi ${actionName} tài khoản.` })
    }
  })
}

const getStatusColor = (status: string) => {
  switch (status) {
    case 'ACTIVE': return 'positive'
    case 'SUSPENDED': return 'negative'
    case 'LOCKED': return 'negative'
    case 'CLOSED': return 'grey-6'
    default: return 'grey'
  }
}

const getStatusLabel = (status: string) => {
  switch (status) {
    case 'ACTIVE': return 'Hoạt động'
    case 'SUSPENDED': return 'Tạm khóa'
    case 'LOCKED': return 'Khóa'
    case 'CLOSED': return 'Đã đóng'
    default: return status
  }
}

onMounted(() => {
  fetchCustomerDetail()
})
</script>

<style scoped>
.h-100 {
  height: 100%;
}
</style>
