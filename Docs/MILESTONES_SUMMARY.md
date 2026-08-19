# Tổng kết Triển khai (Milestones M1 - M8)

Tài liệu này tóm tắt toàn bộ công việc, kiến trúc và tính năng đã được triển khai thành công từ Milestone 1 đến Milestone 8 của dự án **InterLink Banking**.

---

## M1: Project Foundation & DevOps
- Khởi tạo kiến trúc **Modular Monolith + Clean Architecture** cho .NET 10.
- Khởi tạo Frontend với **Nuxt 4 + Vue 3 + Quasar**.
- Cấu hình hạ tầng container hóa bằng **Docker & Docker Compose** (API, Web, SQL Server 2022).
- Thiết lập các endpoint chẩn đoán hệ thống: `/health`, `/api/system`, Swagger UI.
- Cấu hình Global Exception Handling (RFC 7807 ProblemDetails).

## M2: Database Foundation
- Thiết kế Entity Framework Core 10 `ApplicationDbContext`.
- Tạo 13 Entities nghiệp vụ chia làm các domain: Auth, Customer, Account, Transaction, Transfer, Bill, Notification, Audit.
- Tự động chạy **Migrations** và **Seed Data** (Admin, Staff, Customer) khi khởi động container.
- Thiết lập Index và cấu hình quan hệ (Foreign Keys, Cascade Rules).

## M3: Authentication, JWT & RBAC
- Tích hợp bảo mật Token-based với `Access Token (JWT)` và `Refresh Token`.
- Triển khai **Role-Based Access Control (RBAC)** với 3 roles: `CUSTOMER`, `STAFF`, `ADMIN`.
- Các API: Đăng nhập (`/api/v1/auth/login`), Refresh Session (`/api/v1/auth/refresh`), Đăng xuất, Lấy Profile (`/api/v1/auth/me`).

## M4: Customer & Bank Account Management
- API cho Quản lý thông tin khách hàng (CUS).
- Logic sinh số tài khoản (AccountNumber) tuần tự an toàn.
- Quản lý danh sách tài khoản ngân hàng (Checking/Savings) và danh sách Người thụ hưởng (Beneficiaries).
- Bảo mật quyền truy cập: Khách hàng chỉ xem được tài khoản của chính mình.

## M5: Transfer Engine & Transactions
- Cốt lõi giao dịch tài chính: Chuyển khoản nội bộ (Internal Transfers).
- Cơ chế **ACID Transactions** kết hợp **Optimistic Concurrency (RowVersion)** trên EF Core để chống Race Condition khi cập nhật số dư.
- Hỗ trợ **Idempotency-Key** chống trùng lặp giao dịch (Double-spending).
- Ghi log tự động vào sổ cái giao dịch (`Transaction Ledger`).

## M6: Bill Payment & Notifications
- Hệ thống thanh toán hóa đơn điện/nước/internet (Bill Payment) kết nối với Provider.
- Sử dụng Database Transaction nguyên tử (Atomic) trừ tiền và đổi trạng thái hóa đơn cùng lúc.
- Sinh biên lai thanh toán (Payment Receipts).
- Engine Thông báo (Notification Engine): Báo biến động số dư và kết quả giao dịch.

## M7: Staff/Admin Operations & Backend V1 Finalization
- Hoàn thiện cổng API dành cho nhân viên (Staff) và Quản trị viên (Admin).
- API Quản lý khách hàng toàn cục (Tìm kiếm, Phân trang SQL-level).
- Thao tác khóa/mở khóa tài khoản (Suspend/Active).
- Cơ chế **Audit Logging** bắt buộc cho các thao tác nhạy cảm từ Admin/Staff.
- Hoàn thiện Backend V1 (100% test pass).

## M8: Frontend UI (FE1 & FE2)
- **FE1 (Auth & App Shell)**:
  - Tích hợp xác thực với Pinia Store (Access Token in-memory, Refresh Token in localStorage).
  - Middleware bảo vệ Router (`customer` guard, `admin` guard).
  - Hoàn thiện App Shell (Global Layout, Navbar, Sidebar) với Quasar.
- **FE2 (Customer Banking Dashboard)**:
  - Trang Tổng quan (Dashboard): Thống kê số dư, hóa đơn chờ thanh toán, giao dịch gần đây (Render song song với `Promise.allSettled`).
  - Trang Danh sách Tài khoản & Chi tiết Tài khoản (Tính năng copy số tài khoản, ẩn hiện số dư).
  - Trang Quản lý Hồ sơ Cá nhân (Profile).
  - Tích hợp gọi API Backend thông qua `$fetch` Nuxt.
  - Các module Types, Services và Utils formatter.

---

> Toàn bộ hệ thống hiện đã hoạt động trơn tru từ Frontend đến Backend và có thể được khởi chạy chỉ với một lệnh duy nhất: `docker compose up --build -d`.
