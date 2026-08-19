# Notifications API Specification

Tài liệu chi tiết về hệ thống thông báo người dùng (biến động số dư, chuyển tiền, thanh toán hóa đơn, cảnh báo bảo mật).

---

## 1. Danh sách Thông báo (`GET /api/v1/notifications`)

Lấy danh sách thông báo của người dùng hiện tại (sắp xếp mới nhất lên đầu).

- **URL**: `/api/v1/notifications`
- **Method**: `GET`
- **Yêu cầu Xác thực**: Bearer JWT (Mọi role đã đăng nhập)
- **Query Parameters**:
  - `page` (int, mặc định `1`)
  - `pageSize` (int, mặc định `10`)
  - `isRead` (bool, tùy chọn: `true` / `false`)
  - `type` (string, tùy chọn: `TRANSFER`, `PAYMENT`, `ACCOUNT`, `SYSTEM`, `SECURITY`)

### TypeScript Interface
```typescript
export interface NotificationDto {
  id: string;
  title: string;
  message: string;
  type: 'TRANSFER' | 'PAYMENT' | 'ACCOUNT' | 'SYSTEM' | 'SECURITY';
  isRead: boolean;
  createdAtUtc: string;
  readAtUtc?: string | null;
}
```

### Response Example (`200 OK`)
```json
{
  "items": [
    {
      "id": "e2345678-abcd-ef01-2345-6789abcdef01",
      "title": "Thanh toán thành công",
      "message": "Bạn đã thanh toán hóa đơn Da Nang Electricity (ELEC-2026-0001) số tiền 850,000 VND thành công.",
      "type": "PAYMENT",
      "isRead": false,
      "createdAtUtc": "2026-08-18T10:05:00Z",
      "readAtUtc": null
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalItems": 1,
  "totalPages": 1,
  "hasNextPage": false,
  "hasPreviousPage": false
}
```

---

## 2. Số lượng Thông báo Chưa đọc (`GET /api/v1/notifications/unread-count`)

Dùng để hiển thị badge số lượng thông báo trên chuông notification của Navbar.

- **URL**: `/api/v1/notifications/unread-count`
- **Method**: `GET`

### Response Example (`200 OK`)
```json
{
  "count": 3
}
```

---

## 3. Đánh dấu Đã đọc Một thông báo (`PATCH /api/v1/notifications/{id}/read`)

- **URL**: `/api/v1/notifications/{id}/read`
- **Method**: `PATCH`

### Response Example (`200 OK`)
```json
{
  "id": "e2345678-abcd-ef01-2345-6789abcdef01",
  "title": "Thanh toán thành công",
  "message": "Bạn đã thanh toán hóa đơn Da Nang Electricity (ELEC-2026-0001) số tiền 850,000 VND thành công.",
  "type": "PAYMENT",
  "isRead": true,
  "createdAtUtc": "2026-08-18T10:05:00Z",
  "readAtUtc": "2026-08-18T10:06:12Z"
}
```

---

## 4. Đánh dấu Đã đọc Tất cả Thông báo (`PATCH /api/v1/notifications/read-all`)

- **URL**: `/api/v1/notifications/read-all`
- **Method**: `PATCH`

### Response Example (`200 OK`)
```json
{
  "updatedCount": 3
}
```
