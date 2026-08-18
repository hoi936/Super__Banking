# 📚 TÀI LIỆU HƯỚNG DẪN TÍCH HỢP BACKEND API CHO FRONTEND (FE)

Tài liệu này dành cho các lập trình viên **Frontend (FE)** khi xây dựng giao diện Web (Nuxt 4 / Vue 3 / React) hoặc Mobile App kết nối với Backend **InterLink Banking API Gateway**.

---

## 🌐 1. Cấu hình Cơ sở (Base URL & Headers)

- **Môi trường Development**: `http://localhost:8080`
- **Prefix API Versioning**: `/api/v1`
- **Content-Type**: `application/json`

---

## 🔐 2. Chuẩn Xác thực (JWT Bearer Authentication)

Tất cả các API yêu cầu đăng nhập (`[Authorize]`) phải gửi kèm Header:

```http
Authorization: Bearer <access_token_nhận_từ_login_hoặc_refresh>
```

### Luồng xử lý Token khuyến nghị cho Frontend:
1. **Lưu trữ Access Token**: Lưu trong memory state (Pinia / Vuex / Context) hoặc `cookie` an toàn. Token có hiệu lực **15 phút**.
2. **Lưu trữ Refresh Token**: Lưu trong `localStorage` hoặc `HttpOnly Cookie`. Token có hiệu lực **7 ngày**.
3. **Tự động làm mới Token (Axios Interceptor)**:
   - Khi API trả về mã lỗi `401 Unauthorized`, Frontend bắt sự kiện và tự động gọi `POST /api/v1/auth/refresh` với `refreshToken`.
   - Lấy cặp `accessToken` mới và gửi lại request ban đầu mà người dùng không bị văng ra màn hình đăng nhập.
   - Nếu `POST /api/v1/auth/refresh` cũng trả về `401`, xóa sạch token và chuyển hướng về màn hình Login.

---

## ⚠️ 3. Chuẩn Báo lỗi (RFC 7807 ProblemDetails)

Backend trả về lỗi theo định dạng chuẩn quốc tế **RFC 7807 ProblemDetails**:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Cannot add your own bank account as a beneficiary.",
  "instance": "/api/v1/beneficiaries"
}
```

### Bảng mã trạng thái HTTP phổ biến:
| Mã HTTP | Tên chuẩn | Ý nghĩa đối với Frontend |
| :--- | :--- | :--- |
| `200 OK` | Success | Yêu cầu xử lý thành công, có dữ liệu trả về |
| `201 Created` | Created | Tạo mới tài nguyên thành công (ví dụ: thêm thụ hưởng) |
| `204 No Content` | No Content | Xóa thành công, không có body trả về |
| `400 Bad Request` | Bad Request | Dữ liệu gửi lên sai định dạng hoặc vi phạm nghiệp vụ |
| `401 Unauthorized` | Unauthorized | Chưa đăng nhập hoặc Access Token đã hết hạn |
| `403 Forbidden` | Forbidden | Đã đăng nhập nhưng không có quyền truy cập endpoint |
| `404 Not Found` | Not Found | Không tìm thấy tài nguyên (hoặc bị ẩn quyền sở hữu) |
| `409 Conflict` | Conflict | Xung đột dữ liệu (ví dụ: thêm trùng số tài khoản thụ hưởng) |
| `500 Server Error` | Server Error | Lỗi hệ thống Backend |

---

## 📑 4. Chuẩn Phân trang (Pagination)

Tất cả các API danh sách có phân trang (như danh sách khách hàng của Admin) trả về theo định dạng:

```json
{
  "items": [ ... ],
  "page": 1,
  "pageSize": 20,
  "totalItems": 45,
  "totalPages": 3
}
```

---

## 📂 5. Danh mục Chi tiết các Module API

| Module | Đường dẫn File MD | Mô tả |
| :--- | :--- | :--- |
| **Authentication & RBAC** | [auth.md](auth.md) | Đăng nhập, làm mới token, đăng xuất, lấy thông tin cá nhân, phân quyền |
| **Customer Profile** | [customers.md](customers.md) | Xem hồ sơ chính mình, cập nhật thông tin cá nhân |
| **Bank Accounts** | [accounts.md](accounts.md) | Xem danh sách tài khoản, chi tiết tài khoản, tra cứu số tài khoản |
| **Beneficiaries** | [beneficiaries.md](beneficiaries.md) | Danh bạ người thụ hưởng (xem, thêm, xóa) |
| **Transfers (Chuyển tiền)** | [transfers.md](transfers.md) | Chuyển tiền nội bộ, bảo toàn dòng tiền, Idempotency-Key, lịch sử chuyển tiền |
| **Transactions (Sổ cái)** | [transactions.md](transactions.md) | Sổ cái giao dịch, biến động số dư các tài khoản |
| **Bills (Hóa đơn)** | [bills.md](bills.md) | Danh sách và chi tiết hóa đơn dịch vụ (Điện, Nước, Internet) |
| **Payments (Thanh toán)** | [payments.md](payments.md) | Thanh toán hóa đơn, chống trừ tiền 2 lần qua Idempotency-Key, lịch sử thanh toán |
| **Notifications (Thông báo)** | [notifications.md](notifications.md) | Danh sách thông báo, số lượng chưa đọc, đánh dấu đã đọc |
| **Staff & Admin Management** | [admin.md](admin.md) | Quản trị khách hàng, khóa/mở khóa tài khoản & khách hàng |
