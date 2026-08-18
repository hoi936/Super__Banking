/**
 * Tiện ích quản lý nhãn và màu sắc cho các Status trong hệ thống.
 */

// 1. Account / Customer / User Status
export const getUserStatusLabel = (status: string): string => {
  switch (status) {
    case 'ACTIVE': return 'Hoạt động'
    case 'SUSPENDED': return 'Tạm khóa'
    case 'LOCKED': return 'Khóa cứng'
    case 'CLOSED': return 'Đã đóng'
    default: return status || 'Không rõ'
  }
}

export const getUserStatusColor = (status: string): string => {
  switch (status) {
    case 'ACTIVE': return 'positive'
    case 'SUSPENDED': return 'warning'
    case 'LOCKED': return 'negative'
    case 'CLOSED': return 'grey'
    default: return 'grey'
  }
}

export const getUserStatusIcon = (status: string): string => {
  switch (status) {
    case 'ACTIVE': return 'check_circle'
    case 'SUSPENDED': return 'warning'
    case 'LOCKED': return 'lock'
    case 'CLOSED': return 'cancel'
    default: return 'help'
  }
}

// 2. Transaction / Transfer Status
export const getTransactionStatusLabel = (status: string): string => {
  switch (status) {
    case 'COMPLETED': return 'Hoàn tất'
    case 'PENDING': return 'Đang xử lý'
    case 'FAILED': return 'Thất bại'
    case 'CANCELLED': return 'Đã hủy'
    default: return status || 'Không rõ'
  }
}

export const getTransactionStatusColor = (status: string): string => {
  switch (status) {
    case 'COMPLETED': return 'positive'
    case 'PENDING': return 'warning'
    case 'FAILED': return 'negative'
    case 'CANCELLED': return 'grey'
    default: return 'grey'
  }
}

export const getTransactionStatusIcon = (status: string): string => {
  switch (status) {
    case 'COMPLETED': return 'check_circle'
    case 'PENDING': return 'pending'
    case 'FAILED': return 'error'
    case 'CANCELLED': return 'cancel'
    default: return 'help'
  }
}

// 3. Bill / Payment Status
export const getBillStatusLabel = (status: string): string => {
  switch (status) {
    case 'UNPAID': return 'Chưa thanh toán'
    case 'PAID': return 'Đã thanh toán'
    case 'OVERDUE': return 'Quá hạn'
    default: return status || 'Không rõ'
  }
}

export const getBillStatusColor = (status: string): string => {
  switch (status) {
    case 'PAID': return 'positive'
    case 'UNPAID': return 'warning'
    case 'OVERDUE': return 'negative'
    default: return 'grey'
  }
}

export const getBillStatusIcon = (status: string): string => {
  switch (status) {
    case 'PAID': return 'check_circle'
    case 'UNPAID': return 'pending_actions'
    case 'OVERDUE': return 'alarm_off'
    default: return 'help'
  }
}

// 4. Transaction Type
export const getTransactionTypeLabel = (type: string): string => {
  switch (type) {
    case 'TRANSFER': return 'Chuyển tiền'
    case 'PAYMENT': return 'Thanh toán'
    case 'DEPOSIT': return 'Nạp tiền'
    case 'WITHDRAWAL': return 'Rút tiền'
    case 'FEE': return 'Thu phí'
    default: return type || 'Không rõ'
  }
}

export const getTransactionTypeColor = (type: string): string => {
  switch (type) {
    case 'TRANSFER': return 'blue'
    case 'PAYMENT': return 'teal'
    case 'DEPOSIT': return 'positive'
    case 'WITHDRAWAL': return 'warning'
    case 'FEE': return 'grey'
    default: return 'grey'
  }
}
