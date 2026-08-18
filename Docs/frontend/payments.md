# Thanh toán Hóa đơn (Payments) Frontend

## Routes
- `/payments` : Lịch sử giao dịch thanh toán hóa đơn.
- `/payments/[id]` : Chi tiết một giao dịch thanh toán (Biên lai).

## Các tính năng chính
- **Cơ chế Idempotency chống trùng lặp (Quan trọng nhất)**:
  - Frontend luôn sinh một UUID (`crypto.randomUUID()`) trong Bước Xác Nhận tại `/bills/[id]`.
  - Nếu kết nối mạng gián đoạn, UUID được giữ nguyên và cho phép người dùng thử lại. Backend sẽ bắt UUID này và trả về biên lai thành công cũ thay vì trừ tiền lần 2.
- **Không gửi Amount từ Frontend**: 
  - Khác với API chuyển tiền nội bộ, Request thanh toán (`CreatePaymentRequest`) **chỉ truyền `billId` và `accountId`**. 
  - Backend đảm nhận hoàn toàn nghiệp vụ về giá tiền, ngăn ngừa rủi ro người dùng can thiệp giảm giá trị hóa đơn.
- **Double Submit Protection**:
  - Gắn `:loading` và vô hiệu hóa (`disable`) nút xác nhận khi API đang xử lý.
- **Sổ cái Lịch sử Thanh toán (`/payments`)**:
  - Tách biệt so với Sổ cái tổng hợp (`/transactions`). 
  - Cho phép lọc theo Loại hóa đơn, trạng thái, thời gian. Giao diện được tối ưu với Responsive Table (PC) & List Card (Mobile).

## API Services
- **`paymentService.ts`**:
  - `payBill(request, idempotencyKey)`: API POST kết hợp truyền Headers.
  - `getPayments(filters)`
  - `getPayment(id)`
