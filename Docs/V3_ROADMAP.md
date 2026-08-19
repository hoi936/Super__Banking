# LocalLink / InterLink Banking V3 Roadmap

## Mục tiêu V3
Phiên bản V3 sẽ tập trung vào việc nâng cao trải nghiệm người dùng (UX/UI), xây dựng hệ sinh thái khách hàng thân thiết (Loyalty), và hoàn thiện các quy trình quản trị vận hành nội bộ (Ops) cho Admin/Staff.

## Phase 1 - Experience & Loyalty (Chuyển từ V2)
1. **PFM (Personal Finance Management)**
   - API thống kê chi tiêu theo tháng, loại giao dịch và phân bổ theo tài khoản.
   - Hiển thị biểu đồ (Pie/Bar chart) trên màn hình Dashboard khách hàng.
   - Theo dõi ngân sách và gợi ý tiết kiệm.

2. **Dark Mode**
   - Toggle giao diện Sáng/Tối toàn hệ thống (Frontend Khách hàng và Admin).
   - Tối ưu hóa màu sắc Quasar variables.
   - Lưu lựa chọn ở local storage hoặc CSDL (User Preferences).

3. **Rewards & Loyalty**
   - Tích điểm (Coins) khi thanh toán hóa đơn, nạp thẻ, hoặc dùng thẻ tín dụng.
   - Danh mục Voucher quà tặng và quy trình đổi điểm (Redeem).
   - Hạng thành viên (Silver, Gold, Platinum).

## Phase 2 - Admin & Ops (Chuyển từ V2)
1. **Maker-Checker (Phê duyệt kép)**
   - Cấu hình luồng phê duyệt: Giao dịch lớn hơn 500.000.000 VND tự động chuyển sang trạng thái chờ duyệt.
   - Phân quyền Staff tạo yêu cầu (Maker) và Admin duyệt/từ chối (Checker).
   - Áp dụng cho các giao dịch giải ngân, hoàn tiền lớn.

2. **Advanced Analytics (Báo cáo nâng cao)**
   - Dashboard quản trị toàn diện: dòng tiền In/Out, tốc độ tăng trưởng khách hàng mới, sản phẩm bán chạy.
   - Công cụ xuất báo cáo (Export Excel/PDF) theo khoảng thời gian tùy chọn.
   - Biểu đồ biến động số dư toàn hệ thống (System Ledger).
