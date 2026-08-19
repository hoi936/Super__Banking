<template>
  <div class="bank-page-narrow">
    <div class="bank-page-header">
      <div>
        <div class="bank-page-kicker">Thông tin cá nhân</div>
        <div class="bank-page-title">Hồ sơ cá nhân</div>
        <div class="bank-page-subtitle">Cập nhật thông tin liên hệ và dữ liệu định danh dùng trong hệ thống.</div>
      </div>
    </div>

    <AppAlert v-if="hasError" type="error" :message="errorMessage" class="q-mb-md" />

    <q-card flat class="bank-card">
      <q-card-section v-if="isLoading" class="q-pa-lg">
        <q-skeleton type="rect" height="300px" />
      </q-card-section>
      
      <q-card-section v-else-if="profile" class="q-pa-lg">
        <q-form @submit="onSubmit" class="q-gutter-md">
          
          <div class="row q-col-gutter-md">
            <div class="col-12 col-md-6">
              <q-input
                v-model="profile.customerCode" 
                label="Mã khách hàng" 
                readonly 
                outlined
                hint="Không thể thay đổi"
              />
            </div>
            <div class="col-12 col-md-6">
              <q-input
                v-model="profile.status" 
                label="Trạng thái tài khoản" 
                readonly 
                outlined
                hint="Không thể thay đổi"
              />
            </div>
          </div>

          <q-input 
            v-model="formData.fullName" 
            label="Họ và tên *" 
            outlined 
            :readonly="!isEditing"
            :rules="[val => !!val || 'Vui lòng nhập họ tên']"
          />

          <div class="row q-col-gutter-md">
            <div class="col-12 col-md-6">
              <q-input 
                v-model="formData.dateOfBirth" 
                label="Ngày sinh" 
                outlined 
                :readonly="!isEditing"
                type="date"
              />
            </div>
            <div class="col-12 col-md-6">
              <q-select 
                popup-content-class="text-dark bg-white shadow-2"
                options-selected-class="text-primary text-weight-bold"
                v-model="formData.gender" 
                :options="['Male', 'Female', 'Other']" 
                label="Giới tính" 
                outlined 
                :readonly="!isEditing"
              />
            </div>
          </div>

          <q-input 
            v-model="formData.phoneNumber" 
            label="Số điện thoại" 
            outlined 
            :readonly="!isEditing"
            mask="#### ### ###"
            unmasked-value
          />

          <q-input 
            v-model="formData.address" 
            label="Địa chỉ" 
            outlined 
            :readonly="!isEditing"
            type="textarea"
            rows="3"
          />

          <div class="row justify-end q-mt-lg">
            <template v-if="!isEditing">
              <q-btn color="primary" label="Chỉnh sửa hồ sơ" icon="edit" class="bank-action-btn" @click="startEditing" />
            </template>
            <template v-else>
              <q-btn flat color="grey-8" label="Hủy" class="q-mr-sm" @click="cancelEditing" :disable="isSaving" />
              <q-btn color="primary" label="Lưu thay đổi" type="submit" :loading="isSaving" icon="save" class="bank-action-btn" />
            </template>
          </div>
          
        </q-form>
      </q-card-section>
    </q-card>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useQuasar } from 'quasar'
import { useCustomerService } from '~/services/customerService'
import type { CustomerProfile } from '~/types/customer'
import AppAlert from '~/components/common/AppAlert.vue'
import { useAuthStore } from '~/stores/auth'

definePageMeta({
  middleware: ['customer']
})

const $q = useQuasar()
const customerService = useCustomerService()
const authStore = useAuthStore()

const profile = ref<CustomerProfile | null>(null)
const formData = ref<Partial<CustomerProfile>>({})
const isLoading = ref(true)
const isSaving = ref(false)
const isEditing = ref(false)
const hasError = ref(false)
const errorMessage = ref('')

const fetchProfile = async () => {
  isLoading.value = true
  hasError.value = false
  
  try {
    const data = await customerService.getProfile()
    profile.value = data
    formData.value = {
      fullName: data.fullName,
      dateOfBirth: data.dateOfBirth,
      gender: data.gender,
      phoneNumber: data.phoneNumber,
      address: data.address
    }
    // Also update auth store user name if it changed
    if (authStore.user && data.fullName) {
      authStore.user.fullName = data.fullName
    }
  } catch (e: any) {
    hasError.value = true
    errorMessage.value = e?.message || 'Lỗi tải hồ sơ cá nhân.'
  } finally {
    isLoading.value = false
  }
}

onMounted(() => {
  fetchProfile()
})

const startEditing = () => {
  if (profile.value) {
    formData.value = {
      fullName: profile.value.fullName,
      dateOfBirth: profile.value.dateOfBirth,
      gender: profile.value.gender,
      phoneNumber: profile.value.phoneNumber,
      address: profile.value.address
    }
    isEditing.value = true
  }
}

const cancelEditing = () => {
  isEditing.value = false
  if (profile.value) {
    formData.value = {
      fullName: profile.value.fullName,
      dateOfBirth: profile.value.dateOfBirth,
      gender: profile.value.gender,
      phoneNumber: profile.value.phoneNumber,
      address: profile.value.address
    }
  }
}

const onSubmit = async () => {
  isSaving.value = true
  hasError.value = false
  
  try {
    await customerService.updateProfile(formData.value)
    
    $q.notify({
      color: 'positive',
      message: 'Cập nhật thông tin thành công',
      icon: 'check',
      position: 'top-right'
    })
    
    isEditing.value = false
    await fetchProfile() // Refresh data
  } catch (e: any) {
    $q.notify({
      color: 'negative',
      message: e?.message || 'Có lỗi xảy ra khi lưu thay đổi',
      position: 'top-right'
    })
  } finally {
    isSaving.value = false
  }
}
</script>

<style scoped>
</style>
