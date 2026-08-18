# 💸 Module Transfers (Chuyển tiền Nội bộ) API

Module cung cấp các API xử lý giao dịch chuyển tiền nội bộ an toàn, bảo toàn dòng tiền, đảm bảo tính nguyên tử (ACID Transaction), chống gửi trùng lặp (Idempotency Key) và kiểm soát tranh chấp đồng thời (Optimistic Concurrency).

---

## 1. Thực hiện Chuyển tiền (Create Transfer)

- **Endpoint**: `POST /api/v1/transfers`
- **Quyền truy cập**: `[Authorize(Roles = "CUSTOMER")]`
- **Mô tả**: Chuyển tiền từ một tài khoản nguồn của khách hàng sang số tài khoản thụ hưởng trong hệ thống.
- **Header Chống Trùng lặp (Idempotency Key)**: Bắt buộc hoặc khuyến nghị gửi kèm header `Idempotency-Key` (ví dụ UUIDv4) để bảo vệ giao dịch không bị trừ tiền 2 lần khi mạng chập chờn hoặc người dùng bấm đúp nút Chuyển tiền.

### Headers:
```http
Authorization: Bearer <accessToken>
Idempotency-Key: 80f13a76-1234-5678-9abc-def012345678
Content-Type: application/json
```

### Request Body:
```json
{
  "sourceAccountId": "ff6d4d8d-2222-4ca6-9bd9-0129b3c87ff3",
  "destinationAccountNumber": "1000000002",
  "amount": 500000,
  "description": "Chuyen tien an trua"
}
```

### Response `201 Created` (hoặc `200 OK` khi Replay Idempotent):
```json
{
  "transferId": "4a08fd75-80f0-466d-9273-057476e330a1",
  "reference": "TRF202608181615001234",
  "sourceAccountId": "ff6d4d8d-2222-4ca6-9bd9-0129b3c87ff3",
  "sourceAccountNumber": "1000000001",
  "destinationAccountId": "ba123456-7890-4abc-def0-123456789012",
  "destinationAccountNumber": "1000000002",
  "destinationAccountName": "Tran Thi Binh",
  "amount": 500000.00,
  "currency": "VND",
  "description": "Chuyen tien an trua",
  "status": "COMPLETED",
  "createdAtUtc": "2026-08-18T09:15:00.1234567Z",
  "completedAtUtc": "2026-08-18T09:15:00.2345678Z"
}
```

### Error Responses:
- `400 Bad Request`:
  - `Transfer amount must be greater than zero.`
  - `Insufficient funds.` (Số dư không đủ)
  - `Source account is LOCKED and cannot perform transfers.`
  - `Destination account is LOCKED and cannot receive transfers.`
  - `Cannot transfer money to the same bank account.`
- `404 Not Found`:
  - `Source bank account not found or not owned by the current customer.`
  - `Destination account '...' was not found.`
- `409 Conflict`:
  - `Idempotency key was previously used with a different transfer payload.` (Tái sử dụng key cũ nhưng đổi số tiền hoặc người nhận).
  - `A concurrency conflict occurred while processing the transfer. Please refresh your balance and try again.` (Tranh chấp số dư khi có nhiều request đồng thời).

---

## 2. Xem Lịch sử Chuyển tiền (Get Transfers History)

- **Endpoint**: `GET /api/v1/transfers`
- **Quyền truy cập**: `[Authorize(Roles = "CUSTOMER")]`
- **Mô tả**: Trả về danh sách các giao dịch chuyển tiền (cả chiều gửi và nhận) thuộc các tài khoản của khách hàng, hỗ trợ phân trang ở SQL Server.

### Headers:
```http
Authorization: Bearer <accessToken>
```

### Query Parameters:
| Param | Kiểu dữ liệu | Mặc định | Mô tả |
| :--- | :--- | :--- | :--- |
| `page` | `number` | `1` | Trang hiện tại (1-based) |
| `pageSize` | `number` | `20` | Số lượng bản ghi mỗi trang (max 100) |
| `accountId` | `guid` | `null` | Lọc theo tài khoản nguồn/đích cụ thể |
| `status` | `string` | `null` | Lọc trạng thái (`COMPLETED`, `PENDING`, `FAILED`) |
| `fromDate` | `string` (ISO) | `null` | Lọc từ ngày (UTC) |
| `toDate` | `string` (ISO) | `null` | Lọc đến ngày (UTC) |

### Response `200 OK`:
```json
{
  "items": [
    {
      "id": "4a08fd75-80f0-466d-9273-057476e330a1",
      "reference": "TRF202608181615001234",
      "sourceAccountId": "ff6d4d8d-2222-4ca6-9bd9-0129b3c87ff3",
      "sourceAccountNumber": "1000000001",
      "destinationAccountId": "ba123456-7890-4abc-def0-123456789012",
      "destinationAccountNumber": "1000000002",
      "destinationAccountName": "Tran Thi Binh",
      "amount": 500000.00,
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

## 3. Chi tiết Biên lai Chuyển tiền (Get Transfer Detail)

- **Endpoint**: `GET /api/v1/transfers/{id}`
- **Quyền truy cập**: `[Authorize(Roles = "CUSTOMER")]`
- **Mô tả**: Lấy thông tin chi tiết một biên lai chuyển tiền. Khách hàng chỉ có thể xem biên lai nếu sở hữu tài khoản gửi hoặc nhận.

### Response `200 OK`:
```json
{
  "transferId": "4a08fd75-80f0-466d-9273-057476e330a1",
  "reference": "TRF202608181615001234",
  "sourceAccountId": "ff6d4d8d-2222-4ca6-9bd9-0129b3c87ff3",
  "sourceAccountNumber": "1000000001",
  "destinationAccountId": "ba123456-7890-4abc-def0-123456789012",
  "destinationAccountNumber": "1000000002",
  "destinationAccountName": "Tran Thi Binh",
  "amount": 500000.00,
  "currency": "VND",
  "description": "Chuyen tien an trua",
  "status": "COMPLETED",
  "createdAtUtc": "2026-08-18T09:15:00.1234567Z",
  "completedAtUtc": "2026-08-18T09:15:00.2345678Z"
}
```

---

## 4. TypeScript Interfaces cho Frontend

```typescript
export interface CreateTransferRequest {
  sourceAccountId: string;
  destinationAccountNumber: string;
  amount: number;
  description?: string;
}

export interface TransferReceipt {
  transferId: string;
  reference: string;
  sourceAccountId: string;
  sourceAccountNumber: string;
  destinationAccountId: string;
  destinationAccountNumber: string;
  destinationAccountName: string;
  amount: number;
  currency: string;
  description?: string;
  status: 'PENDING' | 'COMPLETED' | 'FAILED' | 'CANCELLED';
  createdAtUtc: string;
  completedAtUtc?: string;
}

export interface TransferListItem {
  id: string;
  reference: string;
  sourceAccountId: string;
  sourceAccountNumber: string;
  destinationAccountId: string;
  destinationAccountNumber: string;
  destinationAccountName: string;
  amount: number;
  description: string;
  status: string;
  createdAtUtc: string;
  completedAtUtc?: string;
}
```
