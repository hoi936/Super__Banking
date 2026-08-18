<template>
  <div class="q-pa-md max-width-1000 q-mx-auto">
    <div class="row items-center justify-between q-mb-md">
      <div class="text-h6 text-primary">
        Thông báo
        <q-badge color="negative" rounded class="q-ml-sm" v-if="unreadCount > 0">{{ unreadCount }}</q-badge>
      </div>
      <q-btn v-if="unreadCount > 0" flat color="primary" label="Đánh dấu tất cả đã đọc" icon="done_all" @click="markAllAsRead" :loading="isMarking" />
    </div>

    <!-- Filters -->
    <div class="row q-col-gutter-sm q-mb-md">
      <div class="col-12 col-sm-auto">
        <q-btn-group outline>
          <q-btn :outline="filters.isRead !== undefined" :color="filters.isRead === undefined ? 'primary' : 'grey'" label="Tất cả" @click="setReadFilter(undefined)" />
          <q-btn :outline="filters.isRead !== false" :color="filters.isRead === false ? 'primary' : 'grey'" label="Chưa đọc" @click="setReadFilter(false)" />
        </q-btn-group>
      </div>
      <div class="col-12 col-sm-3">
        <q-select
          v-model="filters.type"
          :options="[
            { label: 'Tất cả loại', value: '' },
            { label: 'Chuyển tiền', value: 'TRANSFER' },
            { label: 'Thanh toán', value: 'PAYMENT' },
            { label: 'Tài khoản', value: 'ACCOUNT' },
            { label: 'Hệ thống', value: 'SYSTEM' },
            { label: 'Bảo mật', value: 'SECURITY' }
          ]"
          emit-value
          map-options
          outlined
          dense
          @update:model-value="applyFilters"
        />
      </div>
    </div>

    <div v-if="isLoading" class="text-center q-pa-xl">
      <q-spinner color="primary" size="3em" />
    </div>

    <div v-else-if="notifications.length === 0" class="text-center text-grey-7 q-pa-xl bg-white rounded-borders border">
      Bạn chưa có thông báo nào.
    </div>

    <q-list v-else bordered separator class="bg-white rounded-borders">
      <q-item 
        v-for="noti in notifications" 
        :key="noti.id" 
        clickable 
        v-ripple 
        :class="noti.isRead ? '' : 'bg-blue-1'"
        @click="handleNotificationClick(noti)"
      >
        <q-item-section avatar>
          <q-avatar :color="getIconColor(noti.type)" text-color="white">
            <q-icon :name="getIconName(noti.type)" />
          </q-avatar>
        </q-item-section>

        <q-item-section>
          <q-item-label :class="noti.isRead ? 'text-weight-medium' : 'text-weight-bold'">{{ noti.title }}</q-item-label>
          <q-item-label caption lines="2">{{ noti.message }}</q-item-label>
        </q-item-section>

        <q-item-section side top>
          <q-item-label caption>{{ formatDateTime(noti.createdAtUtc) }}</q-item-label>
          <q-badge v-if="!noti.isRead" color="negative" rounded class="q-mt-sm" />
        </q-item-section>
      </q-item>
    </q-list>

    <div class="row justify-center q-mt-md" v-if="notifications.length > 0">
      <q-pagination
        v-model="pagination.page"
        :max="pagination.rowsNumber ? Math.ceil(pagination.rowsNumber / pagination.rowsPerPage) : 1"
        @update:model-value="loadData"
        color="primary"
        boundary-links
        max-pages="5"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useQuasar } from 'quasar'
import { formatDateTime } from '~/utils/date'
import { useNotificationService } from '~/services/notificationService'
import type { NotificationDto } from '~/types/notification'
import { useDashboard } from '~/composables/useDashboard'

definePageMeta({
  layout: 'default',
  middleware: ['customer']
})

const router = useRouter()
const $q = useQuasar()
const notificationService = useNotificationService()
const { fetchDashboardData } = useDashboard()

const notifications = ref<NotificationDto[]>([])
const isLoading = ref(true)
const isMarking = ref(false)
const unreadCount = useState<number>('unreadNotificationCount', () => 0)

const filters = ref({
  isRead: undefined as boolean | undefined,
  type: ''
})

const pagination = ref({
  page: 1,
  rowsPerPage: 15,
  rowsNumber: 0
})

onMounted(() => {
  loadData()
  loadUnreadCount()
})

const loadUnreadCount = async () => {
  try {
    const res = await notificationService.getUnreadCount()
    unreadCount.value = res.count
  } catch (error) {}
}

const setReadFilter = (isRead?: boolean) => {
  filters.value.isRead = isRead
  applyFilters()
}

const applyFilters = () => {
  pagination.value.page = 1
  loadData()
}

const loadData = async () => {
  isLoading.value = true
  try {
    const res = await notificationService.getNotifications({
      page: pagination.value.page,
      pageSize: pagination.value.rowsPerPage,
      isRead: filters.value.isRead,
      type: filters.value.type || undefined
    })

    notifications.value = res.items
    pagination.value.rowsNumber = res.totalItems
  } catch (error) {
    console.error(error)
  } finally {
    isLoading.value = false
  }
}

const markAllAsRead = async () => {
  isMarking.value = true
  try {
    await notificationService.markAllAsRead()
    $q.notify({ type: 'positive', message: 'Đã đánh dấu tất cả là đã đọc' })
    await loadData()
    await loadUnreadCount()
    fetchDashboardData() // trigger layout refresh
  } catch (error) {
    $q.notify({ type: 'negative', message: 'Đã có lỗi xảy ra' })
  } finally {
    isMarking.value = false
  }
}

const handleNotificationClick = async (noti: NotificationDto) => {
  if (!noti.isRead) {
    try {
      await notificationService.markAsRead(noti.id)
      noti.isRead = true
      unreadCount.value = Math.max(0, unreadCount.value - 1)
      fetchDashboardData() // trigger layout refresh
    } catch (error) {}
  }
  
  // Optional navigation logic based on type if needed
  if (noti.type === 'TRANSFER' || noti.type === 'PAYMENT') {
    // You could parse the message to find the ID, but backend doesn't return related entity ID easily
    // We'll just leave them on the notifications page for now
  }
}

const getIconName = (type: string) => {
  switch (type) {
    case 'TRANSFER': return 'swap_horiz'
    case 'PAYMENT': return 'receipt'
    case 'ACCOUNT': return 'account_balance_wallet'
    case 'SECURITY': return 'security'
    case 'SYSTEM': return 'info'
    default: return 'notifications'
  }
}

const getIconColor = (type: string) => {
  switch (type) {
    case 'TRANSFER': return 'blue'
    case 'PAYMENT': return 'orange'
    case 'ACCOUNT': return 'green'
    case 'SECURITY': return 'red'
    case 'SYSTEM': return 'grey-7'
    default: return 'primary'
  }
}
</script>

<style scoped>
.max-width-1000 {
  max-width: 1000px;
}
.border {
  border: 1px solid var(--q-grey-3);
}
</style>
