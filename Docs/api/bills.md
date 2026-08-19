# Bills API Specification

Tài liệu chi tiết về các API tra cứu hóa đơn dịch vụ (điện, nước, internet...) dành cho khách hàng cá nhân (`CUSTOMER`).

---

## 1. Danh sách Hóa đơn (`GET /api/v1/bills`)

Lấy danh sách các hóa đơn của khách hàng đang đăng nhập với phân trang ở mức cơ sở dữ liệu.

- **URL**: `/api/v1/bills`
- **Method**: `GET`
- **Yêu cầu Xác thực**: Bearer JWT (`CUSTOMER`)
- **Query Parameters**:
  - `page` (int, mặc định `1`): Số trang cần lấy.
  - `pageSize` (int, mặc định `10`, tối đa `100`): Số hóa đơn trên mỗi trang.
  - `status` (string, tùy chọn): Lọc theo trạng thái hóa đơn (`UNPAID`, `PAID`, `OVERDUE`, `CANCELLED`).
  - `type` (string, tùy chọn): Lọc theo loại hóa đơn (`ELECTRICITY`, `WATER`, `INTERNET`, `EDUCATION`, `OTHER`).
  - `fromDueDate` (string, dạng `YYYY-MM-DD`, tùy chọn): Lọc hạn thanh toán từ ngày.
  - `toDueDate` (string, dạng `YYYY-MM-DD`, tùy chọn): Lọc hạn thanh toán đến ngày.

### Headers
```http
Authorization: Bearer <access_token>
```

### TypeScript Interface
```typescript
export interface BillListItemDto {
  id: string;
  billNumber: string;
  providerName: string;
  billType: 'ELECTRICITY' | 'WATER' | 'INTERNET' | 'EDUCATION' | 'OTHER';
  amount: number;
  dueDate: string; // YYYY-MM-DD
  status: 'UNPAID' | 'PAID' | 'OVERDUE' | 'CANCELLED';
  createdAtUtc: string;
}

export interface PagedBillsResponse {
  items: BillListItemDto[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}
```

### Response Example (`200 OK`)
```json
{
  "items": [
    {
      "id": "7b7a7012-32a4-44bf-80a5-f8510bf23d4e",
      "billNumber": "ELEC-2026-0001",
      "providerName": "Da Nang Electricity",
      "billType": "ELECTRICITY",
      "amount": 850000.00,
      "dueDate": "2026-08-30",
      "status": "UNPAID",
      "createdAtUtc": "2026-08-18T10:00:00Z"
    },
    {
      "id": "fa23d11a-1290-41bf-a342-990c0aa23456",
      "billNumber": "WATER-2026-0001",
      "providerName": "Da Nang Water",
      "billType": "WATER",
      "amount": 220000.00,
      "dueDate": "2026-08-30",
      "status": "UNPAID",
      "createdAtUtc": "2026-08-18T10:00:00Z"
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalItems": 2,
  "totalPages": 1,
  "hasNextPage": false,
  "hasPreviousPage": false
}
```

---

## 2. Chi tiết Hóa đơn (`GET /api/v1/bills/{id}`)

Lấy chi tiết một hóa đơn cụ thể. Hệ thống kiểm tra quyền sở hữu tuyệt đối (`CustomerId == CurrentCustomer.Id`).

- **URL**: `/api/v1/bills/{id}`
- **Method**: `GET`
- **Yêu cầu Xác thực**: Bearer JWT (`CUSTOMER`)

### TypeScript Interface
```typescript
export interface BillDetailDto {
  id: string;
  billNumber: string;
  providerName: string;
  billType: string;
  amount: number;
  dueDate: string;
  status: string;
  createdAtUtc: string;
  updatedAtUtc?: string | null;
  paymentId?: string | null;
  paidAtUtc?: string | null;
}
```

### Response Example (`200 OK`)
```json
{
  "id": "7b7a7012-32a4-44bf-80a5-f8510bf23d4e",
  "billNumber": "ELEC-2026-0001",
  "providerName": "Da Nang Electricity",
  "billType": "ELECTRICITY",
  "amount": 850000.00,
  "dueDate": "2026-08-30",
  "status": "UNPAID",
  "createdAtUtc": "2026-08-18T10:00:00Z",
  "updatedAtUtc": null,
  "paymentId": null,
  "paidAtUtc": null
}
```

---

## 3. Mã lỗi Thường gặp

| Mã HTTP | Tình huống |
| :--- | :--- |
| `401 Unauthorized` | Thiếu access token hoặc token hết hạn. |
| `403 Forbidden` | Token không có role `CUSTOMER`. |
| `404 Not Found` | Hóa đơn không tồn tại hoặc thuộc quyền sở hữu của khách hàng khác. |
