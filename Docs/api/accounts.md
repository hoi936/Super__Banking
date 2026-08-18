# 💳 Module Bank Accounts API

Module cung cấp các API cho phép khách hàng xem danh sách tài khoản ngân hàng, chi tiết tài khoản, số dư và tra cứu tài khoản thụ hưởng.

> [!IMPORTANT]
> **Bảo mật số dư (Balance Security)**: Frontend không có bất kỳ API nào để sửa đổi trực tiếp `Balance`. Số dư chỉ thay đổi thông qua các giao dịch chuyển tiền/thanh toán hóa đơn.

---

## 1. Danh sách Tài khoản của Khách hàng (Get Accounts)

- **Endpoint**: `GET /api/v1/accounts`
- **Quyền truy cập**: `[Authorize(Roles = "CUSTOMER")]`
- **Mô tả**: Trả về danh sách toàn bộ tài khoản ngân hàng (Checking, Savings...) thuộc sở hữu của khách hàng đang đăng nhập.

### Headers:
```http
Authorization: Bearer <accessToken>
```

### Response `200 OK`:
```json
[
  {
    "id": "ff6d4d8d-2222-4ca6-9bd9-0129b3c87ff3",
    "accountNumber": "1000000001",
    "accountName": "Nguyen Van An - Checking",
    "accountType": "CHECKING",
    "balance": 25000000.00,
    "currency": "VND",
    "status": "ACTIVE"
  }
]
```

---

## 2. Chi tiết Tài khoản (Get Account Detail)

- **Endpoint**: `GET /api/v1/accounts/{id}`
- **Quyền truy cập**: `[Authorize(Roles = "CUSTOMER")]`
- **Mô tả**: Xem thông tin chi tiết một tài khoản cụ thể.
- **Bảo mật quyền sở hữu**: Nếu ID tài khoản thuộc khách hàng khác, Backend trả về `404 Not Found` để tuyệt đối không rò rỉ dữ liệu.

### Headers:
```http
Authorization: Bearer <accessToken>
```

### Response `200 OK`:
```json
{
  "id": "ff6d4d8d-2222-4ca6-9bd9-0129b3c87ff3",
  "customerId": "4dbb6d3f-6c15-4d0b-9e06-1c35eb30fbbc",
  "accountNumber": "1000000001",
  "accountName": "Nguyen Van An - Checking",
  "accountType": "CHECKING",
  "balance": 25000000.00,
  "currency": "VND",
  "status": "ACTIVE",
  "createdAtUtc": "2026-08-18T07:48:02.3163957Z"
}
```

### Error Responses:
- `404 Not Found`: Không tìm thấy tài khoản hoặc tài khoản thuộc người khác.

---

## 3. Tra cứu Tài khoản Thụ hưởng (Account Lookup)

- **Endpoint**: `GET /api/v1/accounts/lookup/{accountNumber}`
- **Quyền truy cập**: `[Authorize(Roles = "CUSTOMER")]`
- **Mô tả**: Tra cứu nhanh tên chủ tài khoản từ số tài khoản (phục vụ màn hình chuyển tiền và thêm người thụ hưởng).
- **Lưu ý bảo mật**: Endpoint này chỉ trả về Số tài khoản và Tên chủ tài khoản, **tuyệt đối không trả về số dư hay ID nội bộ**.

### Headers:
```http
Authorization: Bearer <accessToken>
```

### Response `200 OK`:
```json
{
  "accountNumber": "1000000002",
  "accountName": "Tran Thi Binh - Checking"
}
```

### Error Responses:
- `404 Not Found`: Không tìm thấy tài khoản hoặc tài khoản đang ở trạng thái `CLOSED`.

---

## 4. TypeScript Interfaces cho Frontend

```typescript
export interface AccountSummary {
  id: string;
  accountNumber: string;
  accountName: string;
  accountType: 'CHECKING' | 'SAVINGS' | 'FIXED_DEPOSIT' | 'LOAN';
  balance: number;
  currency: string;
  status: 'ACTIVE' | 'DORMANT' | 'FROZEN' | 'LOCKED' | 'CLOSED';
}

export interface AccountDetail extends AccountSummary {
  customerId: string;
  createdAtUtc: string;
}

export interface AccountLookup {
  accountNumber: string;
  accountName: string;
}
```
