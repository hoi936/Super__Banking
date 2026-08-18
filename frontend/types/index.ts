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

export * from './auth'
export * from './account'
export * from './transfer'
export * from './beneficiary'
export * from './transaction'
export * from './bill'
export * from './payment'
export * from './notification'
