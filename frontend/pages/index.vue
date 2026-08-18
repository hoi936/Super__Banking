<template>
  <div class="locallink-container">
    <!-- Header / Navbar -->
    <header class="header">
      <div class="logo-area">
        <div class="logo-icon">
          <q-icon name="account_balance" size="28px" color="primary" />
        </div>
        <div class="logo-text">
          <q-icon name="account_balance" size="32px" class="logo-icon" />
          <span class="logo-title">InterLink Banking</span>
        </div>
        <div class="header-actions flex q-gutter-sm">
          <q-btn
            unelevated
            rounded
            icon="refresh"
            label="Refresh Status"
            color="primary"
            :loading="loading"
            class="refresh-btn"
            @click="checkStatus"
          />
          <q-btn
            v-if="authStore.isAuthenticated"
            unelevated
            rounded
            icon="dashboard"
            label="Vào ứng dụng"
            color="secondary"
            @click="goToApp"
          />
          <q-btn
            v-else
            unelevated
            rounded
            icon="login"
            label="Đăng nhập"
            color="secondary"
            to="/login"
          />
        </div>
      </div>
    </header>

    <main class="main-content">
      <!-- Hero Section -->
      <section class="hero-section text-center">
        <div class="hero-badge">
          <q-icon name="verified_user" size="16px" class="q-mr-xs" />
          Production-Ready Architecture Foundation
        </div>
        <h1 class="hero-title">
          InterLink Banking Platform
        </h1>
        <p class="hero-subtitle">
          Cloud-native Connected Regional Banking & Local Services Platform
        </p>
        <p class="hero-desc">
          High-performance, modular foundation engineered with ASP.NET Core Web API, Entity Framework Core, SQL Server 2022, Nuxt 4, and Docker.
        </p>
      </section>

      <!-- Status Cards Grid -->
      <section class="status-grid">
        <!-- Backend Status Card -->
        <div class="status-card" :class="backendStatusClass">
          <div class="card-header">
            <div class="service-icon backend-icon">
              <q-icon name="dns" size="24px" />
            </div>
            <div class="service-badge">
              <q-badge
                :color="isBackendOnline ? 'positive' : 'negative'"
                :label="isBackendOnline ? 'Connected' : 'Offline'"
                rounded
                class="q-px-sm q-py-xs"
              />
            </div>
          </div>
          <div class="card-body">
            <div class="service-title">Backend API Service</div>
            <div class="service-meta">ASP.NET Core Web API 10.0</div>
            <div class="status-indicator">
              <span class="pulse-indicator" :class="{ online: isBackendOnline }"></span>
              <span class="status-text">
                {{ isBackendOnline ? 'Status: Running' : 'Status: Offline' }}
              </span>
            </div>
          </div>
        </div>

        <!-- Database Status Card -->
        <div class="status-card" :class="dbStatusClass">
          <div class="card-header">
            <div class="service-icon db-icon">
              <q-icon name="storage" size="24px" />
            </div>
            <div class="service-badge">
              <q-badge
                :color="isDbHealthy ? 'positive' : (isBackendOnline ? 'warning' : 'negative')"
                :label="isDbHealthy ? 'Healthy' : (isBackendOnline ? 'Degraded' : 'Offline')"
                rounded
                class="q-px-sm q-py-xs"
              />
            </div>
          </div>
          <div class="card-body">
            <div class="service-title">Database Engine</div>
            <div class="service-meta">Microsoft SQL Server 2022</div>
            <div class="status-indicator">
              <span class="pulse-indicator" :class="{ online: isDbHealthy }"></span>
              <span class="status-text">
                {{ dbStatusLabel }}
              </span>
            </div>
          </div>
        </div>

        <!-- Frontend Web Card -->
        <div class="status-card active-card">
          <div class="card-header">
            <div class="service-icon web-icon">
              <q-icon name="devices" size="24px" />
            </div>
            <div class="service-badge">
              <q-badge color="positive" label="Operational" rounded class="q-px-sm q-py-xs" />
            </div>
          </div>
          <div class="card-body">
            <div class="service-title">Web Application</div>
            <div class="service-meta">Nuxt 4 + Vue 3 + Quasar + TS</div>
            <div class="status-indicator">
              <span class="pulse-indicator online"></span>
              <span class="status-text">Client: Connected</span>
            </div>
          </div>
        </div>
      </section>

      <!-- System Diagnostic Details Card -->
      <section class="details-section">
        <div class="details-card">
          <div class="details-header">
            <div class="details-title-wrap">
              <q-icon name="tune" size="20px" class="q-mr-sm" />
              <h2 class="details-title">System Runtime Telemetry</h2>
            </div>
            <div class="details-actions">
              <span class="last-checked">
                Last checked: {{ lastCheckedFormatted }}
              </span>
            </div>
          </div>

          <div class="telemetry-grid">
            <div class="telemetry-item">
              <span class="telemetry-label">Application</span>
              <span class="telemetry-value">{{ systemData?.application || 'LocalLink' }}</span>
            </div>
            <div class="telemetry-item">
              <span class="telemetry-label">Environment</span>
              <span class="telemetry-value">{{ systemData?.environment || 'Development' }}</span>
            </div>
            <div class="telemetry-item">
              <span class="telemetry-label">API Gateway Base URL</span>
              <span class="telemetry-value code-font">{{ apiBaseUrl }}</span>
            </div>
            <div class="telemetry-item">
              <span class="telemetry-label">Database Target</span>
              <span class="telemetry-value">LocalLinkDb (SQL Server 2022)</span>
            </div>
            <div class="telemetry-item">
              <span class="telemetry-label">Architecture</span>
              <span class="telemetry-value">Modular Monolith + Clean Architecture</span>
            </div>
            <div class="telemetry-item">
              <span class="telemetry-label">Platform Version</span>
              <span class="telemetry-value">v{{ systemData?.version || '1.0.0' }}</span>
            </div>
          </div>

          <!-- Quick Test Actions -->
          <div class="quick-actions">
            <a :href="`${apiBaseUrl}/health`" target="_blank" class="action-link">
              <q-icon name="favorite" size="16px" class="q-mr-xs" />
              Inspect /health Endpoint
            </a>
            <a :href="`${apiBaseUrl}/api/system`" target="_blank" class="action-link">
              <q-icon name="code" size="16px" class="q-mr-xs" />
              Inspect /api/system
            </a>
            <a :href="`${apiBaseUrl}/swagger`" target="_blank" class="action-link">
              <q-icon name="api" size="16px" class="q-mr-xs" />
              OpenAPI / Swagger UI
            </a>
          </div>
        </div>
      </section>

      <!-- Roadmap Preview -->
      <section class="roadmap-section">
        <h3 class="roadmap-title">Platform Development Roadmap</h3>
        <div class="roadmap-grid">
          <div class="roadmap-card completed">
            <div class="roadmap-badge">Milestone 1</div>
            <div class="roadmap-name">Foundation & DevOps</div>
            <div class="roadmap-status">COMPLETED ✅</div>
          </div>
          <div class="roadmap-card completed">
            <div class="roadmap-badge">Milestone 2</div>
            <div class="roadmap-name">Database Schema Design</div>
            <div class="roadmap-status">COMPLETED ✅</div>
          </div>
          <div class="roadmap-card completed">
            <div class="roadmap-badge">Milestone 3</div>
            <div class="roadmap-name">Auth + JWT + RBAC</div>
            <div class="roadmap-status">COMPLETED ✅</div>
          </div>
          <div class="roadmap-card completed">
            <div class="roadmap-badge">Milestone 4</div>
            <div class="roadmap-name">Customer & Banking Accounts</div>
            <div class="roadmap-status">COMPLETED ✅</div>
          </div>
          <div class="roadmap-card completed">
            <div class="roadmap-badge">Milestone 5</div>
            <div class="roadmap-name">Transfer Engine & Integrity</div>
            <div class="roadmap-status">COMPLETED ✅</div>
          </div>
          <div class="roadmap-card completed">
            <div class="roadmap-badge">Milestone 6</div>
            <div class="roadmap-name">Bill Payment & Notifications</div>
            <div class="roadmap-status">COMPLETED ✅</div>
          </div>
        </div>
      </section>
    </main>

    <!-- Footer -->
    <footer class="footer">
      <div>InterLink Banking — Cloud-native Connected Regional Banking & Local Services Platform</div>
      <div class="q-mt-xs">Engineered with Clean Architecture, .NET 10, Nuxt 4 & Docker Orchestration</div>
    </footer>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiClient } from '~/services/api'
import { useAuthStore } from '~/stores/auth'
import type { SystemStatus, HealthCheckResponse } from '~/types'

const router = useRouter()
const authStore = useAuthStore()
const { baseUrl: apiBaseUrl, getSystemStatus, getHealthCheck } = useApiClient()

const goToApp = () => {
  if (authStore.hasRole('ADMIN') || authStore.hasRole('STAFF')) {
    router.push('/admin')
  } else {
    router.push('/dashboard')
  }
}

const loading = ref(false)
const systemData = ref<SystemStatus | null>(null)
const healthData = ref<HealthCheckResponse | null>(null)
const lastChecked = ref<Date | null>(null)
const fetchError = ref<string | null>(null)

const isBackendOnline = computed(() => {
  return systemData.value?.status === 'running' || healthData.value?.status === 'Healthy'
})

const isDbHealthy = computed(() => {
  if (healthData.value?.status === 'Healthy') return true
  if (systemData.value?.database === 'connected') return true
  return false
})

const dbStatusLabel = computed(() => {
  if (isDbHealthy.value) return 'Database: Connected & Ready'
  if (isBackendOnline.value) return 'Database: Connecting...'
  return 'Database: Offline'
})

const backendStatusClass = computed(() => {
  return isBackendOnline.value ? 'status-online' : 'status-offline'
})

const dbStatusClass = computed(() => {
  return isDbHealthy.value ? 'status-online' : (isBackendOnline.value ? 'status-warning' : 'status-offline')
})

const lastCheckedFormatted = computed(() => {
  if (!lastChecked.value) return 'Checking...'
  return lastChecked.value.toLocaleTimeString()
})

const checkStatus = async () => {
  loading.value = true
  fetchError.value = null
  try {
    const [sys, health] = await Promise.allSettled([
      getSystemStatus(),
      getHealthCheck()
    ])

    if (sys.status === 'fulfilled') {
      systemData.value = sys.value
    } else {
      systemData.value = null
    }

    if (health.status === 'fulfilled') {
      healthData.value = health.value
    } else {
      healthData.value = null
    }

    if (sys.status === 'rejected' && health.status === 'rejected') {
      fetchError.value = 'Backend is currently unreachable.'
    }
  } catch (err: any) {
    fetchError.value = err?.message || 'Failed to communicate with API'
  } finally {
    lastChecked.value = new Date()
    loading.value = false
  }
}

onMounted(() => {
  checkStatus()
})
</script>

<style scoped>
.locallink-container {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  background: radial-gradient(circle at 50% 0%, #1e293b 0%, #0b0f19 75%);
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.25rem 2rem;
  border-bottom: 1px solid rgba(255, 255, 255, 0.08);
  backdrop-filter: blur(12px);
  background: rgba(11, 15, 25, 0.7);
  position: sticky;
  top: 0;
  z-index: 100;
}

.logo-area {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.logo-icon {
  width: 44px;
  height: 44px;
  border-radius: 12px;
  background: linear-gradient(135deg, #0284c7 0%, #0369a1 100%);
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 4px 12px rgba(2, 132, 199, 0.35);
}

.logo-text {
  display: flex;
  flex-direction: column;
}

.logo-title {
  font-size: 1.25rem;
  font-weight: 800;
  letter-spacing: -0.02em;
  color: #ffffff;
}

.logo-badge {
  font-size: 0.65rem;
  font-weight: 700;
  letter-spacing: 0.08em;
  color: #38bdf8;
}

.refresh-btn {
  background: rgba(255, 255, 255, 0.06);
  border: 1px solid rgba(255, 255, 255, 0.12);
  border-radius: 8px;
  font-size: 0.85rem;
  padding: 0.4rem 0.85rem;
  transition: all 0.2s ease;
}

.refresh-btn:hover {
  background: rgba(255, 255, 255, 0.12);
}

.main-content {
  flex: 1;
  max-width: 1140px;
  margin: 0 auto;
  padding: 3rem 1.5rem;
  width: 100%;
}

.hero-section {
  text-align: center;
  margin-bottom: 3.5rem;
}

.hero-tag {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.35rem 1rem;
  border-radius: 9999px;
  background: rgba(2, 132, 199, 0.12);
  border: 1px solid rgba(56, 189, 248, 0.25);
  color: #38bdf8;
  font-size: 0.82rem;
  font-weight: 600;
  margin-bottom: 1.25rem;
}

.tag-dot {
  width: 7px;
  height: 7px;
  border-radius: 50%;
  background-color: #38bdf8;
  box-shadow: 0 0 8px #38bdf8;
}

.hero-title {
  font-size: 3rem;
  letter-spacing: -0.03em;
  color: #ffffff;
  margin-bottom: 0.5rem;
  background: linear-gradient(135deg, #ffffff 0%, #cbd5e1 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
}

.hero-subtitle {
  font-size: 1.35rem;
  font-weight: 600;
  color: #38bdf8;
  margin-bottom: 1rem;
}

.hero-desc {
  max-width: 680px;
  margin: 0 auto;
  color: #94a3b8;
  font-size: 1rem;
  line-height: 1.6;
}

.status-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  gap: 1.5rem;
  margin-bottom: 2.5rem;
}

.status-card {
  background: rgba(17, 24, 39, 0.7);
  border: 1px solid rgba(255, 255, 255, 0.08);
  border-radius: 16px;
  padding: 1.5rem;
  backdrop-filter: blur(16px);
  transition: transform 0.2s ease, border-color 0.2s ease, box-shadow 0.2s ease;
}

.status-card:hover {
  transform: translateY(-3px);
  box-shadow: 0 12px 28px rgba(0, 0, 0, 0.35);
}

.status-online {
  border-color: rgba(16, 185, 129, 0.35);
}

.status-warning {
  border-color: rgba(245, 158, 11, 0.35);
}

.status-offline {
  border-color: rgba(239, 68, 68, 0.35);
}

.active-card {
  border-color: rgba(2, 132, 199, 0.35);
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.25rem;
}

.service-icon {
  width: 44px;
  height: 44px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.backend-icon {
  background: rgba(14, 165, 233, 0.15);
  color: #38bdf8;
}

.db-icon {
  background: rgba(16, 185, 129, 0.15);
  color: #34d399;
}

.web-icon {
  background: rgba(168, 85, 247, 0.15);
  color: #c084fc;
}

.service-title {
  font-size: 1.15rem;
  font-weight: 700;
  color: #ffffff;
  margin-bottom: 0.25rem;
}

.service-meta {
  font-size: 0.85rem;
  color: #64748b;
  margin-bottom: 1rem;
}

.status-indicator {
  display: flex;
  align-items: center;
  gap: 0.6rem;
  font-size: 0.875rem;
  color: #cbd5e1;
  font-weight: 500;
}

.pulse-indicator {
  width: 9px;
  height: 9px;
  border-radius: 50%;
  background-color: #ef4444;
  box-shadow: 0 0 6px #ef4444;
}

.pulse-indicator.online {
  background-color: #10b981;
  box-shadow: 0 0 8px #10b981;
}

.details-section {
  margin-bottom: 2.5rem;
}

.details-card {
  background: rgba(17, 24, 39, 0.6);
  border: 1px solid rgba(255, 255, 255, 0.08);
  border-radius: 16px;
  padding: 1.75rem;
}

.details-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
  padding-bottom: 1rem;
  border-bottom: 1px solid rgba(255, 255, 255, 0.06);
}

.details-title-wrap {
  display: flex;
  align-items: center;
  color: #f1f5f9;
}

.details-title {
  font-size: 1.15rem;
  font-weight: 700;
}

.last-checked {
  font-size: 0.8rem;
  color: #64748b;
}

.telemetry-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  gap: 1.25rem;
  margin-bottom: 1.5rem;
}

.telemetry-item {
  background: rgba(15, 23, 42, 0.5);
  border: 1px solid rgba(255, 255, 255, 0.04);
  padding: 1rem;
  border-radius: 10px;
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
}

.telemetry-label {
  font-size: 0.75rem;
  color: #64748b;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  font-weight: 600;
}

.telemetry-value {
  font-size: 0.95rem;
  color: #f1f5f9;
  font-weight: 600;
}

.code-font {
  font-family: monospace;
  color: #38bdf8;
}

.quick-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 1rem;
  padding-top: 1rem;
  border-top: 1px solid rgba(255, 255, 255, 0.06);
}

.action-link {
  display: inline-flex;
  align-items: center;
  color: #38bdf8;
  text-decoration: none;
  font-size: 0.85rem;
  font-weight: 600;
  padding: 0.4rem 0.8rem;
  border-radius: 6px;
  background: rgba(2, 132, 199, 0.1);
  border: 1px solid rgba(56, 189, 248, 0.2);
  transition: all 0.2s ease;
}

.action-link:hover {
  background: rgba(2, 132, 199, 0.2);
  border-color: rgba(56, 189, 248, 0.4);
}

.roadmap-section {
  margin-top: 3rem;
}

.roadmap-title {
  font-size: 1.25rem;
  color: #f8fafc;
  margin-bottom: 1.25rem;
}

.roadmap-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: 1rem;
}

.roadmap-card {
  background: rgba(17, 24, 39, 0.5);
  border: 1px solid rgba(255, 255, 255, 0.06);
  border-radius: 12px;
  padding: 1.25rem;
}

.roadmap-card.completed {
  border-color: rgba(16, 185, 129, 0.4);
  background: rgba(16, 185, 129, 0.05);
}

.roadmap-badge {
  font-size: 0.7rem;
  font-weight: 700;
  text-transform: uppercase;
  color: #64748b;
  margin-bottom: 0.35rem;
}

.roadmap-card.completed .roadmap-badge {
  color: #34d399;
}

.roadmap-name {
  font-size: 0.95rem;
  font-weight: 700;
  color: #ffffff;
  margin-bottom: 0.5rem;
}

.roadmap-status {
  font-size: 0.78rem;
  font-weight: 600;
  color: #94a3b8;
}

.roadmap-card.completed .roadmap-status {
  color: #34d399;
}

.footer {
  text-align: center;
  padding: 2rem;
  border-top: 1px solid rgba(255, 255, 255, 0.06);
  color: #64748b;
  font-size: 0.85rem;
}

.footer-sub {
  font-size: 0.75rem;
  color: #475569;
  margin-top: 0.25rem;
}

@media (max-width: 640px) {
  .hero-title {
    font-size: 2.25rem;
  }
  .header {
    padding: 1rem;
  }
  .main-content {
    padding: 2rem 1rem;
  }
}
</style>
