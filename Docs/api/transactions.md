# 📊 Module Transactions (Sổ cái Biến động Số dư) API

Module cung cấp các API cho phép khách hàng tra cứu lịch sử biến động số dư (Transaction Ledger) trên tất cả các tài khoản ngân hàng của mình.

---

## 1. Lấy Lịch sử Giao dịch (Get Transactions History)

- **Endpoint**: `GET /api/v1/transactions`
- **Quyền truy cập**: `[Authorize(Roles = "CUSTOMER")]`
- **Mô tả**: Trả về danh sách giao dịch (Chuyển tiền, Thanh toán, Nạp tiền, Rút tiền) thuộc các tài khoản của khách hàng đang đăng nhập, phân trang tại SQL Server.

### Headers:
```http
Authorization: Bearer <accessToken>
```

### Query Parameters:
| Param | Kiểu dữ liệu | Mặc định | Mô tả |
| :--- | :--- | :--- | :--- |
| `page` | `number` | `1` | Trang hiện tại (1-based) |
| `pageSize` | `number` | `20` | Số lượng bản ghi mỗi trang (max 100) |
| `accountId` | `guid` | `null` | Lọc theo tài khoản nguồn hoặc đích cụ thể |
| `type` | `string` | `null` | Lọc loại giao dịch (`Transfer`, `Payment`, `Deposit`, `Withdrawal`) |
| `fromDate` | `string` (ISO) | `null` | Lọc từ ngày (UTC) |
| `toDate` | `string` (ISO) | `null` | Lọc đến ngày (UTC) |

### Response `200 OK`:
```json
{
  "items": [
    {
      "id": "76ec98f1-8f56-42d4-b4a1-0e123456789a",
      "referenceNumber": "TRF202608181615001234",
      "transactionType": "TRANSFER",
      "sourceAccountId": "ff6d4d8d-2222-4ca6-9bd9-0129b3c87ff3",
      "sourceAccountNumber": "1000000001",
      "destinationAccountId": "ba123456-7890-4abc-def0-123456789012",
      "destinationAccountNumber": "1000000002",
      "amount": 500000.00,
      "currency": "VND",
      "description": "Chuyen tien an trua",
      "status": "COMPLETED",
      "createdAtUtc": "2026-08-18T09:15:00.1234567Z",
      "completedAtUtc": "2026-08-18T09:15:00.2345678Z"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalItems": 1,
  "totalPages": 1
}
```

---

## 2. Chi tiết Giao dịch (Get Transaction Detail)

- **Endpoint**: `GET /api/v1/transactions/{id}`
- **Quyền truy cập**: `[Authorize(Roles = "CUSTOMER")]`
- **Mô tả**: Lấy thông tin chi tiết đầy đủ của một giao dịch sổ cái (bao gồm tên tài khoản gửi và nhận). Khách hàng chỉ xem được nếu sở hữu tài khoản gửi hoặc nhận.

### Response `200 OK`:
```json
{
  "id": "76ec98f1-8f56-42d4-b4a1-0e123456789a",
  "referenceNumber": "TRF202608181615001234",
  "transactionType": "TRANSFER",
  "sourceAccountId": "ff6d4d8d-2222-4ca6-9bd9-0129b3c87ff3",
  "sourceAccountNumber": "1000000001",
  "sourceAccountName": "Nguyen Van An - Checking",
  "destinationAccountId": "ba123456-7890-4abc-def0-123456789012",
  "destinationAccountNumber": "1000000002",
  "destinationAccountName": "Tran Thi Binh - Checking",
  "amount": 500000.00,
  "currency": "VND",
  "description": "Chuyen tien an trua",
  "status": "COMPLETED",
  "createdAtUtc": "2026-08-18T09:15:00.1234567Z",
  "completedAtUtc": "2026-08-18T09:15:00.2345678Z"
}
```

### Error Responses:
- `404 Not Found`: Không tìm thấy giao dịch hoặc giao dịch không thuộc tài khoản của bạn.

---

## 3. TypeScript Interfaces cho Frontend

```typescript
export interface TransactionListItem {
  id: string;
  referenceNumber: string;
  transactionType: 'TRANSFER' | 'PAYMENT' | 'DEPOSIT' | 'WITHDRAWAL';
  sourceAccountId?: string;
  sourceAccountNumber?: string;
  destinationAccountId?: string;
  destinationAccountNumber?: string;
  amount: number;
  currency: string;
  description?: string;
  status: 'PENDING' | 'COMPLETED' | 'FAILED' | 'CANCELLED';
  createdAtUtc: string;
  completedAtUtc?: string;
}

export interface TransactionDetail extends TransactionListItem {
  sourceAccountName?: string;
  destinationAccountName?: string;
}
```
