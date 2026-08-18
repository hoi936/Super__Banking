# 👤 Module Customer Profile API

Module cung cấp các API cho phép khách hàng xem và cập nhật thông tin hồ sơ cá nhân của chính mình.

---

## 1. Xem Hồ sơ Khách hàng (Get Profile)

- **Endpoint**: `GET /api/v1/customers/me`
- **Quyền truy cập**: `[Authorize(Roles = "CUSTOMER")]`
- **Mô tả**: Trả về chi tiết hồ sơ của khách hàng gắn liền với tài khoản đang đăng nhập.

### Headers:
```http
Authorization: Bearer <accessToken>
```

### Response `200 OK`:
```json
{
  "id": "4dbb6d3f-6c15-4d0b-9e06-1c35eb30fbbc",
  "customerCode": "CUS000001",
  "fullName": "Nguyen Van An",
  "dateOfBirth": "1990-05-15",
  "gender": "Male",
  "phoneNumber": "0901234567",
  "address": "123 Le Loi, District 1, Ho Chi Minh City",
  "status": "ACTIVE"
}
```

### Error Responses:
- `401 Unauthorized`: Chưa đăng nhập hoặc token không hợp lệ.
- `403 Forbidden`: Token không thuộc Role `CUSTOMER` (ví dụ: Staff hoặc Admin đăng nhập).
- `404 Not Found`: Người dùng chưa được liên kết với hồ sơ khách hàng.

---

## 2. Cập nhật Hồ sơ Khách hàng (Update Profile)

- **Endpoint**: `PUT /api/v1/customers/me`
- **Quyền truy cập**: `[Authorize(Roles = "CUSTOMER")]`
- **Mô tả**: Cho phép khách hàng chỉnh sửa các thông tin được phép: Họ và tên, Ngày sinh, Giới tính, Số điện thoại, Địa chỉ.
- **Lưu ý bảo mật**: Frontend không thể chỉnh sửa `customerCode`, `status`, `userId` qua API này.

### Headers:
```http
Authorization: Bearer <accessToken>
Content-Type: application/json
```

### Request Body:
```json
{
  "fullName": "Nguyen Van An Updated",
  "dateOfBirth": "1990-05-15",
  "gender": "Male",
  "phoneNumber": "0909998877",
  "address": "789 Dong Khoi, District 1, Ho Chi Minh City"
}
```

### Response `200 OK`:
```json
{
  "id": "4dbb6d3f-6c15-4d0b-9e06-1c35eb30fbbc",
  "customerCode": "CUS000001",
  "fullName": "Nguyen Van An Updated",
  "dateOfBirth": "1990-05-15",
  "gender": "Male",
  "phoneNumber": "0909998877",
  "address": "789 Dong Khoi, District 1, Ho Chi Minh City",
  "status": "ACTIVE"
}
```

### Error Responses:
- `400 Bad Request`: Thiếu `fullName` hoặc trường dữ liệu không hợp lệ.

---

## 3. TypeScript Interfaces cho Frontend

```typescript
export interface CustomerProfile {
  id: string;
  customerCode: string;
  fullName: string;
  dateOfBirth?: string; // YYYY-MM-DD
  gender?: string;
  phoneNumber?: string;
  address?: string;
  status: 'ACTIVE' | 'SUSPENDED' | 'CLOSED';
}

export interface UpdateCustomerProfileRequest {
  fullName: string;
  dateOfBirth?: string;
  gender?: string;
  phoneNumber?: string;
  address?: string;
}
```
