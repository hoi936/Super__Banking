# Chuyển tiền (Transfer) Frontend

## Routes
- `/transfer` : Giao diện chuyển tiền nội bộ. Hỗ trợ query param `?sourceAccount=<id>` để điền sẵn tài khoản nguồn.

## Các tính năng chính
- **Tài khoản nguồn**: Chỉ hiển thị các tài khoản ở trạng thái `ACTIVE` của khách hàng.
- **Tài khoản đích**: Hỗ trợ nhập trực tiếp hoặc chọn từ Danh bạ.
- **Tra cứu tên người nhận**: Gọi API `GET /api/v1/accounts/lookup/{accountNumber}` để kiểm tra thông tin.
- **Số tiền**: Tự động format, không được vượt quá số dư `balance` hiện tại của tài khoản nguồn.
- **Xác nhận & Idempotency Key**:
  - Khi bắt đầu bấm "Tiếp tục" ở bước 1, frontend sinh một UUID (`Idempotency-Key`).
  - Nút "Xác nhận chuyển tiền" bị disable khi đang call API (ngăn Double Submit).
  - Nếu call lỗi mạng (fetch failed), Key được giữ nguyên để thử lại an toàn.
- **Biên lai thành công**: Refetch lại số dư tài khoản, cho phép lưu danh bạ người nhận, và copy mã giao dịch.

## API Services
- **`transferService.ts`**:
  - `createTransfer(request, idempotencyKey)`
  - `getTransfers()`
  - `getTransferDetail(id)`

## Responsive
- Hỗ trợ tốt trên Desktop và Mobile, sử dụng `q-stepper` dọc hoặc ngang tùy vào thiết kế của Quasar. Mọi action đều dễ bấm, không bị tràn màn hình.
