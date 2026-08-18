export interface Beneficiary {
  id: string
  accountNumber: string
  accountName: string
  nickname?: string
  createdAtUtc: string
}

export interface CreateBeneficiaryRequest {
  accountNumber: string
  nickname?: string
}
