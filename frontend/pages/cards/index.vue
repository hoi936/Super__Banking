<template>
  <div class="bank-page">
    <div class="bank-page-header">
      <div>
        <div class="bank-page-kicker">Quản lý thẻ</div>
        <div class="bank-page-title">Thẻ tín dụng & ghi nợ</div>
        <div class="bank-page-subtitle">Theo dõi trạng thái, khóa/mở khẩn cấp và thiết lập hạn mức giao dịch.</div>
      </div>
      <q-btn unelevated color="primary" icon="refresh" label="Cập nhật" class="bank-action-btn" :loading="isLoading" @click="fetchCards" />
    </div>

    <AppAlert v-if="errorMessage" type="error" :message="errorMessage" class="q-mb-md" @dismiss="errorMessage = ''" />
    <AppAlert v-if="successMessage" type="success" :message="successMessage" class="q-mb-md" @dismiss="successMessage = ''" />

    <div v-if="isLoading" class="row q-col-gutter-lg">
      <div v-for="i in 2" :key="i" class="col-12 col-md-6">
        <q-card flat class="bank-card">
          <q-card-section><q-skeleton type="rect" height="220px" /></q-card-section>
        </q-card>
      </div>
    </div>

    <div v-else-if="cards.length === 0" class="empty-state">
      <q-icon name="credit_card" size="64px" color="grey-4" />
      <div class="text-h6 text-grey-7 q-mt-md">Chưa có thẻ</div>
    </div>

    <div v-else class="row q-col-gutter-lg">
      <div class="col-12 col-lg-7">
        <div class="cards-grid">
          <button
            v-for="card in cards"
            :key="card.id"
            class="card-3d"
            :class="[card.cardType === 'CREDIT' ? 'card-credit' : 'card-debit', { 'card-selected': selectedCard?.id === card.id, 'card-locked': card.status === 'LOCKED' }]"
            @click="selectCard(card)"
          >
            <span class="card-top">
              <span>{{ card.cardType === 'CREDIT' ? 'Super Credit' : 'Super Debit' }}</span>
              <q-icon :name="card.status === 'LOCKED' ? 'lock' : 'contactless'" size="28px" />
            </span>
            <span class="card-number">{{ card.cardNumberMasked }}</span>
            <span class="card-bottom">
              <span>
                <small>Chủ thẻ</small>
                <strong>{{ card.cardholderName }}</strong>
              </span>
              <span>
                <small>Hết hạn</small>
                <strong>{{ pad(card.expiryMonth) }}/{{ String(card.expiryYear).slice(-2) }}</strong>
              </span>
            </span>
          </button>
        </div>
      </div>

      <div class="col-12 col-lg-5">
        <q-card v-if="selectedCard" flat class="bank-card">
          <q-card-section class="q-pa-lg">
            <div class="row items-center justify-between q-mb-lg">
              <div>
                <div class="panel-label">Thẻ đang chọn</div>
                <div class="panel-title">•••• {{ selectedCard.lastFourDigits }}</div>
              </div>
              <q-badge :color="statusColor(selectedCard.status)" class="bank-chip">{{ statusLabel(selectedCard.status) }}</q-badge>
            </div>

            <div class="info-list">
              <div>
                <span>Tài khoản liên kết</span>
                <strong>{{ selectedCard.linkedAccountNumber }}</strong>
              </div>
              <div>
                <span>Hạn mức ngày</span>
                <strong>{{ formatCurrency(selectedCard.dailyLimit, selectedCard.currency) }}</strong>
              </div>
              <div>
                <span>Hạn mức tháng</span>
                <strong>{{ formatCurrency(selectedCard.monthlyLimit, selectedCard.currency) }}</strong>
              </div>
            </div>

            <q-separator class="q-my-lg" />

            <div class="settings-list">
              <q-toggle
                :model-value="selectedCard.onlinePaymentEnabled"
                color="primary"
                label="Thanh toán online"
                :disable="isSaving || isInactiveCard"
                @update:model-value="value => updateSettings({ onlinePaymentEnabled: value, contactlessEnabled: selectedCard!.contactlessEnabled })"
              />
              <q-toggle
                :model-value="selectedCard.contactlessEnabled"
                color="primary"
                label="Contactless"
                :disable="isSaving || isInactiveCard"
                @update:model-value="value => updateSettings({ onlinePaymentEnabled: selectedCard!.onlinePaymentEnabled, contactlessEnabled: value })"
              />
            </div>

            <div class="row q-col-gutter-sm q-mt-lg">
              <div class="col-12 col-sm-6">
                <q-btn
                  v-if="selectedCard.status === 'LOCKED'"
                  unelevated
                  color="positive"
                  icon="lock_open"
                  label="Mở thẻ"
                  class="full-width bank-action-btn"
                  :loading="isSaving"
                  @click="unlockSelectedCard"
                />
                <q-btn
                  v-else
                  unelevated
                  color="negative"
                  icon="lock"
                  label="Khóa thẻ"
                  class="full-width bank-action-btn"
                  :loading="isSaving"
                  :disable="isInactiveCard"
                  @click="lockSelectedCard"
                />
              </div>
              <div class="col-12 col-sm-6">
                <q-btn
                  outline
                  color="primary"
                  icon="tune"
                  label="Hạn mức"
                  class="full-width bank-action-btn"
                  :disable="isInactiveCard"
                  @click="openLimitDialog"
                />
              </div>
            </div>
          </q-card-section>
        </q-card>
      </div>
    </div>

    <q-dialog v-model="limitDialog">
      <q-card class="limit-card">
        <q-card-section>
          <div class="text-h6 text-weight-bold">Cập nhật hạn mức</div>
        </q-card-section>
        <q-card-section class="q-pt-none">
          <q-input v-model.number="limitForm.dailyLimit" outlined type="number" suffix="VND" label="Hạn mức ngày" class="q-mb-md" />
          <q-input v-model.number="limitForm.monthlyLimit" outlined type="number" suffix="VND" label="Hạn mức tháng" />
        </q-card-section>
        <q-card-actions align="right" class="q-pa-md">
          <q-btn flat color="grey-7" label="Hủy" v-close-popup />
          <q-btn unelevated color="primary" label="Lưu" :loading="isSaving" @click="saveLimits" />
        </q-card-actions>
      </q-card>
    </q-dialog>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import AppAlert from '~/components/common/AppAlert.vue'
import { useCardService } from '~/services/cardService'
import type { BankCard, UpdateCardSettingsRequest } from '~/types/card'
import { formatCurrency } from '~/utils/currency'

definePageMeta({
  middleware: ['customer']
})

const cardService = useCardService()
const cards = ref<BankCard[]>([])
const selectedCard = ref<BankCard | null>(null)
const isLoading = ref(true)
const isSaving = ref(false)
const errorMessage = ref('')
const successMessage = ref('')
const limitDialog = ref(false)
const limitForm = reactive({
  dailyLimit: 0,
  monthlyLimit: 0
})

const isInactiveCard = computed(() =>
  !selectedCard.value || ['BLOCKED', 'EXPIRED', 'CANCELLED'].includes(selectedCard.value.status)
)

const fetchCards = async () => {
  isLoading.value = true
  errorMessage.value = ''

  try {
    cards.value = await cardService.getCards()
    selectedCard.value = cards.value.find(card => card.id === selectedCard.value?.id) || cards.value[0] || null
  } catch (e: any) {
    errorMessage.value = e?.data?.message || e?.message || 'Không thể tải danh sách thẻ.'
  } finally {
    isLoading.value = false
  }
}

const selectCard = (card: BankCard) => {
  selectedCard.value = card
}

const replaceCard = (updated: BankCard) => {
  cards.value = cards.value.map(card => card.id === updated.id ? updated : card)
  selectedCard.value = updated
}

const lockSelectedCard = async () => {
  if (!selectedCard.value) return
  await runCardAction(async () => cardService.lockCard(selectedCard.value!.id), 'Đã khóa thẻ.')
}

const unlockSelectedCard = async () => {
  if (!selectedCard.value) return
  await runCardAction(async () => cardService.unlockCard(selectedCard.value!.id), 'Đã mở khóa thẻ.')
}

const updateSettings = async (request: UpdateCardSettingsRequest) => {
  if (!selectedCard.value) return
  await runCardAction(async () => cardService.updateSettings(selectedCard.value!.id, request), 'Đã cập nhật thiết lập thẻ.')
}

const openLimitDialog = () => {
  if (!selectedCard.value) return
  limitForm.dailyLimit = selectedCard.value.dailyLimit
  limitForm.monthlyLimit = selectedCard.value.monthlyLimit
  limitDialog.value = true
}

const saveLimits = async () => {
  if (!selectedCard.value) return

  if (limitForm.monthlyLimit < limitForm.dailyLimit) {
    errorMessage.value = 'Hạn mức tháng phải lớn hơn hoặc bằng hạn mức ngày.'
    return
  }

  await runCardAction(
    async () => cardService.updateLimits(selectedCard.value!.id, {
      dailyLimit: Number(limitForm.dailyLimit),
      monthlyLimit: Number(limitForm.monthlyLimit)
    }),
    'Đã cập nhật hạn mức thẻ.'
  )
  limitDialog.value = false
}

const runCardAction = async (action: () => Promise<BankCard>, message: string) => {
  isSaving.value = true
  errorMessage.value = ''
  successMessage.value = ''

  try {
    replaceCard(await action())
    successMessage.value = message
  } catch (e: any) {
    errorMessage.value = e?.data?.message || e?.message || 'Không thể cập nhật thẻ.'
  } finally {
    isSaving.value = false
  }
}

const pad = (value: number) => String(value).padStart(2, '0')

const statusColor = (status: string) => {
  switch (status) {
    case 'ACTIVE': return 'positive'
    case 'LOCKED': return 'warning'
    case 'BLOCKED': return 'negative'
    default: return 'grey'
  }
}

const statusLabel = (status: string) => {
  switch (status) {
    case 'ACTIVE': return 'Đang hoạt động'
    case 'LOCKED': return 'Đã khóa'
    case 'BLOCKED': return 'Bị chặn'
    case 'EXPIRED': return 'Hết hạn'
    case 'CANCELLED': return 'Đã hủy'
    default: return status
  }
}

onMounted(fetchCards)
</script>

<style scoped>
.cards-grid {
  display: grid;
  gap: 22px;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
}

.card-3d {
  aspect-ratio: 1.58;
  border: 0;
  border-radius: 18px;
  box-shadow: 0 24px 48px rgba(16, 24, 40, 0.18);
  color: #ffffff;
  cursor: pointer;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  padding: 24px;
  text-align: left;
  transform: perspective(900px) rotateX(0deg) rotateY(-7deg);
  transition: transform 0.18s ease, box-shadow 0.18s ease, outline-color 0.18s ease;
  width: 100%;
}

.card-3d:hover,
.card-selected {
  box-shadow: 0 30px 62px rgba(31, 111, 209, 0.24);
  transform: perspective(900px) rotateX(0deg) rotateY(0deg) translateY(-3px);
  outline: 3px solid rgba(31, 111, 209, 0.28);
}

.card-debit {
  background: linear-gradient(135deg, #1f6fd1, #123a7a);
}

.card-credit {
  background: linear-gradient(135deg, #111827, #475467);
}

.card-locked {
  filter: grayscale(0.65);
}

.card-top,
.card-bottom {
  display: flex;
  justify-content: space-between;
  gap: 16px;
}

.card-top {
  align-items: center;
  font-size: 18px;
  font-weight: 850;
}

.card-number {
  font-size: clamp(22px, 3vw, 30px);
  font-weight: 850;
  letter-spacing: 1px;
}

.card-bottom small {
  color: rgba(255, 255, 255, 0.72);
  display: block;
  font-size: 11px;
  font-weight: 800;
  margin-bottom: 4px;
  text-transform: uppercase;
}

.card-bottom strong {
  font-size: 14px;
}

.panel-label {
  color: #667085;
  font-size: 12px;
  font-weight: 850;
  text-transform: uppercase;
}

.panel-title {
  color: #1d2939;
  font-size: 28px;
  font-weight: 900;
}

.info-list {
  display: grid;
  gap: 12px;
}

.info-list div {
  border: 1px solid #eaecf0;
  border-radius: 8px;
  display: flex;
  justify-content: space-between;
  gap: 14px;
  padding: 14px;
}

.info-list span {
  color: #667085;
  font-weight: 700;
}

.info-list strong {
  color: #1d2939;
  text-align: right;
}

.settings-list {
  display: grid;
  gap: 6px;
}

.empty-state {
  text-align: center;
  padding: 56px 16px;
}

.limit-card {
  width: min(480px, calc(100vw - 32px));
}

@media (max-width: 640px) {
  .cards-grid {
    grid-template-columns: 1fr;
  }

  .info-list div {
    flex-direction: column;
  }

  .info-list strong {
    text-align: left;
  }
}
</style>
