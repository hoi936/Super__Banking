<template>
  <div class="admin-page">
    <div class="admin-page-header">
      <div class="row items-center">
        <q-btn flat round dense icon="arrow_back" @click="router.back()" class="admin-soft-btn q-mr-sm" />
        <div>
          <div class="admin-page-kicker">Nhân sự vận hành</div>
          <div class="admin-page-title">Chi tiết người dùng</div>
          <div class="admin-page-subtitle">Thông tin đăng nhập, vai trò và trạng thái truy cập hệ thống.</div>
        </div>
      </div>
      <q-btn unelevated color="primary" icon="refresh" label="Làm mới" class="admin-action-btn" @click="fetchUserDetail" />
    </div>

    <q-banner v-if="hasError" inline-actions rounded class="bg-red-1 text-negative q-mb-md">
      Có lỗi xảy ra khi tải dữ liệu. {{ errorMessage }}
      <template v-slot:action>
        <q-btn flat label="Thử lại" @click="fetchUserDetail" />
      </template>
    </q-banner>

    <!-- Skeleton Loading -->
    <div v-if="isLoading">
      <q-card flat class="admin-card q-mb-md">
        <q-card-section>
          <q-skeleton type="text" width="30%" class="text-h6" />
          <q-skeleton type="text" width="60%" class="q-mt-md" />
          <q-skeleton type="text" width="50%" />
        </q-card-section>
      </q-card>
    </div>

    <template v-else-if="user">
      <div class="admin-detail-grid">
        <div class="col-12 col-md-6">
          <q-card flat class="admin-card admin-detail-card">
            <q-card-section>
              <div class="admin-detail-title row items-center justify-between">
                <span>Thông tin hệ thống</span>
                
                <q-btn
                  v-if="canManageStatus"
                  :color="user.status === 'ACTIVE' ? 'negative' : 'positive'"
                  :icon="user.status === 'ACTIVE' ? 'person_off' : 'person_add'"
                  :label="user.status === 'ACTIVE' ? 'Tạm khóa' : 'Kích hoạt'"
                  size="sm"
                  unelevated
                  class="admin-action-btn"
                  @click="confirmToggleUserStatus"
                />
                
                <q-btn
                  v-else-if="isSelf"
                  color="grey"
                  icon="person_off"
                  label="Tạm khóa"
                  size="sm"
                  outline
                  disable
                >
                  <q-tooltip>Bạn không thể tạm khóa tài khoản đang đăng nhập</q-tooltip>
                </q-btn>
              </div>

              <q-list dense class="admin-kv-list">
                <q-item>
                  <q-item-section>
                    <q-item-label caption>Email (Đăng nhập)</q-item-label>
                    <q-item-label class="text-weight-medium">{{ user.email }}</q-item-label>
                  </q-item-section>
                </q-item>
                <q-item>
                  <q-item-section>
                    <q-item-label caption>Trạng thái</q-item-label>
                    <q-item-label>
                      <q-badge :color="getStatusColor(user.status)" class="admin-chip">
                        {{ getStatusLabel(user.status) }}
                      </q-badge>
                    </q-item-label>
                  </q-item-section>
                </q-item>
                <q-item>
                  <q-item-section>
                    <q-item-label caption>Vai trò (Roles)</q-item-label>
                    <q-item-label>
                      <q-chip v-for="role in user.roles" :key="role" dense :color="role === 'ADMIN' ? 'red-2' : 'blue-1'" class="admin-chip">
                        {{ role }}
                      </q-chip>
                    </q-item-label>
                  </q-item-section>
                </q-item>
                <q-item>
                  <q-item-section>
                    <q-item-label caption>Ngày tạo tài khoản</q-item-label>
                    <q-item-label>{{ formatDateTime(user.createdAtUtc) }}</q-item-label>
                  </q-item-section>
                </q-item>
                <q-item>
                  <q-item-section>
                    <q-item-label caption>Đăng nhập lần cuối</q-item-label>
                    <q-item-label>
                      <template v-if="user.lastLoginAtUtc">
                        {{ formatDateTime(user.lastLoginAtUtc) }}
                      </template>
                      <span v-else class="text-grey-6 text-italic">Chưa từng đăng nhập</span>
                    </q-item-label>
                  </q-item-section>
                </q-item>
              </q-list>
            </q-card-section>
          </q-card>
        </div>

        <div class="col-12 col-md-6">
          <q-card flat class="admin-card admin-detail-card">
            <q-card-section>
              <div class="admin-detail-title">Thông tin hồ sơ</div>
              
              <template v-if="user.fullName || user.customerCode">
                <q-list dense class="admin-kv-list">
                  <q-item>
                    <q-item-section>
                      <q-item-label caption>Họ và tên</q-item-label>
                      <q-item-label class="text-weight-medium">{{ user.fullName || 'N/A' }}</q-item-label>
                    </q-item-section>
                  </q-item>
                  <q-item v-if="user.customerCode">
                    <q-item-section>
                      <q-item-label caption>Mã khách hàng liên kết</q-item-label>
                      <q-item-label>{{ user.customerCode }}</q-item-label>
                    </q-item-section>
                  </q-item>
                </q-list>
              </template>
              <div v-else class="text-center text-grey-6 q-pa-lg">
                <q-icon name="person_outline" size="3em" />
                <div class="q-mt-sm">Người dùng này không có hồ sơ khách hàng.</div>
                <div class="text-caption">Thường là tài khoản Staff hoặc Admin thuần túy.</div>
              </div>
            </q-card-section>
          </q-card>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useQuasar } from 'quasar'
import { useAuthStore } from '~/stores/auth'
import { useAdminUserService } from '~/services/adminUserService'
import type { AdminUserDetailDto } from '~/types/adminUser'
import { formatDateTime } from '~/utils/date'
import { getUserStatusColor as getStatusColor, getUserStatusLabel as getStatusLabel } from '~/utils/status'

definePageMeta({
  layout: 'admin',
  middleware: ['admin']
})

const $q = useQuasar()
const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const userService = useAdminUserService()

const userId = route.params.id as string
const user = ref<AdminUserDetailDto | null>(null)
const isLoading = ref(true)
const hasError = ref(false)
const errorMessage = ref('')

const isSelf = computed(() => {
  return authStore.user?.id === userId
})

const canManageStatus = computed(() => {
  return authStore.hasRole('ADMIN') && !isSelf.value
})

const fetchUserDetail = async () => {
  isLoading.value = true
  hasError.value = false
  
  try {
    user.value = await userService.getUser(userId)
  } catch (error: any) {
    console.error('Lỗi lấy chi tiết người dùng:', error)
    hasError.value = true
    errorMessage.value = error.data?.title || 'Không thể lấy thông tin người dùng'
  } finally {
    isLoading.value = false
  }
}

const confirmToggleUserStatus = () => {
  if (!user.value) return
  
  const currentStatus = user.value.status
  const newStatus = currentStatus === 'ACTIVE' ? 'SUSPENDED' : 'ACTIVE'
  const actionName = currentStatus === 'ACTIVE' ? 'tạm khóa' : 'kích hoạt'
  
  let msg = `Bạn có chắc muốn ${actionName} tài khoản người dùng này?`
  if (currentStatus === 'ACTIVE') {
    msg += `<br/><br/><span class="text-negative text-weight-medium">Lưu ý: User sẽ không thể đăng nhập hoặc refresh session sau khi bị tạm khóa.</span>`
  }

  $q.dialog({
    title: 'Xác nhận',
    message: msg,
    html: true,
    cancel: true,
    persistent: true
  }).onOk(async () => {
    try {
      await userService.updateUserStatus(userId, { status: newStatus })
      $q.notify({ type: 'positive', message: `Đã ${actionName} tài khoản.` })
      await fetchUserDetail()
    } catch (error: any) {
      $q.notify({ type: 'negative', message: error.data?.title || `Lỗi khi ${actionName} tài khoản.` })
    }
  })
}

onMounted(() => {
  fetchUserDetail()
})
</script>

<style scoped>
.h-100 {
  height: 100%;
}
</style>
