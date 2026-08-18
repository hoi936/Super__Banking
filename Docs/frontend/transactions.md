# Lịch sử Giao dịch (Transactions) Frontend

## Routes
- `/transactions` : Danh sách lịch sử giao dịch (sổ cái).
- `/transactions/[id]` : Chi tiết một giao dịch cụ thể.

## Các tính năng chính
- **Bảng danh sách**:
  - Hỗ trợ xem dạng danh sách trên Mobile (responsive).
  - Màu sắc số tiền được tự động xử lý dựa trên loại giao dịch (`DEPOSIT` -> `+` màu xanh, `WITHDRAWAL`/`PAYMENT` -> `-` màu đỏ). Đối với `TRANSFER` (chuyển tiền nội bộ), cần lọc theo Account đang xem để hiện `+` hoặc `-`. (Frontend hiện áp dụng filter accountId cho logic này).
- **Bộ lọc (Filters)**:
  - Account: Lọc theo 1 tài khoản cụ thể.
  - Loại GD: Chuyển tiền, Thanh toán, Nạp, Rút.
  - Ngày (From / To): Validate không cho phép From > To.
- **Phân trang (Pagination)**:
  - Sử dụng phân trang từ Server, pageSize mặc định 10.
- **Chi tiết giao dịch**:
  - Hiển thị đầy đủ người gửi, người nhận, thời gian xử lý, mã giao dịch và trạng thái.
  - Cho phép copy nhanh mã giao dịch vào khay nhớ tạm.

## API Services
- **`transactionService.ts`**:
  - `getTransactions(params)`
  - `getTransaction(id)`

## Tích hợp
- Các nút "Lịch sử giao dịch" ở Dashboard và Chi tiết Tài khoản được liên kết trực tiếp với `/transactions?accountId={id}`.
