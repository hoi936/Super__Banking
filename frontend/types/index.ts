export interface SystemStatus {
  application: string
  status: string
  environment: string
  database: string
  timestampUtc: string
  version: string
}

export interface HealthCheckResponse {
  status: 'Healthy' | 'Degraded' | 'Unhealthy' | string
  totalDurationMs: number
  entries: {
    key: string
    status: string
    description?: string
    durationMs: number
    error?: string
  }[]
}
