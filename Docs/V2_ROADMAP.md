# LocalLink / InterLink Banking V2 Roadmap

## Mục tiêu V2
V2 mở rộng LocalLink từ ngân hàng số cơ bản của V1 thành nền tảng tài chính cá nhân có nhiều sản phẩm, tích hợp giả lập bên thứ ba, trải nghiệm tốt hơn và quy trình vận hành chặt chẽ hơn.

## Nguyên tắc triển khai
- Giữ kiến trúc Modular Monolith + Clean Architecture của V1.
- Mỗi nghiệp vụ mới đi theo lát cắt dọc: Domain, Application DTO/Interface, Infrastructure Service, API Controller, EF Configuration, migration, seeder, frontend service, frontend page.
- Các nghiệp vụ tiền phải có idempotency, audit log, notification và transaction ledger nếu làm thay đổi số dư.
- Tính năng phụ thuộc tích hợp ngoài sẽ bắt đầu bằng adapter giả lập để demo ổn định trước khi nối API thật.

## Phase 1 - Financial Core
1. Term Deposits
   - Khách hàng mở sổ tiết kiệm từ tài khoản VND đang hoạt động.
   - Kỳ hạn hỗ trợ: 1, 3, 12 tháng.
   - Lãi suất cấu hình theo kỳ hạn.
   - Khi mở sổ: trừ tiền tài khoản nguồn, tạo giao dịch ledger, audit log, notification.
   - Khi đến hạn: service tất toán cộng gốc + lãi vào tài khoản nguồn.

2. Napas 24/7 Simulation
   - Danh mục ngân hàng ngoài.
   - Lookup tài khoản/thẻ giả lập.
   - Chuyển tiền liên ngân hàng với phí, idempotency và trạng thái xử lý.

3. Cards
   - Danh sách thẻ ghi nợ/tín dụng.
   - Khóa/mở thẻ khẩn cấp.
   - Thiết lập hạn mức giao dịch.

4. Loans
   - Khách hàng nộp hồ sơ vay.
   - Admin duyệt/từ chối.
   - Giải ngân tự động vào tài khoản khi duyệt.

## Phase 2 - Third-party Integration Layer
1. Mobile Top-up
   - Adapter giả lập nhà cung cấp telco.
   - Nạp tiền/mua mã thẻ, ghi nhận payment và notification.

2. QR Pay
   - Tạo QR nhận tiền cá nhân.
   - Parse QR chuẩn VietQR ở mức demo để điền nhanh form chuyển tiền.
   - Foundation đã triển khai API tạo/parse payload, màn hình `/qr-pay` và prefill sang chuyển khoản nội bộ.

3. Payment Gateway
   - Adapter giả lập VNPay/Momo/Stripe.
   - Luồng tạo payment intent, callback, đối soát và nạp tiền vào tài khoản.

> **Lưu ý:** Các chức năng thuộc Phase 3 (PFM, Dark Mode, Rewards) và Phase 4 (Maker-Checker, Advanced Analytics) đã được dời sang lộ trình [V3_ROADMAP.md](./V3_ROADMAP.md) để tập trung hoàn thiện và đóng gói phiên bản V2.

## Lát cắt triển khai đầu tiên
Module đầu tiên của V2 là Term Deposits:
- Domain entity `TermDeposit`.
- Enum trạng thái/kỳ hạn.
- DTO và service mở sổ, xem danh sách, xem chi tiết, tất toán khi đến hạn.
- API `/api/v1/term-deposits`.
- EF configuration, DbSet và migration `AddTermDeposits`.
- Frontend service, type contract, menu khách hàng và trang `/term-deposits` để mở sổ/tra cứu danh sách.

## Trạng thái triển khai
- `2026-08-19`: Hoàn tất foundation Term Deposits backend và màn hình khách hàng cơ bản.
- `2026-08-19`: Hoàn tất foundation Napas 24/7 simulation: ngân hàng ngoài giả lập, lookup người nhận, chuyển liên ngân hàng có phí, ledger, audit, notification và màn hình `/napas`.
- `2026-08-19`: Hoàn tất foundation Cards: bảng thẻ, seed thẻ demo, API danh sách/khóa/mở/cập nhật hạn mức/settings và màn hình `/cards` với thẻ 3D.
- `2026-08-19`: Hoàn tất foundation Loans: khách hàng nộp hồ sơ vay tại `/loans`, admin duyệt/từ chối tại `/admin/loans`, giải ngân tự động vào tài khoản khi duyệt, ledger/audit/notification đầy đủ.
- `2026-08-19`: Hoàn tất foundation Mobile Top-up: adapter nhà mạng giả lập, danh mục gói nạp tiền/mã thẻ/data, debit tài khoản, ledger/audit/notification/idempotency và màn hình `/mobile-topup`.
- `2026-08-19`: Hoàn tất foundation QR Pay: API `/api/v1/qr-pay/receive-payload`, `/api/v1/qr-pay/parse`, màn hình `/qr-pay`, tạo QR nhận tiền LocalLink, parse payload LocalBank/VietQR demo và chuyển tiếp sang `/transfer` hoặc `/napas`.
- `2026-08-19`: **[NEW]** Hoàn tất Cổng thanh toán (Payment Gateway): tích hợp nạp tiền từ VNPay/Momo/Stripe, tạo Webhook xử lý Callback, giao diện `/deposit` và luồng giả lập thanh toán.
- Đã kiểm tra build backend, EF không còn pending model changes.
- Đã kiểm tra build frontend Nuxt.
- Đã smoke test API health và toàn bộ các routes/components của V2.

**=> V2 ĐÃ HOÀN TẤT TOÀN BỘ CÁC MỤC TIÊU PHASE 1 & PHASE 2.**
