# LocalLink — Software Requirements Specification (SRS) V1.0

## 1. Giới thiệu

### 1.1 Tên hệ thống

**LocalLink — Cloud-native Regional Banking & Local Services Platform**

LocalLink là một hệ thống ngân hàng số và dịch vụ địa phương **giả lập**, được xây dựng cho mục đích học tập, portfolio và trình diễn kỹ năng phát triển phần mềm.

Toàn bộ:

- khách hàng
- tài khoản
- số dư
- giao dịch
- hóa đơn
- thanh toán

đều sử dụng **dữ liệu giả lập**.

Hệ thống không kết nối ngân hàng thật, payment gateway thật hoặc dữ liệu tài chính thật.

---

## 1.2 Mục tiêu

LocalLink mô phỏng một nền tảng cho phép khách hàng:

- đăng ký và đăng nhập
- quản lý hồ sơ cá nhân
- xem tài khoản ngân hàng
- xem số dư
- chuyển tiền giữa các tài khoản trong hệ thống
- xem lịch sử giao dịch
- thanh toán hóa đơn
- nhận thông báo

Nhân viên và quản trị viên có thể:

- xem khách hàng
- quản lý trạng thái khách hàng
- xem tài khoản
- xem giao dịch
- kiểm tra Audit Log

---

# 2. Phạm vi V1

V1 bao gồm các module:

```text
Identity
Customer
Bank Account
Transfer
Transaction
Audit Log
Bill
Payment
Notification
Administration
```

Không nằm trong V1:

```text
Real banking integration
Real payment gateway
Credit card processing
Loan
Investment
KYC provider integration
Flutter mobile application
Microservices
Kafka/RabbitMQ
AI recommendation
```

---

# 3. Actors

## 3.1 Customer

Người dùng cuối của hệ thống.

Có thể:

- đăng nhập
- xem profile
- xem tài khoản
- xem số dư
- chuyển tiền
- xem lịch sử giao dịch
- thanh toán hóa đơn
- xem thông báo

---

## 3.2 Staff

Nhân viên giả lập của hệ thống.

Có thể:

- xem danh sách khách hàng
- xem chi tiết khách hàng
- xem tài khoản
- xem giao dịch
- khóa/mở khóa khách hàng nếu được cấp quyền

---

## 3.3 Admin

Quản trị hệ thống.

Có toàn bộ quyền của Staff và có thể:

- quản lý trạng thái user
- quản lý role
- xem Audit Log
- xem toàn bộ giao dịch
- khóa/mở khóa account

---

# 4. Role

Hệ thống V1 có 3 role:

```text
CUSTOMER
STAFF
ADMIN
```

Không tạo thêm role nếu chưa có yêu cầu.

---

# 5. Functional Requirements

## FR-01 — User Authentication

Hệ thống phải cho phép user đăng nhập bằng:

```text
Email
Password
```

Sau khi đăng nhập thành công hệ thống trả về:

```text
Access Token
Refresh Token
User Information
Roles
```

### Business Rules

- Email phải unique.
- User bị khóa không được đăng nhập.
- Password không lưu plain text.
- Refresh Token phải có thời hạn.
- Refresh Token có thể revoke.

---

## FR-02 — Authorization

Hệ thống phải hỗ trợ Role-Based Access Control.

Role:

```text
CUSTOMER
STAFF
ADMIN
```

Ví dụ:

```text
CUSTOMER
→ chỉ truy cập dữ liệu thuộc chính mình

STAFF
→ xem Customer / Account / Transaction

ADMIN
→ có quyền quản trị
```

---

# 6. Customer Management

## FR-03 — Customer Profile

Customer có các thông tin:

```text
CustomerId
CustomerCode
FullName
DateOfBirth
Gender
PhoneNumber
Address
Status
CreatedAt
UpdatedAt
```

Customer có thể xem và cập nhật một số thông tin cá nhân.

---

## FR-04 — Customer Status

Customer status:

```text
ACTIVE
SUSPENDED
CLOSED
```

### Business Rules

Customer `SUSPENDED`:

- không được thực hiện transfer
- không được thực hiện payment

Customer `CLOSED`:

- không được thực hiện giao dịch mới

---

# 7. Bank Account

## FR-05 — Bank Account

Một Customer có thể sở hữu nhiều Bank Account.

Bank Account gồm:

```text
AccountId
AccountNumber
CustomerId
AccountName
AccountType
Balance
Currency
Status
CreatedAt
UpdatedAt
```

---

## FR-06 — Account Type

V1 sử dụng:

```text
CHECKING
SAVINGS
```

---

## FR-07 — Account Status

```text
ACTIVE
LOCKED
CLOSED
```

Chỉ account `ACTIVE` mới được thực hiện transfer/payment.

---

## FR-08 — Account Balance

Balance phải sử dụng kiểu decimal.

Ví dụ:

```text
decimal(18,2)
```

Không sử dụng:

```text
float
double
```

cho tiền.

Balance không được âm.

---

# 8. Transfer

## FR-09 — Transfer Money

Customer có thể chuyển tiền từ một account của mình đến một account khác trong LocalLink.

Input:

```text
SourceAccountId
DestinationAccountNumber
Amount
Description
```

---

## FR-10 — Transfer Validation

Transfer chỉ được thực hiện khi:

```text
Amount > 0
```

Source Account tồn tại.

Destination Account tồn tại.

Source Account thuộc Customer đang đăng nhập.

Source Account = ACTIVE.

Destination Account = ACTIVE.

Source và Destination không giống nhau.

Source Balance >= Amount.

Customer = ACTIVE.

---

## FR-11 — Atomic Transfer

Transfer phải được xử lý trong **một database transaction**.

Flow:

```text
Begin Transaction

↓
Validate Source Account

↓
Validate Destination Account

↓
Validate Balance

↓
Debit Source

↓
Credit Destination

↓
Create Transaction record

↓
Create Audit Log

↓
Commit
```

Nếu bất kỳ bước nào thất bại:

```text
Rollback
```

Không được xảy ra trường hợp:

```text
Source đã bị trừ tiền
nhưng
Destination chưa được cộng tiền
```

---

# 9. Transaction

## FR-12 — Transaction Record

Mỗi hoạt động tài chính phải sinh một Transaction.

Transaction gồm:

```text
TransactionId
ReferenceNumber
TransactionType
SourceAccountId
DestinationAccountId
Amount
Currency
Description
Status
CreatedAt
CompletedAt
```

---

## FR-13 — Transaction Type

V1:

```text
TRANSFER
PAYMENT
DEPOSIT
WITHDRAWAL
```

DEPOSIT/WITHDRAWAL có thể chỉ dùng cho seed/admin simulation trong V1.

---

## FR-14 — Transaction Status

```text
PENDING
COMPLETED
FAILED
CANCELLED
```

---

## FR-15 — Transaction Immutability

Sau khi transaction đã `COMPLETED`:

không được sửa:

```text
Amount
SourceAccount
DestinationAccount
ReferenceNumber
CreatedAt
```

Nếu cần correction phải tạo transaction mới.

Không UPDATE transaction tài chính lịch sử.

---

# 10. Transaction History

## FR-16

Customer có thể xem lịch sử giao dịch của các account thuộc mình.

Hỗ trợ:

```text
Pagination
Date range
Transaction type
Transaction status
Search reference number
```

Mặc định:

```text
newest first
```

---

# 11. Audit Log

## FR-17 — Audit Logging

Các thao tác quan trọng phải tạo Audit Log.

Ví dụ:

```text
LOGIN
LOGIN_FAILED
TRANSFER
PAYMENT
CUSTOMER_LOCK
CUSTOMER_UNLOCK
ACCOUNT_LOCK
ACCOUNT_UNLOCK
```

Audit Log gồm:

```text
AuditLogId
UserId
Action
EntityType
EntityId
Description
IpAddress
CreatedAt
```

---

## FR-18 — Audit Immutability

Audit Log:

```text
không được UPDATE
không được DELETE thông qua application API
```

---

# 12. Bill

## FR-19 — Bill

Customer có thể có các hóa đơn giả lập.

Bill gồm:

```text
BillId
CustomerId
ProviderName
BillType
BillNumber
Amount
DueDate
Status
CreatedAt
```

---

## FR-20 — Bill Type

V1:

```text
ELECTRICITY
WATER
INTERNET
EDUCATION
OTHER
```

---

## FR-21 — Bill Status

```text
UNPAID
PAID
OVERDUE
CANCELLED
```

---

# 13. Payment

## FR-22 — Pay Bill

Customer chọn:

```text
Bill
+
Bank Account
```

để thanh toán.

Validation:

```text
Bill = UNPAID

Account = ACTIVE

Account belongs to Customer

Balance >= Bill Amount
```

---

## FR-23 — Payment Success

Payment thành công phải:

```text
Debit Account

↓

Create Transaction(type = PAYMENT)

↓

Create Payment

↓

Bill.Status = PAID

↓

Create Audit Log

↓

Create Notification
```

Toàn bộ operation phải atomic.

---

# 14. Notification

## FR-24

Notification được tạo khi:

```text
Transfer success
Transfer failed
Payment success
Account locked
Important system event
```

Notification gồm:

```text
NotificationId
UserId
Title
Message
Type
IsRead
CreatedAt
ReadAt
```

---

# 15. Admin

## FR-25 — Customer Management

Admin/Staff có thể:

```text
List Customers
Search Customer
View Customer Detail
View Accounts
View Transactions
```

---

## FR-26 — Lock Customer

Admin có thể thay:

```text
ACTIVE → SUSPENDED
SUSPENDED → ACTIVE
```

Action phải được Audit Log.

---

## FR-27 — Lock Account

Admin có thể:

```text
ACTIVE → LOCKED
LOCKED → ACTIVE
```

Action phải được Audit Log.

---

# 16. Non-Functional Requirements

## NFR-01 Security

Password phải hash.

Không log:

```text
Password
JWT secret
Refresh token raw value
Database password
```

---

## NFR-02 Data Integrity

Các nghiệp vụ tài chính phải sử dụng transaction database.

Balance không được âm.

Foreign key phải được enforce tại SQL Server.

---

## NFR-03 Performance

List API phải sử dụng pagination.

Không load toàn bộ transaction history vào memory.

---

## NFR-04 API

REST API.

Route convention:

```text
/api/v1/...
```

Response/error phải nhất quán.

Sử dụng ProblemDetails cho error response.

---

## NFR-05 Database

Database:

```text
Microsoft SQL Server 2022
```

ORM:

```text
Entity Framework Core
```

---

## NFR-06 Deployment

Development environment sử dụng Docker Compose:

```text
Nuxt
ASP.NET Core
SQL Server
```

---

# 17. Database Design V1

## 17.1 Tables

Database V1 dự kiến có:

```text
Users
Roles
UserRoles
RefreshTokens

Customers

BankAccounts

Transactions

Bills
Payments

Notifications

AuditLogs
```

Technical table:

```text
SystemInfos
__EFMigrationsHistory
```

---

# 18. Entity Relationships

```text
User
 │
 ├── N:N ── Role
 │
 ├── 1:1 ── Customer
 │
 ├── 1:N ── RefreshToken
 │
 ├── 1:N ── Notification
 │
 └── 1:N ── AuditLog

Customer
 │
 ├── 1:N ── BankAccount
 │
 └── 1:N ── Bill

BankAccount
 │
 ├── 1:N ── Transaction (Source)
 │
 ├── 1:N ── Transaction (Destination)
 │
 └── 1:N ── Payment

Bill
 │
 └── 1:0..1 ── Payment

Payment
 │
 └── 1:1 ── Transaction
```

---

# 19. Database Tables

## Users

```text
Id                  uniqueidentifier PK
Email               nvarchar(255) NOT NULL UNIQUE
PasswordHash        nvarchar(500) NOT NULL
Status              varchar(30) NOT NULL
LastLoginAtUtc      datetime2 NULL
CreatedAtUtc        datetime2 NOT NULL
UpdatedAtUtc        datetime2 NULL
```

Index:

```text
UX_Users_Email
```

---

# 20. Roles

```text
Id                  uniqueidentifier PK
Name                varchar(50) NOT NULL UNIQUE
Description         nvarchar(255) NULL
CreatedAtUtc        datetime2 NOT NULL
```

Seed:

```text
CUSTOMER
STAFF
ADMIN
```

---

# 21. UserRoles

```text
UserId              uniqueidentifier FK
RoleId              uniqueidentifier FK
CreatedAtUtc        datetime2 NOT NULL
```

Composite PK:

```text
(UserId, RoleId)
```

---

# 22. RefreshTokens

```text
Id                  uniqueidentifier PK
UserId              uniqueidentifier FK
TokenHash           nvarchar(500) NOT NULL
ExpiresAtUtc        datetime2 NOT NULL
RevokedAtUtc        datetime2 NULL
CreatedAtUtc        datetime2 NOT NULL
```

Index:

```text
IX_RefreshTokens_UserId
IX_RefreshTokens_ExpiresAtUtc
```

Raw refresh token không lưu trực tiếp nếu thiết kế hash-token được triển khai.

---

# 23. Customers

```text
Id                  uniqueidentifier PK
UserId              uniqueidentifier FK UNIQUE
CustomerCode        varchar(30) NOT NULL UNIQUE
FullName            nvarchar(150) NOT NULL
DateOfBirth         date NULL
Gender              varchar(20) NULL
PhoneNumber         varchar(20) NULL
Address             nvarchar(500) NULL
Status              varchar(30) NOT NULL
CreatedAtUtc        datetime2 NOT NULL
UpdatedAtUtc        datetime2 NULL
```

Relationship:

```text
User 1 —— 1 Customer
```

---

# 24. BankAccounts

```text
Id                  uniqueidentifier PK
CustomerId          uniqueidentifier FK
AccountNumber       varchar(30) NOT NULL UNIQUE
AccountName         nvarchar(150) NOT NULL
AccountType         varchar(30) NOT NULL
Balance             decimal(18,2) NOT NULL
Currency            char(3) NOT NULL
Status              varchar(30) NOT NULL
CreatedAtUtc        datetime2 NOT NULL
UpdatedAtUtc        datetime2 NULL
```

Constraints:

```text
Balance >= 0
Currency = VND trong V1
```

Indexes:

```text
UX_BankAccounts_AccountNumber
IX_BankAccounts_CustomerId
```

---

# 25. Transactions

```text
Id                      uniqueidentifier PK
ReferenceNumber         varchar(50) NOT NULL UNIQUE

TransactionType         varchar(30) NOT NULL

SourceAccountId         uniqueidentifier NULL FK
DestinationAccountId    uniqueidentifier NULL FK

Amount                  decimal(18,2) NOT NULL
Currency                char(3) NOT NULL

Description             nvarchar(500) NULL

Status                  varchar(30) NOT NULL

CreatedAtUtc            datetime2 NOT NULL
CompletedAtUtc          datetime2 NULL
```

Constraints:

```text
Amount > 0
```

Indexes:

```text
UX_Transactions_ReferenceNumber

IX_Transactions_SourceAccountId_CreatedAtUtc

IX_Transactions_DestinationAccountId_CreatedAtUtc

IX_Transactions_CreatedAtUtc
```

Không Cascade Delete transaction khi account/customer bị xóa.

---

# 26. Bills

```text
Id                  uniqueidentifier PK
CustomerId          uniqueidentifier FK

ProviderName        nvarchar(150) NOT NULL
BillType            varchar(30) NOT NULL
BillNumber          varchar(50) NOT NULL

Amount              decimal(18,2) NOT NULL

DueDate             date NOT NULL

Status              varchar(30) NOT NULL

CreatedAtUtc        datetime2 NOT NULL
UpdatedAtUtc        datetime2 NULL
```

Indexes:

```text
IX_Bills_CustomerId_Status
UX_Bills_BillNumber
```

---

# 27. Payments

```text
Id                  uniqueidentifier PK

BillId              uniqueidentifier FK UNIQUE
AccountId           uniqueidentifier FK
TransactionId       uniqueidentifier FK UNIQUE

Amount              decimal(18,2) NOT NULL

Status              varchar(30) NOT NULL

PaidAtUtc           datetime2 NULL
CreatedAtUtc        datetime2 NOT NULL
```

---

# 28. Notifications

```text
Id                  uniqueidentifier PK

UserId              uniqueidentifier FK

Title               nvarchar(200) NOT NULL
Message             nvarchar(1000) NOT NULL

Type                varchar(50) NOT NULL

IsRead              bit NOT NULL

CreatedAtUtc        datetime2 NOT NULL
ReadAtUtc           datetime2 NULL
```

Index:

```text
IX_Notifications_UserId_IsRead_CreatedAtUtc
```

---

# 29. AuditLogs

```text
Id                  uniqueidentifier PK

UserId              uniqueidentifier NULL FK

Action              varchar(100) NOT NULL

EntityType          varchar(100) NULL
EntityId            varchar(100) NULL

Description         nvarchar(1000) NULL
IpAddress           varchar(64) NULL

CreatedAtUtc        datetime2 NOT NULL
```

Indexes:

```text
IX_AuditLogs_UserId
IX_AuditLogs_Action
IX_AuditLogs_CreatedAtUtc
```

Audit log không cascade delete khi User bị xóa.

---

# 30. Delete Strategy

Đối với dữ liệu tài chính:

Không physical delete:

```text
Customer
BankAccount
Transaction
Payment
AuditLog
```

Sử dụng status:

```text
ACTIVE
SUSPENDED
LOCKED
CLOSED
```

Transaction và Audit Log phải được giữ lại.

---

# 31. Concurrency

BankAccount phải hỗ trợ optimistic concurrency.

Có thể sử dụng:

```text
rowversion
```

thêm vào:

```text
BankAccounts.RowVersion
```

để tránh hai request đồng thời cập nhật Balance sai.

---

# 32. Seed Data

Development seed data:

### User 1

```text
Email:
customer1@locallink.local

Role:
CUSTOMER

Customer:
Nguyen Van An

CustomerCode:
CUS000001
```

Account:

```text
AccountNumber:
1000000001

Type:
CHECKING

Balance:
25,000,000 VND
```

---

### User 2

```text
Email:
customer2@locallink.local

Role:
CUSTOMER

Customer:
Tran Thi Binh

CustomerCode:
CUS000002
```

Account:

```text
AccountNumber:
1000000002

Balance:
15,000,000 VND
```

---

### Admin

```text
Email:
admin@locallink.local

Role:
ADMIN
```

Password seed chỉ dùng development.

Không ghi password seed production vào repository.

---

# 33. Database Milestones

Không tạo toàn bộ schema cùng lúc nếu chưa cần.

## M2A — Identity & Customer

```text
Users
Roles
UserRoles
Customers
```

## M2B — Bank Account

```text
BankAccounts
```

## M3

```text
RefreshTokens
```

được hoàn thiện cùng Authentication.

## M5 — Financial Core

```text
Transactions
AuditLogs
```

## M7

```text
Bills
Payments
Notifications
```

---

# 34. Database Design Principle

Ưu tiên:

```text
Data integrity
Transaction safety
Auditability
Simple architecture
Clear relationships
```

Không thiết kế database giống hệ thống ngân hàng production thật.

Đây là banking simulation portfolio project, nhưng phải thể hiện được engineering practices thực tế.

---

# 35. V1 Development Flow

```text
M1 Foundation ✅

↓

M2 Database Core
Users
Roles
Customer
BankAccount

↓

M3 Authentication
JWT
Refresh Token
RBAC

↓

M4 Customer / Account APIs

↓

M5
Transfer
Transaction
Audit

↓

M6
Frontend Banking UI

↓

M7
Bill
Payment
Notification

↓

Testing

↓

Azure / CI-CD
```