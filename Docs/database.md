# 🗄️ LocalLink — Complete Database Architecture & Specification

Tài liệu thiết kế cơ sở dữ liệu chi tiết cho dự án **LocalLink (InterLink Banking Platform)** — Nền tảng Ngân hàng Số Khu vực & Dịch vụ Tiện ích Địa phương.

---

## 1. Tổng quan Cơ sở Dữ liệu

- **Database Engine**: Microsoft SQL Server 2022 (`mcr.microsoft.com/mssql/server:2022-latest`)
- **Database Name**: `LocalLinkDb`
- **ORM / Persistence**: Entity Framework Core 10.0 (Code-First Migrations)
- **Container Volume**: `locallink_sql_data` mount tới `/var/opt/mssql` (Đảm bảo lưu trữ bền vững)
- **Default Currency**: `VND`
- **Financial Precision**: `decimal(18, 2)` cho toàn bộ các trường số tiền (Balance, Amount).
- **Concurrency Control**: Sử dụng `RowVersion` (`rowversion` / `byte[]`) trên bảng `BankAccounts` nhằm ngăn ngừa tình trạng Race Condition / Double Spending trong giao dịch tài chính.

---

## 2. Sơ đồ Thực thể Liên kết (Mermaid ER Diagram)

```mermaid
erDiagram
    USERS ||--o| CUSTOMERS : "1:0..1 has profile"
    USERS ||--o{ USER_ROLES : "1:N has roles"
    ROLES ||--o{ USER_ROLES : "1:N assigned to"
    USERS ||--o{ REFRESH_TOKENS : "1:N owns"
    USERS ||--o{ NOTIFICATIONS : "1:N receives"
    USERS ||--o{ AUDIT_LOGS : "1:N generates"

    CUSTOMERS ||--o{ BANK_ACCOUNTS : "1:N owns"
    CUSTOMERS ||--o{ BENEFICIARIES : "1:N saves"
    CUSTOMERS ||--o{ BILLS : "1:N receives"

    BANK_ACCOUNTS ||--o{ BENEFICIARIES : "1:N is target of"
    BANK_ACCOUNTS ||--o{ TRANSACTIONS : "1:N as source/dest"
    BANK_ACCOUNTS ||--o{ TRANSFERS : "1:N as source/dest"
    BANK_ACCOUNTS ||--o{ PAYMENTS : "1:N source account"

    TRANSACTIONS ||--o| TRANSFERS : "1:0..1 details"
    TRANSACTIONS ||--o| PAYMENTS : "1:0..1 details"
    BILLS ||--o| PAYMENTS : "1:0..1 paid by"

    USERS {
        Guid Id PK
        string Email UK
        string PasswordHash
        string Status
        datetime2 LastLoginAtUtc
        datetime2 CreatedAtUtc
        datetime2 UpdatedAtUtc
    }

    ROLES {
        Guid Id PK
        string Name UK
        string Description
        datetime2 CreatedAtUtc
    }

    USER_ROLES {
        Guid UserId PK_FK
        Guid RoleId PK_FK
        datetime2 CreatedAtUtc
    }

    REFRESH_TOKENS {
        Guid Id PK
        Guid UserId FK
        string TokenHash
        datetime2 ExpiresAtUtc
        datetime2 CreatedAtUtc
        datetime2 RevokedAtUtc
        string CreatedByIp
        string RevokedByIp
    }

    CUSTOMERS {
        Guid Id PK
        Guid UserId FK_UK
        string CustomerCode UK
        string FullName
        date DateOfBirth
        string Gender
        string PhoneNumber
        string Address
        string Status
        datetime2 CreatedAtUtc
        datetime2 UpdatedAtUtc
    }

    BANK_ACCOUNTS {
        Guid Id PK
        Guid CustomerId FK
        string AccountNumber UK
        string AccountName
        string AccountType
        decimal Balance
        char Currency
        string Status
        rowversion RowVersion
        datetime2 CreatedAtUtc
        datetime2 UpdatedAtUtc
    }

    BENEFICIARIES {
        Guid Id PK
        Guid CustomerId FK
        Guid BeneficiaryAccountId FK
        string Nickname
        datetime2 CreatedAtUtc
    }

    TRANSACTIONS {
        Guid Id PK
        string ReferenceNumber UK
        string TransactionType
        Guid SourceAccountId FK
        Guid DestinationAccountId FK
        decimal Amount
        char Currency
        string Description
        string Status
        datetime2 CreatedAtUtc
        datetime2 CompletedAtUtc
    }

    TRANSFERS {
        Guid Id PK
        Guid TransactionId FK_UK
        Guid SourceAccountId FK
        Guid DestinationAccountId FK
        decimal Amount
        string Description
        string Status
        datetime2 CreatedAtUtc
        datetime2 CompletedAtUtc
    }

    BILLS {
        Guid Id PK
        Guid CustomerId FK
        string ProviderName
        string BillType
        string BillNumber UK
        decimal Amount
        date DueDate
        string Status
        datetime2 CreatedAtUtc
        datetime2 UpdatedAtUtc
    }

    PAYMENTS {
        Guid Id PK
        Guid BillId FK_UK
        Guid AccountId FK
        Guid TransactionId FK_UK
        decimal Amount
        string Status
        datetime2 PaidAtUtc
        datetime2 CreatedAtUtc
    }

    NOTIFICATIONS {
        Guid Id PK
        Guid UserId FK
        string Title
        string Message
        string Type
        bit IsRead
        datetime2 CreatedAtUtc
        datetime2 ReadAtUtc
    }

    AUDIT_LOGS {
        Guid Id PK
        Guid UserId FK
        string Action
        string EntityType
        string EntityId
        string Description
        string IpAddress
        datetime2 CreatedAtUtc
    }
```

---

## 3. Danh sách Bảng & Thực thể (Domain Modules)

### 3.1. Phân hệ Identity & Bảo mật
1. **`Users`**: Lưu trữ tài khoản định danh, email đăng nhập, trạng thái (`ACTIVE`, `SUSPENDED`, `LOCKED`, `CLOSED`).
2. **`Roles`**: Danh mục vai trò trong hệ thống (`CUSTOMER`, `STAFF`, `ADMIN`).
3. **`UserRoles`**: Bảng nối nhiều - nhiều giữa `Users` và `Roles` (Composite PK: `UserId`, `RoleId`).
4. **`RefreshTokens`**: Quản lý phiên đăng nhập và token refresh (Indexed: `UserId`, `ExpiresAtUtc`).

### 3.2. Phân hệ Khách hàng (Customer)
5. **`Customers`**: Thông tin hồ sơ khách hàng định danh (liên kết 1:1 với `Users`), mã định danh `CustomerCode` duy nhất, trạng thái (`ACTIVE`, `SUSPENDED`, `CLOSED`).
6. **`Beneficiaries`**: Danh bạ thụ hưởng lưu các tài khoản thường xuyên giao dịch (Unique Composite: `CustomerId`, `BeneficiaryAccountId`).

### 3.3. Phân hệ Ngân hàng & Sổ cái (Banking & Ledger)
7. **`BankAccounts`**: Tài khoản ngân hàng, số dư tài khoản (`Balance >= 0`), số tài khoản `AccountNumber` duy nhất, loại tài khoản (`CHECKING`, `SAVINGS`), cột `RowVersion` kiểm soát xung đột đồng thời.
8. **`Transactions`**: Sổ cái giao dịch tài chính bất biến (Immutable Financial Ledger Record), mã tham chiếu `ReferenceNumber` duy nhất, loại giao dịch (`TRANSFER`, `PAYMENT`, `DEPOSIT`, `WITHDRAWAL`), trạng thái (`PENDING`, `COMPLETED`, `FAILED`, `CANCELLED`).
9. **`Transfers`**: Metadata giao dịch chuyển khoản nội bộ/liên tài khoản, liên kết 1:1 với `Transactions`.

### 3.4. Phân hệ Thanh toán Tiện ích (Payments)
10. **`Bills`**: Hóa đơn dịch vụ địa phương (Điện, Nước, Internet, Học phí,...), mã hóa đơn `BillNumber` duy nhất, trạng thái (`UNPAID`, `PAID`, `OVERDUE`, `CANCELLED`).
11. **`Payments`**: Bản ghi thanh toán hóa đơn, liên kết 1:1 với `Bills` và 1:1 với `Transactions`.

### 3.5. Phân hệ Truyền thông & Kiểm toán (Communication & Audit)
12. **`Notifications`**: Thông báo biến động số dư, thanh toán, bảo mật hệ thống (Indexed: `UserId`, `IsRead`, `CreatedAtUtc`).
13. **`AuditLogs`**: Nhật ký kiểm toán an toàn tài chính dạng **Append-only** (không cho phép sửa hay xóa).

---

## 4. Ràng buộc An toàn & Ràng buộc Kiểm tra (Check Constraints)

| Bảng | Tên Constraint | Điều kiện SQL | Mục đích bảo vệ |
| :--- | :--- | :--- | :--- |
| `BankAccounts` | `CK_BankAccounts_Balance_NonNegative` | `[Balance] >= 0` | Ngăn chặn số dư tài khoản bị âm ở mức CSDL |
| `Transactions` | `CK_Transactions_Amount_Positive` | `[Amount] > 0` | Số tiền giao dịch bắt buộc phải lớn hơn 0 |
| `Transfers` | `CK_Transfers_Amount_Positive` | `[Amount] > 0` | Số tiền chuyển khoản bắt buộc phải lớn hơn 0 |
| `Bills` | `CK_Bills_Amount_Positive` | `[Amount] > 0` | Giá trị hóa đơn bắt buộc phải lớn hơn 0 |
| `Payments` | `CK_Payments_Amount_Positive` | `[Amount] > 0` | Số tiền thanh toán bắt buộc phải lớn hơn 0 |

---

## 5. Chiến lược Khóa ngoại (DeleteBehavior Strategy)

Để đảm bảo tính toàn vẹn dữ liệu tài chính và ngăn ngừa lỗi chu kỳ xóa chéo của SQL Server (*Multiple Cascade Paths Error*):
- **`DeleteBehavior.Restrict`**: Áp dụng cho toàn bộ các quan hệ tài chính (`Customer` $\rightarrow$ `BankAccount`, `BankAccount` $\rightarrow$ `Transaction`, `BankAccount` $\rightarrow$ `Transfer`, `Customer` $\rightarrow$ `Bill`, `Bill` $\rightarrow$ `Payment`, `Transaction` $\rightarrow$ `Payment`,...). Dữ liệu tài chính không bao giờ bị xóa theo tầng.
- **`DeleteBehavior.Cascade`**: Chỉ áp dụng cho các bảng kỹ thuật phụ thuộc trực tiếp vào User (`User` $\rightarrow$ `UserRole`, `User` $\rightarrow$ `RefreshToken`, `User` $\rightarrow$ `Notification`).
- **`DeleteBehavior.SetNull`**: Áp dụng cho `User` $\rightarrow$ `AuditLog` (nhật ký kiểm toán vẫn tồn tại bất biến ngay cả khi tài khoản user bị xóa).

---

## 6. Dữ liệu Khởi tạo Mẫu (Development Seed Data)

Hệ thống được trang bị **`DatabaseSeeder`** chạy tự động khi khởi động container và đảm bảo tính **Idempotent** (chạy nhiều lần không tạo dữ liệu trùng lặp):

### Vai trò (Roles):
- `CUSTOMER`: Khách hàng cá nhân.
- `STAFF`: Nhân viên / Giao dịch viên ngân hàng.
- `ADMIN`: Quản trị viên hệ thống.

### Khách hàng mẫu 1:
- **Email**: `customer1@locallink.local`
- **Mã khách hàng**: `CUS000001`
- **Họ tên**: `Nguyen Van An`
- **Số tài khoản**: `1000000001` (Checking Account)
- **Số dư ban đầu**: `25,000,000 VND`
- **Trạng thái**: `ACTIVE`

### Khách hàng mẫu 2:
- **Email**: `customer2@locallink.local`
- **Mã khách hàng**: `CUS000002`
- **Họ tên**: `Tran Thi Binh`
- **Số tài khoản**: `1000000002` (Checking Account)
- **Số dư ban đầu**: `15,000,000 VND`
- **Trạng thái**: `ACTIVE`

### Tài khoản Quản trị:
- **Email**: `admin@locallink.local`
- **Vai trò**: `ADMIN`
- **Trạng thái**: `ACTIVE`

---

## 7. Quy trình Quản lý CSDL cho Đội ngũ Phát triển (Team Workflow)

### Khi một lập trình viên thay đổi Entity:
1. Chỉnh sửa Entity trong `LocalLink.Domain/Entities/`.
2. Cập nhật Fluent API Configuration tương ứng trong `LocalLink.Infrastructure/Persistence/Configurations/`.
3. Sinh Migration mới:
   ```bash
   cd backend
   dotnet dotnet-ef migrations add <TenMigration> \
     --project src/LocalLink.Infrastructure \
     --startup-project src/LocalLink.API \
     --output-dir Persistence/Migrations
   ```
4. Kiểm tra mã migration được sinh ra và commit vào Git (cả file `.cs` và `Designer.cs`).
5. Đẩy lên nhánh làm việc: `git push origin feature/<ten-nhanh>`.

### Khi thành viên khác trong nhóm kéo code về:
1. `git pull origin dev`
2. Khởi động lại stack:
   ```bash
   docker compose up --build -d
   ```
   *Hệ thống API sẽ tự động nhận diện các migration mới và tự động apply vào database SQL Server mà không cần gửi nhận file `.bak` thủ công!*

---

## 8. Các lệnh Thao tác Nhanh (Database Commands)

### Reset Database sạch sẽ từ đầu (Clean Database):
```bash
# Sử dụng script
./scripts/db-reset.ps1   # Windows PowerShell
./scripts/db-reset.sh    # Linux / macOS / Bash

# Hoặc dùng Docker Compose trực tiếp
docker compose down -v
docker compose up --build -d
```

### Cập nhật Migration thủ công vào SQL Server (Local Dev không dùng Docker auto-migrate):
```bash
# Sử dụng script
./scripts/db-update.ps1  # Windows PowerShell
./scripts/db-update.sh   # Linux / macOS / Bash

# Hoặc dùng dotnet-ef trực tiếp
cd backend
dotnet dotnet-ef database update --project src/LocalLink.Infrastructure --startup-project src/LocalLink.API
```
