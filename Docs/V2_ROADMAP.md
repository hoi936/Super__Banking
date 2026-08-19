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

3. Payment Gateway
   - Adapter giả lập VNPay/Momo/Stripe.
   - Luồng tạo payment intent, callback, đối soát và nạp tiền vào tài khoản.

## Phase 3 - Experience & Loyalty
1. PFM
   - API thống kê chi tiêu theo tháng, loại giao dịch và tài khoản.
   - Biểu đồ pie/bar trên dashboard khách hàng.

2. Dark Mode
   - Toggle toàn hệ thống.
   - Lưu lựa chọn ở local storage.

3. Rewards
   - Cộng điểm khi thanh toán hóa đơn.
   - Danh mục voucher và đổi điểm.

## Phase 4 - Admin & Ops
1. Maker-Checker
   - Giao dịch lớn hơn 500.000.000 VND chuyển sang trạng thái chờ duyệt.
   - Staff tạo yêu cầu, Admin duyệt/từ chối.

2. Advanced Analytics
   - Báo cáo dòng tiền, khách hàng mới, sản phẩm mới.
   - Xuất Excel/PDF theo khoảng thời gian.

## Lát cắt triển khai đầu tiên
Module đầu tiên của V2 là Term Deposits backend foundation:
- Domain entity `TermDeposit`.
- Enum trạng thái/kỳ hạn.
- DTO và service mở sổ, xem danh sách, xem chi tiết, tất toán khi đến hạn.
- API `/api/v1/term-deposits`.
- EF configuration và DbSet.
