import type { PagedResult } from './api'

export type MobileTopUpProductType = 'PhoneTopUp' | 'CardCode' | 'DataPackage' | string

export interface MobileProduct {
  code: string
  name: string
  productType: MobileTopUpProductType
  amount: number
  currency: string
}

export interface MobileProvider {
  code: string
  name: string
  products: MobileProduct[]
}

export interface CreateMobileTopUpRequest {
  sourceAccountId: string
  providerCode: string
  productCode: string
  phoneNumber?: string | null
}

export interface MobileTopUpReceipt {
  mobileTopUpId: string
  reference: string
  sourceAccountNumber: string
  providerCode: string
  providerName: string
  productType: MobileTopUpProductType
  productName: string
  phoneNumber?: string | null
  amount: number
  remainingBalance: number
  currency: string
  cardSerial?: string | null
  cardPin?: string | null
  status: string
  createdAtUtc: string
  completedAtUtc?: string | null
}

export interface MobileTopUpListItem {
  id: string
  reference: string
  sourceAccountNumber: string
  providerName: string
  productType: MobileTopUpProductType
  productName: string
  phoneNumber?: string | null
  amount: number
  currency: string
  status: string
  createdAtUtc: string
}

export type MobileTopUpPagedResult = PagedResult<MobileTopUpListItem>
