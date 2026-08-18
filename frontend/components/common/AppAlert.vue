<template>
  <q-banner v-if="message" :class="colorClass" rounded class="q-mb-md">
    <template v-slot:avatar>
      <q-icon :name="iconName" :color="iconColor" />
    </template>
    {{ message }}
    <template v-slot:action v-if="dismissible">
      <q-btn flat icon="close" @click="dismiss" />
    </template>
  </q-banner>
</template>

<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps({
  message: {
    type: String,
    default: ''
  },
  type: {
    type: String,
    default: 'error',
    validator: (val: string) => ['error', 'warning', 'info', 'success'].includes(val)
  },
  dismissible: {
    type: Boolean,
    default: true
  }
})

const emit = defineEmits(['dismiss'])

const colorClass = computed(() => {
  switch (props.type) {
    case 'error': return 'bg-red-2 text-red-9'
    case 'warning': return 'bg-orange-2 text-orange-9'
    case 'success': return 'bg-green-2 text-green-9'
    default: return 'bg-blue-2 text-blue-9'
  }
})

const iconName = computed(() => {
  switch (props.type) {
    case 'error': return 'error'
    case 'warning': return 'warning'
    case 'success': return 'check_circle'
    default: return 'info'
  }
})

const iconColor = computed(() => {
  switch (props.type) {
    case 'error': return 'red'
    case 'warning': return 'orange'
    case 'success': return 'green'
    default: return 'blue'
  }
})

const dismiss = () => {
  emit('dismiss')
}
</script>
