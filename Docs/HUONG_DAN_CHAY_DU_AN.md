# 🚀 HƯỚNG DẪN CÀI ĐẶT VÀ CHẠY DỰ ÁN INTERLINK BANKING (LOCALLINK)

Tài liệu hướng dẫn chi tiết từng bước từ cài đặt môi trường, khởi chạy hệ thống bằng Docker Compose, chạy từng phần cho lập trình viên, đến kiểm thử các tính năng Authentication và phân quyền (RBAC).

---

## 📌 1. Yêu cầu Hệ thống (Prerequisites)

Trước khi bắt đầu, máy tính của bạn cần cài đặt sẵn:

| Công cụ | Phiên bản tối thiểu | Mục đích |
| :--- | :--- | :--- |
| **Docker Desktop** | Mới nhất (hỗ trợ Compose V2) | Chạy toàn bộ stack (API, Web, SQL Server) |
| **.NET SDK** | .NET 10.0 (nếu chạy local không qua Docker) | Phát triển Backend API |
| **Node.js** | Node.js 22+ hoặc 24 LTS | Phát triển Frontend Nuxt 4 / Vue 3 |
| **Git** | Mới nhất | Quản lý mã nguồn |

---

## ⚙️ 2. Chuẩn bị File Cấu hình Môi trường

1. Mở thư mục gốc của dự án trong terminal hoặc VS Code.
2. Kiểm tra xem đã có file `.env` chưa. Nếu chưa, hãy tạo từ file mẫu `.env.example`:

```powershell
# Trên Windows PowerShell
Copy-Item .env.example -Destination .env
```

```bash
# Trên Linux / macOS / Git Bash
cp .env.example .env
```

Nội dung file `.env` mặc định phục vụ môi trường phát triển (Development):
```ini
# Database Configuration (SQL Server 2022)
DB_NAME=LocalLinkDb
DB_USER=sa
DB_PASSWORD=YourStrongPassword123!
SQL_SERVER_PORT=1433

# Backend API Configuration
API_PORT=8080
ASPNETCORE_ENVIRONMENT=Development

# JWT Authentication Configuration
JWT_SECRET=LocalLink_Dev_Super_Secret_Key_Minimum_32_Bytes_Long_2026_Secure_Token_Key!
JWT_ISSUER=LocalLink
JWT_AUDIENCE=LocalLink.Client
JWT_ACCESS_TOKEN_MINUTES=15
JWT_REFRESH_TOKEN_DAYS=7

# Frontend Nuxt Configuration
WEB_PORT=3000
NUXT_PUBLIC_API_BASE_URL=http://localhost:8080
```

---

## 🐳 3. Cách 1: Khởi chạy Toàn bộ Hệ thống bằng Docker Compose (Khuyên Dùng)

Đây là cách nhanh nhất và chuẩn nhất để chạy toàn bộ hệ thống gồm **SQL Server 2022**, **Backend .NET 10 API**, và **Frontend Nuxt 4 Web**.

### Bước 1: Build và Khởi động Container
Tại thư mục gốc dự án, chạy lệnh:

```powershell
docker compose up --build -d
```

> [!NOTE]
> Khi khởi động lần đầu, hệ thống sẽ tự động:
> 1. Tạo database `LocalLinkDb` trong SQL Server.
> 2. Tự động áp dụng toàn bộ các **EF Core Migrations**.
> 3. Tự động chạy **Database Seeder** tạo 3 Role, 1 Admin, 2 Khách hàng mẫu, tài khoản ngân hàng và băm mật khẩu chuẩn ASP.NET Core.

### Bước 2: Kiểm tra Trạng thái Container
Chạy lệnh:
```powershell
docker compose ps
```
Cả 3 service phải ở trạng thái `running` (hoặc `healthy`):
- `locallink-sqlserver` $\rightarrow$ Cổng `1433`
- `locallink-api` $\rightarrow$ Cổng `8080`
- `locallink-web` $\rightarrow$ Cổng `3000`

### Bước 3: Xem Log hệ thống
```powershell
# Xem log realtime của API
docker compose logs -f locallink-api

# Xem log realtime của Frontend
docker compose logs -f locallink-web
```

---

## 🌐 4. Các Đường dẫn Truy cập Ứng dụng

Sau khi khởi chạy thành công, bạn có thể mở trình duyệt và truy cập các địa chỉ sau:

| Ứng dụng / Dịch vụ | Đường dẫn URL | Mô tả |
| :--- | :--- | :--- |
| **Giao diện Web Frontend** | [http://localhost:3000](http://localhost:3000) | Dashboard & Màn hình chính Nuxt 4 |
| **Tài liệu API (Swagger UI)** | [http://localhost:8080/swagger](http://localhost:8080/swagger) | Giao diện tương tác và test API trực tiếp |
| **Kiểm tra Sức khỏe (Health Check)** | [http://localhost:8080/health](http://localhost:8080/health) | Trạng thái API và kết nối SQL Server |
| **Thông tin Hệ thống (System Info)** | [http://localhost:8080/api/system](http://localhost:8080/api/system) | Telemetry runtime của hệ thống |

---

## 🔑 5. Danh sách Tài khoản Đăng nhập Mẫu (Demo Credentials)

> [!IMPORTANT]
> Đây là các tài khoản thử nghiệm dành riêng cho môi trường phát triển (Development).

| Loại tài khoản | Email đăng nhập | Mật khẩu mặc định | Vai trò (Role) | Hồ sơ khách hàng |
| :--- | :--- | :--- | :--- | :--- |
| **Quản trị viên (Admin)** | `admin@locallink.local` | `LocalLink@123` | `ADMIN` | Toàn quyền hệ thống |
| **Khách hàng 1** | `customer1@locallink.local` | `LocalLink@123` | `CUSTOMER` | `Nguyen Van An` (STK: `1000000001` - Số dư: 25.000.000đ) |
| **Khách hàng 2** | `customer2@locallink.local` | `LocalLink@123` | `CUSTOMER` | `Tran Thi Binh` (STK: `1000000002` - Số dư: 15.000.000đ) |

---

## 🧪 6. Hướng dẫn Kiểm thử API trên Swagger UI

### Bước 1: Mở Swagger
Truy cập [http://localhost:8080/swagger](http://localhost:8080/swagger).

### Bước 2: Đăng nhập lấy Token
1. Tìm endpoint `POST /api/v1/auth/login`.
2. Bấm **Try it out**, nhập JSON:
```json
{
  "email": "customer1@locallink.local",
  "password": "LocalLink@123"
}
```
3. Bấm **Execute**.
4. Response trả về mã `200 OK` kèm theo chuỗi `accessToken` (JWT) và `refreshToken`.

### Bước 3: Xác thực Swagger (Authorize)
1. Copy toàn bộ chuỗi trong trường `accessToken`.
2. Cuộn lên đầu trang Swagger, bấm vào nút **Authorize** (hình chiếc ổ khóa màu xanh lá).
3. Trong ô giá trị, nhập:
   ```text
   Bearer <dán_access_token_vào_đây>
   ```
4. Bấm **Authorize** $\rightarrow$ **Close**.

### Bước 4: Kiểm tra các Endpoint có bảo vệ
- **Xem thông tin cá nhân**: Gọi `GET /api/v1/auth/me` $\rightarrow$ Bấm **Execute** $\rightarrow$ Trả về thông tin khách hàng `Nguyen Van An` và role `CUSTOMER`.
- **Test RBAC Khách hàng**: Gọi `GET /api/v1/auth/test/customer` $\rightarrow$ Trả về `200 OK`.
- **Test chặn quyền Admin**: Gọi `GET /api/v1/auth/test/admin` $\rightarrow$ Bị chặn với mã `403 Forbidden` (do token thuộc role CUSTOMER).
- **Test xoay vòng Token (Refresh)**: Gọi `POST /api/v1/auth/refresh` với `refreshToken` $\rightarrow$ Trả về cặp token mới và tự động hủy token cũ.

---

## 💻 7. Cách 2: Chạy Từng Phần Cho Lập trình viên (Local Development)

Nếu bạn muốn debug trực tiếp backend hoặc frontend mà không build lại container:

### 1. Chỉ chạy SQL Server trên Docker:
```powershell
docker compose up -d locallink-sqlserver
```

### 2. Chạy Backend .NET 10:
```powershell
cd backend
dotnet run --project src/LocalLink.API
```
Backend API sẽ lắng nghe tại `http://localhost:8080`.

### 3. Chạy Frontend Nuxt 4:
Mở một cửa sổ terminal mới:
```powershell
cd frontend
npm install
npm run dev
```
Frontend Web sẽ lắng nghe tại `http://localhost:3000`.

---

## 🔄 8. Các Lệnh Quản lý Cơ sở Dữ liệu & Reset Dữ liệu

### Reset Database về trạng thái ban đầu (Clean Database):
Khi bạn muốn xóa sạch dữ liệu cũ và nạp lại dữ liệu mẫu từ đầu:

```powershell
# Cách 1: Dùng script PowerShell có sẵn
./scripts/db-reset.ps1

# Cách 2: Dùng lệnh Docker trực tiếp
docker compose down -v
docker compose up --build -d
```

### Cập nhật Migration vào Database:
```powershell
# Dùng script
./scripts/db-update.ps1

# Hoặc dùng dotnet-ef trực tiếp
cd backend
dotnet dotnet-ef database update --project src/LocalLink.Infrastructure --startup-project src/LocalLink.API
```

---

## 🛡️ 9. Chạy Bộ Kiểm thử Tự động (Automated Unit Tests)

Dự án có sẵn bộ Unit Test đầy đủ cho Authentication, Password Hashing, JWT, Token Rotation và RBAC:

```powershell
dotnet test backend/LocalLink.sln
```

Kết quả mong đợi:
```text
Passed!  - Failed: 0, Passed: 14, Skipped: 0, Total: 14
```

---

## 🛑 10. Tắt Hệ thống Khi Không Sử Dụng

- **Tắt hệ thống và giữ nguyên dữ liệu trong database**:
  ```powershell
  docker compose down
  ```

- **Tắt hệ thống và xóa sạch toàn bộ volume database**:
  ```powershell
  docker compose down -v
  ```

---

## ❓ 11. Xử lý Lỗi Thường Gặp (Troubleshooting)

### 1. Lỗi Cổng 1433 hoặc 8080 hoặc 3000 bị chiếm dụng:
- **Nguyên nhân**: Đang có ứng dụng khác hoặc SQL Server local đang chạy chiếm cổng.
- **Khắc phục**: Đổi số cổng ngoài máy host trong file `.env` (ví dụ: `SQL_SERVER_PORT=1434`, `API_PORT=8081`).

### 2. Không kết nối được SQL Server khi API khởi động:
- **Nguyên nhân**: SQL Server cần khoảng 5-10 giây để khởi tạo hoàn tất.
- **Khắc phục**: Docker Compose đã có cấu hình `healthcheck` tự động chờ SQL Server sẵn sàng. Bạn chỉ cần chờ vài giây hoặc chạy `docker compose restart locallink-api`.

### 3. Đăng nhập báo lỗi 401 Unauthorized:
- **Khắc phục**: Đảm bảo bạn nhập đúng định dạng email (ví dụ: `customer1@locallink.local`) và mật khẩu `LocalLink@123` (chú ý chữ hoa/thường).
