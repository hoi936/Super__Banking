import { ref, computed } from 'vue'
import type { CustomerProfile } from '~/types/customer'
import type { AccountSummary } from '~/types/account'
import type { TransactionListItem } from '~/types/transaction'
import type { BillListItemDto } from '~/types/bill'
import { useCustomerService } from '~/services/customerService'
import { useAccountService } from '~/services/accountService'
import { useTransactionService } from '~/services/transactionService'
import { useBillService } from '~/services/billService'

export function useDashboard() {
  const customerService = useCustomerService()
  const accountService = useAccountService()
  const transactionService = useTransactionService()
  const billService = useBillService()

  const profile = ref<CustomerProfile | null>(null)
  const accounts = ref<AccountSummary[]>([])
  const recentTransactions = ref<TransactionListItem[]>([])
  const bills = ref<BillListItemDto[]>([])
  
  const isLoading = ref(true)
  const hasError = ref(false)
  const errorMessage = ref('')

  const totalBalance = computed(() => {
    return accounts.value.reduce((sum, account) => sum + account.balance, 0)
  })

  const unreadBillsCount = computed(() => {
    return bills.value.length
  })

  const fetchDashboardData = async () => {
    isLoading.value = true
    hasError.value = false
    errorMessage.value = ''

    try {
      // Execute non-dependent calls in parallel using Promise.allSettled
      // so that partial failures don't crash the whole dashboard
      const [profileResult, accountsResult, transactionsResult, billsResult] = await Promise.allSettled([
        customerService.getProfile(),
        accountService.getAccounts(),
        transactionService.getTransactions({ page: 1, pageSize: 5 }),
        billService.getBills({ page: 1, pageSize: 10, status: 'UNPAID' })
      ])

      if (profileResult.status === 'fulfilled') {
        profile.value = profileResult.value
      } else {
        console.error('Failed to load profile', profileResult.reason)
      }

      if (accountsResult.status === 'fulfilled') {
        accounts.value = accountsResult.value
      } else {
        console.error('Failed to load accounts', accountsResult.reason)
      }

      if (transactionsResult.status === 'fulfilled') {
        recentTransactions.value = transactionsResult.value.items || []
      } else {
        console.error('Failed to load transactions', transactionsResult.reason)
      }

      if (billsResult.status === 'fulfilled') {
        bills.value = billsResult.value.items || []
      } else {
        console.error('Failed to load bills', billsResult.reason)
      }

    } catch (e: any) {
      hasError.value = true
      errorMessage.value = e?.message || 'Có lỗi xảy ra khi tải dữ liệu tổng quan.'
      console.error('Dashboard fetch error', e)
    } finally {
      isLoading.value = false
    }
  }

  return {
    profile,
    accounts,
    recentTransactions,
    bills,
    totalBalance,
    unreadBillsCount,
    isLoading,
    hasError,
    errorMessage,
    fetchDashboardData
  }
}
