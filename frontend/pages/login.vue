<template>
  <div class="fullscreen bg-grey-1 flex flex-center">
    <q-card class="my-card shadow-4" style="width: 100%; max-width: 400px">
      <q-card-section class="bg-primary text-white text-center q-pa-lg">
        <div class="text-h5 text-weight-bold">InterLink Banking</div>
        <div class="text-subtitle2">Kết nối tiện ích - Vững bước tương lai</div>
      </q-card-section>

      <q-card-section class="q-pa-md">
        <AppAlert 
          v-if="errorMessage" 
          :message="errorMessage" 
          type="error" 
          @dismiss="errorMessage = ''" 
        />

        <q-form @submit="onSubmit" class="q-gutter-md">
          <q-input
            v-model="email"
            type="email"
            label="Email đăng nhập *"
            outlined
            :rules="[
              val => !!val || 'Email không được để trống',
              val => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(val) || 'Email không hợp lệ'
            ]"
            lazy-rules
          >
            <template v-slot:prepend>
              <q-icon name="email" />
            </template>
          </q-input>

          <q-input
            v-model="password"
            :type="isPwdVisible ? 'text' : 'password'"
            label="Mật khẩu *"
            outlined
            :rules="[val => !!val || 'Mật khẩu không được để trống']"
            lazy-rules
          >
            <template v-slot:prepend>
              <q-icon name="lock" />
            </template>
            <template v-slot:append>
              <q-icon
                :name="isPwdVisible ? 'visibility_off' : 'visibility'"
                class="cursor-pointer"
                @click="isPwdVisible = !isPwdVisible"
              />
            </template>
          </q-input>

          <div>
            <q-btn 
              label="Đăng nhập" 
              type="submit" 
              color="primary" 
              class="full-width q-py-sm" 
              :loading="authStore.isLoading" 
            />
          </div>
        </q-form>
      </q-card-section>

      <q-separator />

      <q-card-section class="q-pa-md bg-grey-2">
        <div class="text-caption text-weight-bold q-mb-sm text-grey-8">
          Tài khoản Demo (Development only)
        </div>
        <div class="text-caption text-grey-7">
          <div><b>Customer:</b> customer1@locallink.local / LocalLink@123</div>
          <div><b>Staff:</b> staff@locallink.local / LocalLink@123</div>
          <div><b>Admin:</b> admin@locallink.local / LocalLink@123</div>
        </div>
      </q-card-section>
    </q-card>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '~/stores/auth'
import AppAlert from '~/components/common/AppAlert.vue'

definePageMeta({
  middleware: ['guest'],
  layout: false // fullscreen login
})

const email = ref('')
const password = ref('')
const isPwdVisible = ref(false)
const errorMessage = ref('')

const authStore = useAuthStore()
const router = useRouter()

const onSubmit = async () => {
  errorMessage.value = ''
  try {
    await authStore.login({
      email: email.value,
      password: password.value
    })

    // Routing based on role
    if (authStore.hasRole('ADMIN') || authStore.hasRole('STAFF')) {
      router.push('/admin')
    } else {
      router.push('/dashboard')
    }
  } catch (error: any) {
    if (error.response && error.response.status === 401) {
      errorMessage.value = 'Email hoặc mật khẩu không chính xác.'
    } else if (error.message === 'fetch failed' || error.message.includes('Network Error')) {
      errorMessage.value = 'Không thể kết nối đến máy chủ.'
    } else {
      errorMessage.value = 'Đã có lỗi xảy ra. Vui lòng thử lại sau.'
    }
  }
}
</script>
