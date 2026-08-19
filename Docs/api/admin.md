# 🛠️ Module Staff & Admin Management API

Module cung cấp các API dành cho nhân viên vận hành (**STAFF**) và quản trị viên (**ADMIN**) để quản lý danh sách khách hàng, thông tin tài khoản và điều chỉnh trạng thái tài khoản.

---

## 1. Danh sách Khách hàng Phân trang (Get Customers)

- **Endpoint**: `GET /api/v1/admin/customers`
- **Quyền truy cập**: `[Authorize(Roles = "STAFF,ADMIN")]`
- **Mô tả**: Lấy danh sách khách hàng có hỗ trợ phân trang ở mức SQL Server, tìm kiếm và lọc trạng thái.

### Headers:
```http
Authorization: Bearer <staff_or_admin_accessToken>
```

### Query Parameters:
| Param | Kiểu dữ liệu | Mặc định | Mô tả |
| :--- | :--- | :--- | :--- |
| `page` | `number` | `1` | Trang hiện tại (bắt đầu từ 1) |
| `pageSize` | `number` | `20` | Số lượng bản ghi trên một trang (tối đa 100) |
| `search` | `string` | `null` | Từ khóa tìm kiếm theo Họ tên, Mã khách hàng, Email hoặc SĐT |
| `status` | `string` | `null` | Lọc theo trạng thái (`ACTIVE`, `SUSPENDED`, `CLOSED`) |

**Ví dụ URL**:
```text
GET /api/v1/admin/customers?page=1&pageSize=10&search=nguyen&status=ACTIVE
```

### Response `200 OK`:
```json
{
  "items": [
    {
      "id": "4dbb6d3f-6c15-4d0b-9e06-1c35eb30fbbc",
      "userId": "fc5049d7-f5b2-4163-8ebd-e3a0d89cd92c",
      "email": "customer1@locallink.local",
      "customerCode": "CUS000001",
      "fullName": "Nguyen Van An",
      "phoneNumber": "0901234567",
      "customerStatus": "ACTIVE",
      "userStatus": "ACTIVE",
      "accountsCount": 1,
      "createdAtUtc": "2026-08-18T07:48:02.3163957Z"
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalItems": 1,
  "totalPages": 1
}
```

---

## 2. Chi tiết Khách hàng cho Admin (Get Customer Detail)

- **Endpoint**: `GET /api/v1/admin/customers/{id}`
- **Quyền truy cập**: `[Authorize(Roles = "STAFF,ADMIN")]`
- **Mô tả**: Xem toàn bộ thông tin chi tiết của một khách hàng bao gồm trạng thái User, danh sách Role và tóm tắt các tài khoản ngân hàng liên kết.

### Headers:
```http
Authorization: Bearer <staff_or_admin_accessToken>
```

### Response `200 OK`:
```json
{
  "id": "4dbb6d3f-6c15-4d0b-9e06-1c35eb30fbbc",
  "userId": "fc5049d7-f5b2-4163-8ebd-e3a0d89cd92c",
  "email": "customer1@locallink.local",
  "customerCode": "CUS000001",
  "fullName": "Nguyen Van An",
  "dateOfBirth": "1990-05-15",
  "gender": "Male",
  "phoneNumber": "0901234567",
  "address": "123 Le Loi, District 1, Ho Chi Minh City",
  "customerStatus": "ACTIVE",
  "userStatus": "ACTIVE",
  "roles": [
    "CUSTOMER"
  ],
  "accounts": [
    {
      "id": "ff6d4d8d-2222-4ca6-9bd9-0129b3c87ff3",
      "accountNumber": "1000000001",
      "accountName": "Nguyen Van An - Checking",
      "accountType": "CHECKING",
      "balance": 25000000.00,
      "currency": "VND",
      "status": "ACTIVE"
    }
  ],
  "createdAtUtc": "2026-08-18T07:48:02.3163957Z",
  "updatedAtUtc": null
}
```

---

## 3. Danh sách Tài khoản của Khách hàng (Get Customer Accounts)

- **Endpoint**: `GET /api/v1/admin/customers/{id}/accounts`
- **Quyền truy cập**: `[Authorize(Roles = "STAFF,ADMIN")]`
- **Mô tả**: Lấy danh sách tài khoản ngân hàng của khách hàng theo `customerId`.

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

## 4. Khóa / Mở khóa Khách hàng (Update Customer Status)

- **Endpoint**: `PATCH /api/v1/admin/customers/{id}/status`
- **Quyền truy cập**: `[Authorize(Roles = "ADMIN")]` *(Staff không có quyền gọi API này)*
- **Mô tả**: Thay đổi trạng thái khách hàng (`ACTIVE` $\leftrightarrow$ `SUSPENDED`). Hệ thống sẽ tự động đồng bộ trạng thái User và tạo bản ghi kiểm toán `AuditLog` (`CUSTOMER_SUSPEND` hoặc `CUSTOMER_ACTIVATE`).

### Request Body:
```json
{
  "status": "SUSPENDED"
}
```

### Response `200 OK`:
```json
{
  "id": "4dbb6d3f-6c15-4d0b-9e06-1c35eb30fbbc",
  "customerCode": "CUS000001",
  "fullName": "Nguyen Van An",
  "status": "SUSPENDED"
}
```

---

## 5. Khóa / Mở khóa Tài khoản Ngân hàng (Update Account Status)

- **Endpoint**: `PATCH /api/v1/admin/accounts/{id}/status`
- **Quyền truy cập**: `[Authorize(Roles = "ADMIN")]` *(Staff không có quyền gọi API này)*
- **Mô tả**: Thay đổi trạng thái tài khoản ngân hàng (`ACTIVE` $\leftrightarrow$ `LOCKED`) và tạo bản ghi kiểm toán `AuditLog` (`ACCOUNT_LOCK` hoặc `ACCOUNT_UNLOCK`).

### Request Body:
```json
{
  "status": "LOCKED"
}
```

### Response `200 OK`:
```json
{
  "id": "ff6d4d8d-2222-4ca6-9bd9-0129b3c87ff3",
  "accountNumber": "1000000001",
  "accountName": "Nguyen Van An - Checking",
  "accountType": "CHECKING",
  "balance": 25000000.00,
  "currency": "VND",
  "status": "LOCKED"
}
```

---

## 6. TypeScript Interfaces cho Frontend

```typescript
export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}

export interface AdminCustomerListItem {
  id: string;
  userId: string;
  email: string;
  customerCode: string;
  fullName: string;
  phoneNumber?: string;
  customerStatus: string;
  userStatus: string;
  accountsCount: number;
  createdAtUtc: string;
}

export interface AdminCustomerDetail {
  id: string;
  userId: string;
  email: string;
  customerCode: string;
  fullName: string;
  dateOfBirth?: string;
  gender?: string;
  phoneNumber?: string;
  address?: string;
  customerStatus: string;
  userStatus: string;
  roles: string[];
  accounts: AccountSummary[];
  createdAtUtc: string;
  updatedAtUtc?: string;
}

export interface UpdateCustomerStatusRequest {
  status: 'ACTIVE' | 'SUSPENDED';
}

export interface UpdateAccountStatusRequest {
  status: 'ACTIVE' | 'LOCKED';
}
```
