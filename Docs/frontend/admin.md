# Quản lý Quản trị viên (Admin & Staff) Frontend

## Routes
- `/admin` : Admin Dashboard (Thống kê tổng quan hệ thống).
- `/admin/customers` : Danh sách khách hàng (Quản lý và tìm kiếm khách hàng).
- `/admin/customers/[id]` : Chi tiết khách hàng, danh sách tài khoản ngân hàng, thay đổi trạng thái hoạt động (Khóa/Mở khóa).
- `/admin/transactions` : Lịch sử toàn bộ giao dịch hệ thống.
- `/admin/transactions/[id]` : Chi tiết giao dịch.
- `/admin/payments` : Lịch sử toàn bộ thanh toán hóa đơn.
- `/admin/payments/[id]` : Chi tiết thanh toán hóa đơn.
- `/admin/users` : Quản lý người dùng nội bộ (Chỉ dành cho ADMIN).
- `/admin/users/[id]` : Chi tiết người dùng nội bộ và khóa/mở khóa tài khoản (Chỉ dành cho ADMIN).
- `/admin/audit-logs` : Nhật ký hệ thống Audit Logs (Chỉ dành cho ADMIN).

## Các tính năng chính
- **Dashboard Thống kê**: Hiển thị tổng số người dùng, giao dịch, thanh toán, doanh thu và sức khỏe hệ thống.
- **RBAC (Role-Based Access Control)**:
  - `STAFF`: Có quyền xem danh sách khách hàng, giao dịch, thanh toán nhưng **không được phép** xem Users và Audit Logs, đồng thời không có quyền Khóa/Mở khóa (Mutation).
  - `ADMIN`: Có toàn quyền truy cập, bao gồm khóa tài khoản khách hàng, khóa tài khoản ngân hàng, đình chỉ người dùng nội bộ.
- **Bảo mật thao tác (Button Safety)**: Mọi hành động nhạy cảm (Khóa tài khoản) đều yêu cầu hộp thoại xác nhận (Confirm Dialog) và bị vô hiệu hóa khi đang xử lý (loading/disabled) để tránh double-submit.

## API Services
- **`adminCustomerService.ts`**: Lấy danh sách, chi tiết khách hàng và cập nhật trạng thái.
- **`adminTransactionService.ts`**: Quản lý giao dịch toàn hệ thống.
- **`adminPaymentService.ts`**: Quản lý thanh toán hóa đơn toàn hệ thống.
- **`adminUserService.ts`**: Quản lý người dùng nội bộ (Staff/Admin).
- **`auditLogService.ts`**: Lấy nhật ký hệ thống.
