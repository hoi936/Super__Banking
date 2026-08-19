# Quản lý Hóa đơn (Bills) Frontend

## Routes
- `/bills` : Danh sách hóa đơn của khách hàng.
- `/bills/[id]` : Chi tiết hóa đơn và giao diện Wizard thanh toán (Payment Stepper).

## Các tính năng chính
- **Danh sách Hóa đơn**:
  - Hỗ trợ xem dạng danh sách trên Mobile (responsive).
  - Bộ lọc: Trạng thái (UNPAID, PAID, OVERDUE, CANCELLED), Loại dịch vụ, Hạn từ ngày/Đến ngày.
  - Trạng thái màu sắc (Badge) rõ ràng: `Chưa thanh toán` (Vàng), `Đã thanh toán` (Xanh), `Quá hạn` (Đỏ).
- **Chi tiết & Thanh toán (Wizard / QStepper)**:
  - Tích hợp 3 bước ngay trên một màn hình:
    1. **Thông tin hóa đơn**: Nếu ở trạng thái UNPAID, hiển thị nút "Thanh toán ngay".
    2. **Xác nhận**: Yêu cầu người dùng chọn "Tài khoản nguồn", kiểm tra số dư trực tiếp trước khi cho phép xác nhận. Hệ thống tự sinh `Idempotency-Key` (UUID) bảo vệ khi Submit.
    3. **Biên lai (Receipt)**: Hiển thị đầy đủ biên lai nếu thanh toán thành công, liên kết về Danh sách thanh toán.

## API Services
- **`billService.ts`**:
  - `getBills(filters)`: Hỗ trợ PagedResult, lọc hóa đơn.
  - `getBill(id)`: Lấy dữ liệu 1 hóa đơn, kiểm tra quyền sở hữu tuyệt đối.

## Tích hợp Dashboard
- Hóa đơn chưa thanh toán sẽ được load trong màn hình Dashboard (sử dụng service `getBills` qua `useDashboard()`).
