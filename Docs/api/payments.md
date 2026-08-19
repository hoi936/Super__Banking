# Payments API Specification

Tài liệu chi tiết về API thanh toán hóa đơn dịch vụ, chống trùng lặp qua `Idempotency-Key`, kiểm soát tương tranh `RowVersion` và tra cứu lịch sử thanh toán.

---

## 1. Thanh toán Hóa đơn (`POST /api/v1/payments`)

Thực hiện thanh toán một hóa đơn từ tài khoản thanh toán VND của khách hàng.

> [!IMPORTANT]
> - **Số tiền do Server kiểm soát**: Request chỉ truyền `{ billId, accountId }`. Số tiền `Amount` được server trích xuất trực tiếp từ bản ghi `Bill` trong cơ sở dữ liệu để ngăn chặn hành vi can thiệp số tiền từ client.
> - **Chống trùng lặp (Idempotency)**: Luôn gửi kèm header `Idempotency-Key` (chuỗi ngẫu nhiên hoặc UUID). Nếu mạng gián đoạn và client gửi lại đúng key đó, server sẽ trả về biên lai thanh toán ban đầu mà **không trừ tiền lần 2**.

- **URL**: `/api/v1/payments`
- **Method**: `POST`
- **Yêu cầu Xác thực**: Bearer JWT (`CUSTOMER`)

### Headers
```http
Authorization: Bearer <access_token>
Idempotency-Key: <unique_idempotency_key>
Content-Type: application/json
```

### TypeScript Request Interface
```typescript
export interface CreatePaymentRequest {
  billId: string;
  accountId: string;
}
```

### Request Body Example
```json
{
  "billId": "7b7a7012-32a4-44bf-80a5-f8510bf23d4e",
  "accountId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

### TypeScript Response Interface
```typescript
export interface PaymentReceiptDto {
  paymentId: string;
  reference: string;
  billNumber: string;
  providerName: string;
  accountNumber: string;
  amount: number;
  currency: string;
  status: 'COMPLETED';
  paidAtUtc: string;
}
```

### Response Example (`201 Created` / `200 OK`)
```json
{
  "paymentId": "91234567-89ab-cdef-0123-456789abcdef",
  "reference": "PAY202608181005001234",
  "billNumber": "ELEC-2026-0001",
  "providerName": "Da Nang Electricity",
  "accountNumber": "1000000001",
  "amount": 850000.00,
  "currency": "VND",
  "status": "COMPLETED",
  "paidAtUtc": "2026-08-18T10:05:00.123Z"
}
```

---

## 2. Lịch sử Thanh toán Hóa đơn (`GET /api/v1/payments`)

Lấy danh sách các hóa đơn đã thanh toán của khách hàng hiện tại.

- **URL**: `/api/v1/payments`
- **Method**: `GET`
- **Query Parameters**:
  - `page` (int, mặc định `1`)
  - `pageSize` (int, mặc định `10`)
  - `status` (string, tùy chọn: `COMPLETED`, `PENDING`, `FAILED`)
  - `fromDate` (string `YYYY-MM-DD`, tùy chọn)
  - `toDate` (string `YYYY-MM-DD`, tùy chọn)
  - `billType` (string, tùy chọn: `ELECTRICITY`, `WATER`, `INTERNET`...)

### TypeScript Interface
```typescript
export interface PaymentListItemDto {
  id: string;
  referenceNumber: string;
  billNumber: string;
  providerName: string;
  billType: string;
  accountNumber: string;
  amount: number;
  currency: string;
  status: string;
  paidAtUtc?: string | null;
  createdAtUtc: string;
}
```

---

## 3. Chi tiết Thanh toán Hóa đơn (`GET /api/v1/payments/{id}`)

- **URL**: `/api/v1/payments/{id}`
- **Method**: `GET`

### TypeScript Interface
```typescript
export interface PaymentDetailDto {
  id: string;
  referenceNumber: string;
  billId: string;
  billNumber: string;
  providerName: string;
  billType: string;
  accountId: string;
  accountNumber: string;
  amount: number;
  currency: string;
  status: string;
  paidAtUtc?: string | null;
  createdAtUtc: string;
  idempotencyKey?: string | null;
}
```

---

## 4. Mã lỗi Thường gặp

| Mã HTTP | Tình huống |
| :--- | :--- |
| `400 Bad Request` | Hóa đơn đã thanh toán (`BILL_ALREADY_PAID`), tài khoản nguồn bị khóa, hoặc không đủ số dư (`INSUFFICIENT_FUNDS`). |
| `404 Not Found` | Không tìm thấy hóa đơn hoặc tài khoản (hoặc thuộc quyền sở hữu của khách hàng khác). |
| `409 Conflict` | Xung đột `Idempotency-Key` với payload khác, hoặc tranh chấp giao dịch đồng thời trên số dư (`DbUpdateConcurrencyException`). |
