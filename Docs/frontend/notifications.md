# Thông báo (Notifications) Frontend

## Routes
- `/notifications` : Hộp thư hệ thống thông báo dành cho khách hàng.

## Các tính năng chính
- **Cơ chế Unread Badge**:
  - Dựa trên state toàn cục `useState<number>('unreadNotificationCount')` để hiển thị chấm đỏ & con số trên App Header (cái chuông).
  - Tự động Load số lượng mỗi khi App Mount (reload).
  - Tự động update khi User xem hộp thư thông báo hoặc có hành động `markAsRead`.
  - Giảm thiểu API request thừa, không dùng Polling interval làm nặng server (đáp ứng đúng requirement).
- **Hộp thư Thông báo**:
  - Tích hợp 2 bộ lọc nhanh (Tất cả / Chưa đọc) qua Button Group.
  - Phân loại (TRANSFER, PAYMENT, ACCOUNT, SECURITY) bằng các Icon và Color Badge tương ứng.
  - Đánh dấu đã đọc tự động khi click (với API `PATCH /read`).
  - Nút "Đánh dấu tất cả đã đọc" (Gọi `PATCH /read-all` và reset bộ đếm Unread Count ngay lập tức).

## API Services
- **`notificationService.ts`**:
  - `getNotifications(filters)`: Lấy dữ liệu với phân trang.
  - `getUnreadCount()`: Trả về Object `{ count: number }`.
  - `markAsRead(id)`: Gọi Patch.
  - `markAllAsRead()`: Clear toàn bộ thông báo của current user thành read.
