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

export * from './account'
export * from './auth'
export * from './bill'
export * from './customer'
export * from './notification'
export * from './transaction'
export * from './transfer'
export * from './beneficiary'
