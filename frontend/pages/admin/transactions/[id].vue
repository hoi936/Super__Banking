<template>
  <div class="admin-page">
    <div class="admin-page-header">
      <div class="row items-center">
        <q-btn flat round dense icon="arrow_back" @click="router.back()" class="admin-soft-btn q-mr-sm" />
        <div>
          <div class="admin-page-kicker">Giám sát dòng tiền</div>
          <div class="admin-page-title">Chi tiết giao dịch</div>
          <div class="admin-page-subtitle">Kiểm tra số tiền, mã tham chiếu, trạng thái và tài khoản liên quan.</div>
        </div>
      </div>
      <q-btn unelevated color="primary" icon="refresh" label="Làm mới" class="admin-action-btn" @click="fetchTransaction" />
    </div>

    <q-banner v-if="hasError" inline-actions rounded class="bg-red-1 text-negative q-mb-md">
      Có lỗi xảy ra khi tải dữ liệu. {{ errorMessage }}
      <template v-slot:action>
        <q-btn flat label="Thử lại" @click="fetchTransaction" />
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

    <template v-else-if="transaction">
      <div class="admin-detail-grid">
        <div class="col-12 col-md-6">
          <q-card flat class="admin-card admin-detail-card">
            <q-card-section>
              <div class="admin-detail-title">Thông tin chung</div>
              
              <div class="admin-money-panel q-mb-lg">
                <div class="text-center">
                  <div class="text-subtitle1 text-grey-7">Số tiền</div>
                  <div class="text-h3 text-weight-bold" :class="transaction.transactionType === 'FEE' ? 'text-negative' : 'text-primary'">
                    {{ formatCurrency(transaction.amount, transaction.currency) }}
                  </div>
                  <div class="q-mt-sm">
                    <q-badge :color="getStatusColor(transaction.status)" class="admin-chip">
                      {{ getStatusLabel(transaction.status) }}
                    </q-badge>
                  </div>
                </div>
              </div>

              <q-list dense class="admin-kv-list">
                <q-item>
                  <q-item-section>
                    <q-item-label caption>Mã tham chiếu</q-item-label>
                    <q-item-label class="text-weight-medium">{{ transaction.referenceNumber }}</q-item-label>
                  </q-item-section>
                </q-item>
                <q-item>
                  <q-item-section>
                    <q-item-label caption>Loại giao dịch</q-item-label>
                    <q-item-label>
                      <q-badge :color="getTypeColor(transaction.transactionType)" outline class="admin-chip">
                        {{ getTypeLabel(transaction.transactionType) }}
                      </q-badge>
                    </q-item-label>
                  </q-item-section>
                </q-item>
                <q-item>
                  <q-item-section>
                    <q-item-label caption>Nội dung</q-item-label>
                    <q-item-label>{{ transaction.description || 'Không có nội dung' }}</q-item-label>
                  </q-item-section>
                </q-item>
                <q-item>
                  <q-item-section>
                    <q-item-label caption>Ngày tạo</q-item-label>
                    <q-item-label>{{ formatDateTime(transaction.createdAtUtc) }}</q-item-label>
                  </q-item-section>
                </q-item>
                <q-item v-if="transaction.completedAtUtc">
                  <q-item-section>
                    <q-item-label caption>Ngày hoàn thành</q-item-label>
                    <q-item-label>{{ formatDateTime(transaction.completedAtUtc) }}</q-item-label>
                  </q-item-section>
                </q-item>
              </q-list>
            </q-card-section>
          </q-card>
        </div>

        <div class="col-12 col-md-6">
          <div class="row q-col-gutter-md h-100">
            <div class="col-12" v-if="transaction.sourceAccount">
              <q-card flat class="admin-card admin-detail-card">
                <q-card-section>
                  <div class="text-subtitle1 text-weight-bold q-mb-sm text-negative">
                    <q-icon name="arrow_upward" class="q-mr-xs" /> Tài khoản gửi (Nguồn)
                  </div>
                  <q-list dense class="admin-kv-list">
                    <q-item>
                      <q-item-section>
                        <q-item-label caption>Chủ tài khoản (Khách hàng)</q-item-label>
                        <q-item-label class="text-weight-medium">
                          {{ transaction.sourceAccount.customerFullName }} 
                          <span class="text-caption text-grey-6">({{ transaction.sourceAccount.customerCode }})</span>
                        </q-item-label>
                      </q-item-section>
                    </q-item>
                    <q-item>
                      <q-item-section>
                        <q-item-label caption>Số tài khoản</q-item-label>
                        <q-item-label>{{ transaction.sourceAccount.accountNumber }}</q-item-label>
                      </q-item-section>
                    </q-item>
                  </q-list>
                </q-card-section>
              </q-card>
            </div>

            <div class="col-12" v-if="transaction.destinationAccount">
              <q-card flat class="admin-card admin-detail-card">
                <q-card-section>
                  <div class="text-subtitle1 text-weight-bold q-mb-sm text-positive">
                    <q-icon name="arrow_downward" class="q-mr-xs" /> Tài khoản nhận (Đích)
                  </div>
                  <q-list dense class="admin-kv-list">
                    <q-item>
                      <q-item-section>
                        <q-item-label caption>Chủ tài khoản (Khách hàng)</q-item-label>
                        <q-item-label class="text-weight-medium">
                          {{ transaction.destinationAccount.customerFullName }}
                          <span class="text-caption text-grey-6">({{ transaction.destinationAccount.customerCode }})</span>
                        </q-item-label>
                      </q-item-section>
                    </q-item>
                    <q-item>
                      <q-item-section>
                        <q-item-label caption>Số tài khoản</q-item-label>
                        <q-item-label>{{ transaction.destinationAccount.accountNumber }}</q-item-label>
                      </q-item-section>
                    </q-item>
                  </q-list>
                </q-card-section>
              </q-card>
            </div>
            
            <div class="col-12" v-if="!transaction.sourceAccount && !transaction.destinationAccount">
               <q-card flat class="admin-card admin-detail-card h-100 flex flex-center">
                  <q-card-section class="text-center text-grey-6">
                    <q-icon name="info" size="2em" />
                    <div class="q-mt-sm">Giao dịch này không liên kết với tài khoản hệ thống.</div>
                  </q-card-section>
               </q-card>
            </div>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAdminTransactionService } from '~/services/adminTransactionService'
import type { AdminTransactionDetailDto } from '~/types/adminTransaction'
import { formatCurrency } from '~/utils/currency'
import { formatDateTime } from '~/utils/date'
import { getTransactionStatusColor as getStatusColor, getTransactionStatusLabel as getStatusLabel } from '~/utils/status'

definePageMeta({
  layout: 'admin',
  middleware: ['admin']
})

const route = useRoute()
const router = useRouter()
const transactionService = useAdminTransactionService()

const transactionId = route.params.id as string
const transaction = ref<AdminTransactionDetailDto | null>(null)
const isLoading = ref(true)
const hasError = ref(false)
const errorMessage = ref('')

const fetchTransaction = async () => {
  isLoading.value = true
  hasError.value = false
  
  try {
    transaction.value = await transactionService.getTransaction(transactionId)
  } catch (error: any) {
    console.error('Lỗi lấy chi tiết giao dịch:', error)
    hasError.value = true
    errorMessage.value = error.data?.title || 'Không thể lấy thông tin giao dịch'
  } finally {
    isLoading.value = false
  }
}

const getTypeColor = (type: string) => {
  switch (type) {
    case 'TRANSFER': return 'primary'
    case 'PAYMENT': return 'orange'
    case 'FEE': return 'red'
    default: return 'grey'
  }
}

const getTypeLabel = (type: string) => {
  switch (type) {
    case 'TRANSFER': return 'Chuyển khoản'
    case 'PAYMENT': return 'Thanh toán'
    case 'FEE': return 'Phí dịch vụ'
    default: return type
  }
}

onMounted(() => {
  fetchTransaction()
})
</script>

<style scoped>
.h-100 {
  height: 100%;
}
</style>
