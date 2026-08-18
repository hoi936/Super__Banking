# 🔐 Module Authentication & RBAC API

Module cung cấp các API xử lý đăng nhập, quản lý phiên làm việc với JWT & Refresh Token, xoay vòng token (Token Rotation), đăng xuất và phân quyền người dùng (Role-Based Access Control).

---

## 1. Đăng nhập (Login)

- **Endpoint**: `POST /api/v1/auth/login`
- **Quyền truy cập**: Public (Không cần Token)
- **Mô tả**: Xác thực người dùng bằng email và mật khẩu, trả về cặp Access Token và Refresh Token.

### Request Body:
```json
{
  "email": "customer1@locallink.local",
  "password": "LocalLink@123"
}
```

### Response `200 OK`:
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "QO7h4N/yDLHVwdskiIJcW5q78...",
  "expiresAtUtc": "2026-08-18T08:35:00Z",
  "user": {
    "id": "fc5049d7-f5b2-4163-8ebd-e3a0d89cd92c",
    "email": "customer1@locallink.local",
    "status": "ACTIVE"
  },
  "roles": [
    "CUSTOMER"
  ],
  "customer": {
    "id": "4dbb6d3f-6c15-4d0b-9e06-1c35eb30fbbc",
    "customerCode": "CUS000001",
    "fullName": "Nguyen Van An"
  }
}
```

### Error Responses:
- `401 Unauthorized`: Sai email hoặc mật khẩu / Tài khoản đang bị khóa (Suspended).
```json
{
  "status": 401,
  "title": "Unauthorized",
  "detail": "Invalid email or password."
}
```

---

## 2. Làm mới Token (Refresh Token Rotation)

- **Endpoint**: `POST /api/v1/auth/refresh`
- **Quyền truy cập**: Public
- **Mô tả**: Gửi `refreshToken` hiện tại để nhận về một cặp token hoàn toàn mới (`accessToken` mới + `refreshToken` mới). Token cũ sẽ lập tức bị thu hồi để chống hack phiên.

### Request Body:
```json
{
  "refreshToken": "QO7h4N/yDLHVwdskiIJcW5q78..."
}
```

### Response `200 OK`:
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.new...",
  "refreshToken": "RuvDf21U8SdF3mAEWcmMRuvDf...",
  "expiresAtUtc": "2026-08-18T08:50:00Z"
}
```

### Error Responses:
- `401 Unauthorized`: Refresh token không hợp lệ, đã hết hạn hoặc đã bị thu hồi trước đó.

---

## 3. Đăng xuất (Logout)

- **Endpoint**: `POST /api/v1/auth/logout`
- **Quyền truy cập**: `[Authorize]`
- **Mô tả**: Hủy refresh token đang hoạt động của người dùng và ghi nhật ký kiểm toán `LOGOUT`.

### Headers:
```http
Authorization: Bearer <accessToken>
```

### Request Body:
```json
{
  "refreshToken": "RuvDf21U8SdF3mAEWcmMRuvDf..."
}
```

### Response `200 OK`:
```json
{
  "message": "Logged out successfully."
}
```

---

## 4. Lấy thông tin người dùng hiện tại (Get Current User)

- **Endpoint**: `GET /api/v1/auth/me`
- **Quyền truy cập**: `[Authorize]` (Áp dụng cho mọi Role)
- **Mô tả**: Trả về thông tin ID, Email, Roles và Hồ sơ khách hàng (nếu có) của token đang đăng nhập.

### Headers:
```http
Authorization: Bearer <accessToken>
```

### Response `200 OK`:
```json
{
  "id": "fc5049d7-f5b2-4163-8ebd-e3a0d89cd92c",
  "email": "customer1@locallink.local",
  "roles": [
    "CUSTOMER"
  ],
  "customer": {
    "id": "4dbb6d3f-6c15-4d0b-9e06-1c35eb30fbbc",
    "customerCode": "CUS000001",
    "fullName": "Nguyen Van An"
  }
}
```

---

## 5. TypeScript Interfaces cho Frontend

```typescript
export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  expiresAtUtc: string;
  user: UserSummary;
  roles: string[];
  customer?: CustomerSummary;
}

export interface UserSummary {
  id: string;
  email: string;
  status: 'ACTIVE' | 'SUSPENDED' | 'LOCKED';
}

export interface CustomerSummary {
  id: string;
  customerCode: string;
  fullName: string;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export interface TokenResponse {
  accessToken: string;
  refreshToken: string;
  expiresAtUtc: string;
}

export interface CurrentUser {
  id: string;
  email: string;
  roles: ('CUSTOMER' | 'STAFF' | 'ADMIN')[];
  customer?: CustomerSummary;
}
```
