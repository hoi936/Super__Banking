# LocalLink Milestone 2 — Complete Database Foundation

Bạn đang làm việc trên project **LocalLink** đã hoàn thành Milestone 1.

Project hiện tại đã có:

```text
.NET 10
ASP.NET Core
Entity Framework Core 10
SQL Server 2022
Nuxt 4
Docker Compose
ApplicationDbContext
EF Core Migrations
SQL Server Docker Volume
```

Không tạo project mới.

Không thay đổi architecture hiện tại.

Không downgrade framework/package.

---

# 1. MỤC TIÊU

Thiết kế và triển khai database schema đủ khoảng **80–85% LocalLink V1** để các milestone sau chủ yếu phát triển business logic/API/UI thay vì phải thiết kế lại database.

Database là database của một:

```text
Simulated Regional Banking
&
Local Services Platform
```

Toàn bộ dữ liệu là fictional/demo data.

Không kết nối ngân hàng thật.

---

# 2. DATABASE ENGINE

Bắt buộc sử dụng:

```text
Microsoft SQL Server 2022
Entity Framework Core 10
```

SQL Server tiếp tục chạy bằng Docker.

Không chuyển sang:

```text
SQLite
PostgreSQL
MongoDB
JSON database
In-memory database
```

---

# 3. DOMAIN MODULES

Tổ chức database theo domain:

```text
Identity
Customer
Banking
Payments
Communication
Audit
```

Không tạo microservice.

Đây vẫn là Modular Monolith.

---

# 4. TABLES CẦN THIẾT

Tạo các entity/table:

```text
Users
Roles
UserRoles
RefreshTokens

Customers

BankAccounts
Beneficiaries

Transactions
Transfers

Bills
Payments

Notifications

AuditLogs
```

Giữ lại technical table hiện có:

```text
SystemInfos
__EFMigrationsHistory
```

---

# 5. USERS

Entity:

```text
User
```

Fields:

```text
Id                  Guid PK

Email               nvarchar(255)
PasswordHash        nvarchar(500)

Status              varchar(30)

LastLoginAtUtc      datetime2 nullable

CreatedAtUtc        datetime2
UpdatedAtUtc        datetime2 nullable
```

Constraints:

```text
Email NOT NULL
Email UNIQUE
```

User Status:

```text
ACTIVE
SUSPENDED
LOCKED
CLOSED
```

Không lưu password plain text.

---

# 6. ROLES

Entity:

```text
Role
```

Fields:

```text
Id                  Guid PK

Name                varchar(50)
Description         nvarchar(255) nullable

CreatedAtUtc        datetime2
```

Unique:

```text
Name
```

Seed roles:

```text
CUSTOMER
STAFF
ADMIN
```

---

# 7. USER ROLES

Entity:

```text
UserRole
```

Fields:

```text
UserId
RoleId
CreatedAtUtc
```

Composite Primary Key:

```text
(UserId, RoleId)
```

Relationships:

```text
User N:N Role
```

---

# 8. REFRESH TOKENS

Entity:

```text
RefreshToken
```

Fields:

```text
Id                  Guid PK
UserId              Guid FK

TokenHash           nvarchar(500)

ExpiresAtUtc        datetime2
CreatedAtUtc        datetime2
RevokedAtUtc        datetime2 nullable

CreatedByIp         varchar(64) nullable
RevokedByIp         varchar(64) nullable
```

Indexes:

```text
UserId
ExpiresAtUtc
```

Không lưu raw refresh token nếu authentication implementation sau này sử dụng hashed token.

---

# 9. CUSTOMERS

Entity:

```text
Customer
```

Fields:

```text
Id                  Guid PK
UserId              Guid FK UNIQUE

CustomerCode        varchar(30)

FullName            nvarchar(150)

DateOfBirth         date nullable
Gender              varchar(20) nullable

PhoneNumber         varchar(20) nullable
Address             nvarchar(500) nullable

Status              varchar(30)

CreatedAtUtc        datetime2
UpdatedAtUtc        datetime2 nullable
```

Unique:

```text
CustomerCode
UserId
```

Relationship:

```text
User 1:0..1 Customer
```

Customer status:

```text
ACTIVE
SUSPENDED
CLOSED
```

---

# 10. BANK ACCOUNTS

Entity:

```text
BankAccount
```

Fields:

```text
Id                  Guid PK
CustomerId          Guid FK

AccountNumber       varchar(30)
AccountName         nvarchar(150)

AccountType         varchar(30)

Balance             decimal(18,2)

Currency            char(3)

Status              varchar(30)

RowVersion          rowversion

CreatedAtUtc        datetime2
UpdatedAtUtc        datetime2 nullable
```

Unique:

```text
AccountNumber
```

Indexes:

```text
CustomerId
Status
```

Account types:

```text
CHECKING
SAVINGS
```

Account status:

```text
ACTIVE
LOCKED
CLOSED
```

Constraints:

```text
Balance >= 0
```

V1 currency:

```text
VND
```

Không dùng float/double cho tiền.

---

# 11. BENEFICIARIES

Entity:

```text
Beneficiary
```

Mục đích:

Customer lưu người nhận thường xuyên.

Fields:

```text
Id                      Guid PK

CustomerId              Guid FK

BeneficiaryAccountId    Guid FK

Nickname                nvarchar(100) nullable

CreatedAtUtc            datetime2
```

Unique composite:

```text
CustomerId
BeneficiaryAccountId
```

Không cho customer lưu chính account của mình nếu business layer kiểm tra được.

---

# 12. TRANSACTIONS

Entity:

```text
Transaction
```

Đây là financial ledger record cấp application.

Fields:

```text
Id                      Guid PK

ReferenceNumber         varchar(50)

TransactionType         varchar(30)

SourceAccountId         Guid nullable FK
DestinationAccountId    Guid nullable FK

Amount                  decimal(18,2)

Currency                char(3)

Description             nvarchar(500) nullable

Status                  varchar(30)

CreatedAtUtc            datetime2
CompletedAtUtc          datetime2 nullable
```

Transaction types:

```text
TRANSFER
PAYMENT
DEPOSIT
WITHDRAWAL
```

Statuses:

```text
PENDING
COMPLETED
FAILED
CANCELLED
```

Unique:

```text
ReferenceNumber
```

Indexes:

```text
SourceAccountId + CreatedAtUtc

DestinationAccountId + CreatedAtUtc

CreatedAtUtc

Status
```

Constraint:

```text
Amount > 0
```

Transaction data sau khi `COMPLETED` sẽ được xem là immutable ở application layer.

Không cascade delete từ BankAccount.

---

# 13. TRANSFERS

Entity:

```text
Transfer
```

Transaction là record tài chính chung.

Transfer chứa metadata riêng của nghiệp vụ chuyển tiền.

Fields:

```text
Id                      Guid PK

TransactionId           Guid FK UNIQUE

SourceAccountId         Guid FK
DestinationAccountId    Guid FK

Amount                  decimal(18,2)

Description             nvarchar(500) nullable

Status                  varchar(30)

CreatedAtUtc            datetime2
CompletedAtUtc          datetime2 nullable
```

Relationship:

```text
Transaction 1:0..1 Transfer
```

Một transfer thành công phải có Transaction tương ứng.

---

# 14. BILLS

Entity:

```text
Bill
```

Fields:

```text
Id                  Guid PK

CustomerId          Guid FK

ProviderName        nvarchar(150)

BillType            varchar(30)

BillNumber          varchar(50)

Amount              decimal(18,2)

DueDate             date

Status              varchar(30)

CreatedAtUtc        datetime2
UpdatedAtUtc        datetime2 nullable
```

Bill types:

```text
ELECTRICITY
WATER
INTERNET
EDUCATION
OTHER
```

Status:

```text
UNPAID
PAID
OVERDUE
CANCELLED
```

Unique:

```text
BillNumber
```

Index:

```text
CustomerId + Status
DueDate
```

Constraint:

```text
Amount > 0
```

---

# 15. PAYMENTS

Entity:

```text
Payment
```

Fields:

```text
Id                  Guid PK

BillId              Guid FK UNIQUE

AccountId           Guid FK

TransactionId       Guid FK UNIQUE

Amount              decimal(18,2)

Status              varchar(30)

PaidAtUtc           datetime2 nullable

CreatedAtUtc        datetime2
```

Payment Status:

```text
PENDING
COMPLETED
FAILED
```

Relationship:

```text
Bill 1:0..1 Payment

Payment 1:1 Transaction
```

Constraint:

```text
Amount > 0
```

---

# 16. NOTIFICATIONS

Entity:

```text
Notification
```

Fields:

```text
Id                  Guid PK

UserId              Guid FK

Title               nvarchar(200)

Message             nvarchar(1000)

Type                varchar(50)

IsRead              bit

CreatedAtUtc        datetime2
ReadAtUtc           datetime2 nullable
```

Indexes:

```text
UserId + IsRead + CreatedAtUtc
```

Notification types:

```text
TRANSFER
PAYMENT
ACCOUNT
SYSTEM
SECURITY
```

---

# 17. AUDIT LOGS

Entity:

```text
AuditLog
```

Fields:

```text
Id                  Guid PK

UserId              Guid nullable FK

Action              varchar(100)

EntityType          varchar(100) nullable

EntityId            varchar(100) nullable

Description         nvarchar(1000) nullable

IpAddress           varchar(64) nullable

CreatedAtUtc        datetime2
```

Indexes:

```text
UserId
Action
CreatedAtUtc
EntityType + EntityId
```

AuditLogs phải được thiết kế để:

```text
append-only
```

Application không có:

```text
Update AuditLog
Delete AuditLog
```

Không cascade delete khi User bị disable/closed.

---

# 18. DELETE STRATEGY

Không physical delete financial data.

Các entity:

```text
Customer
BankAccount
Transaction
Transfer
Bill
Payment
AuditLog
```

phải ưu tiên status/state thay vì delete.

Không cascade delete financial records.

Cascade chỉ dùng cho data kỹ thuật an toàn nếu thực sự phù hợp.

---

# 19. FOREIGN KEY DELETE BEHAVIOR

Định nghĩa DeleteBehavior rõ ràng.

Ưu tiên:

```text
Restrict
NoAction
```

cho financial relationships.

Không để EF Core tự tạo cascade chain nguy hiểm.

SQL Server không được gặp multiple cascade path errors.

---

# 20. ENUM STRATEGY

Domain sử dụng C# enum.

Persist database dưới dạng string nếu architecture hiện tại phù hợp.

Ví dụ:

```csharp
AccountStatus.Active
```

database:

```text
ACTIVE
```

Hoặc sử dụng consistent convention hiện có.

Không trộn ngẫu nhiên integer/string enum.

---

# 21. ENTITY CONFIGURATION

Không configure tất cả entity trong:

```text
OnModelCreating()
```

Tạo Fluent API configuration riêng:

```text
Persistence/
Configurations/

UserConfiguration.cs
RoleConfiguration.cs
UserRoleConfiguration.cs
RefreshTokenConfiguration.cs

CustomerConfiguration.cs

BankAccountConfiguration.cs
BeneficiaryConfiguration.cs

TransactionConfiguration.cs
TransferConfiguration.cs

BillConfiguration.cs
PaymentConfiguration.cs

NotificationConfiguration.cs
AuditLogConfiguration.cs
```

ApplicationDbContext sử dụng:

```csharp
ApplyConfigurationsFromAssembly(...)
```

nếu phù hợp architecture hiện tại.

---

# 22. DB CONTEXT

Update:

```text
ApplicationDbContext
```

DbSets:

```text
Users
Roles
UserRoles
RefreshTokens

Customers

BankAccounts
Beneficiaries

Transactions
Transfers

Bills
Payments

Notifications

AuditLogs
```

Giữ lại:

```text
SystemInfos
```

---

# 23. MIGRATION STRATEGY

Không xóa migration M1 hiện có.

Giữ:

```text
InitialCreate
```

Tạo migration mới:

```text
AddCoreBankingSchema
```

Command concept:

```bash
cd backend

dotnet dotnet-ef migrations add AddCoreBankingSchema \
  --project src/LocalLink.Infrastructure \
  --startup-project src/LocalLink.API \
  --output-dir Persistence/Migrations
```

Sau đó:

```bash
dotnet dotnet-ef database update \
  --project src/LocalLink.Infrastructure \
  --startup-project src/LocalLink.API
```

---

# 24. DEVELOPMENT SEED DATA

Tạo development data seeder.

Không đưa seed demo trực tiếp vào production behavior.

Seed:

## Roles

```text
CUSTOMER
STAFF
ADMIN
```

---

## Customer 1

```text
Email:
customer1@locallink.local

Full Name:
Nguyen Van An

Customer Code:
CUS000001

Status:
ACTIVE
```

Account:

```text
1000000001

CHECKING

25,000,000 VND

ACTIVE
```

---

## Customer 2

```text
Email:
customer2@locallink.local

Full Name:
Tran Thi Binh

Customer Code:
CUS000002

Status:
ACTIVE
```

Account:

```text
1000000002

CHECKING

15,000,000 VND

ACTIVE
```

---

## Admin

```text
admin@locallink.local

Role:
ADMIN
```

---

# 25. SEED PASSWORD

Authentication chưa implement ở milestone này.

Do đó không tự phát minh password hashing architecture.

Nếu User yêu cầu PasswordHash NOT NULL nhưng chưa có password hasher:

có thể dùng development placeholder hash rõ ràng được đánh dấu là temporary.

Không dùng plain password.

Không triển khai JWT/Auth trong milestone này.

---

# 26. IDEMPOTENT SEEDING

Seeder phải idempotent.

Chạy app nhiều lần không được tạo duplicate:

```text
Role
User
Customer
BankAccount
```

Check dữ liệu bằng unique identifiers như:

```text
Email
CustomerCode
AccountNumber
Role.Name
```

---

# 27. DATABASE DOCKER PORTABILITY

Mục tiêu cực kỳ quan trọng:

Một developer mới phải có thể:

```bash
git clone <repository>

cd LocalLink

cp .env.example .env

docker compose up --build -d
```

và hệ thống tự dựng:

```text
SQL Server container
↓
LocalLinkDb
↓
EF Core migrations
↓
Database schema
↓
Development seed data
↓
API
↓
Frontend
```

Không yêu cầu developer import `.bak` thủ công.

Không commit SQL Server database binary files.

---

# 28. AUTOMATIC MIGRATIONS

Project hiện đã có:

```text
APPLY_MIGRATIONS_AT_STARTUP
```

Kiểm tra implementation hiện tại.

Giữ cơ chế này chỉ cho:

```text
Development
Docker local environment
```

Không thiết kế production deployment phụ thuộc blind auto migration.

Khi:

```text
APPLY_MIGRATIONS_AT_STARTUP=true
```

API startup phải:

```text
wait SQL Server
↓
Database.Migrate()
↓
run development seeder
↓
start API
```

Xử lý transient connection failures phù hợp.

---

# 29. SQL SERVER DATA PERSISTENCE

Docker Compose phải tiếp tục dùng:

```text
locallink_sql_data
```

mapped:

```text
/var/opt/mssql
```

Không đổi volume name nếu không có lý do.

Verify:

```bash
docker compose down
docker compose up -d
```

Database vẫn tồn tại.

---

# 30. DATABASE RESET COMMAND

Document development reset:

```bash
docker compose down -v
docker compose up --build -d
```

Giải thích:

```text
docker compose down
```

→ giữ DB.

```text
docker compose down -v
```

→ xóa database development volume và dựng lại database từ migrations.

Đây chỉ dùng cho local development.

---

# 31. MIGRATION WORKFLOW CHO TEAM

Update README:

Khi developer thay đổi entity:

```text
1. Modify entity

2. Modify EntityTypeConfiguration

3. Create migration

4. Review generated migration

5. Run database update

6. Test Docker

7. Commit:
   entity
   configuration
   migration
```

Migration files PHẢI commit vào Git.

Không commit SQL Server volume.

---

# 32. TEAM DATABASE SYNCHRONIZATION

Developer A:

```bash
dotnet ef migrations add AddExample
git add .
git commit
git push
```

Developer B:

```bash
git pull
docker compose up --build -d
```

Auto migration phải nâng database của Developer B.

Không cần gửi `.bak`.

---

# 33. OPTIONAL DATABASE SCRIPT

Nếu hữu ích, tạo:

```text
scripts/
```

với:

```text
db-reset.ps1
db-reset.sh

db-update.ps1
db-update.sh
```

Ví dụ behavior:

```text
db-reset
→ docker compose down -v
→ docker compose up --build -d
```

Không duplicate logic quá nhiều.

---

# 34. DATABASE DOCUMENTATION

Tạo:

```text
docs/database.md
```

Bao gồm:

```text
Database overview
Entity list
Relationships
Constraints
Indexes
Delete strategy
Migration workflow
Development reset
Seed data
```

Thêm Mermaid ER Diagram.

---

# 35. ER DIAGRAM

Diagram phải thể hiện:

```text
Users
Roles
UserRoles
RefreshTokens
Customers
BankAccounts
Beneficiaries
Transactions
Transfers
Bills
Payments
Notifications
AuditLogs
```

Không cần thể hiện EF migration technical tables.

---

# 36. BUILD VERIFICATION

Sau khi code:

```bash
dotnet restore
dotnet build
```

Phải:

```text
0 errors
```

---

# 37. MIGRATION VERIFICATION

Apply migration.

Verify SQL Server có các table:

```text
Users
Roles
UserRoles
RefreshTokens
Customers
BankAccounts
Beneficiaries
Transactions
Transfers
Bills
Payments
Notifications
AuditLogs
SystemInfos
__EFMigrationsHistory
```

---

# 38. SEED VERIFICATION

Query database hoặc dùng EF để xác minh:

```text
3 Roles

2 Customer demo users

2 Customers

2 BankAccounts

1 Admin
```

Không tạo duplicate sau restart.

---

# 39. DOCKER CLEAN TEST

Thực hiện clean database test:

```bash
docker compose down -v

docker compose up --build -d
```

Sau đó verify:

```text
SQL Server Healthy

Migration applied

Schema created

Seed applied

API Healthy

Frontend Running
```

---

# 40. PERSISTENCE TEST

Sau clean test:

```bash
docker compose down

docker compose up -d
```

Verify database và data vẫn tồn tại.

---

# 41. KHÔNG IMPLEMENT BUSINESS FEATURES

Milestone này KHÔNG implement:

```text
Login endpoint
JWT issuing
Refresh endpoint
Transfer API
Payment API
Admin API
Dashboard banking
```

Chỉ database/domain foundation.

---

# 42. KHÔNG OVERENGINEER

Không thêm:

```text
CQRS
MediatR
Unit of Work abstraction
Generic repository
Event bus
Kafka
RabbitMQ
Redis
Microservices
```

chỉ để chuẩn bị trước.

---

# 43. GIT

Thực hiện trên feature branch:

```text
feature/database-core
```

Nếu đang ở `dev`:

```bash
git checkout dev
git pull
git checkout -b feature/database-core
```

Không commit trực tiếp lên:

```text
main
```

Sau khi hoàn tất:

```bash
git status
```

Đảm bảo:

```text
.env
```

không được commit.

Commit đề xuất:

```bash
git add .
git commit -m "feat: add core banking database schema"
```

Push:

```bash
git push -u origin feature/database-core
```

Không tự merge vào main.

---

# 44. DEFINITION OF DONE

Chỉ hoàn thành khi:

```text
[ ] Các entity được tạo

[ ] Fluent configurations hoàn chỉnh

[ ] Relationships đúng

[ ] Foreign keys đúng

[ ] Unique constraints đúng

[ ] Financial decimal fields đúng

[ ] RowVersion BankAccount hoạt động

[ ] DeleteBehavior an toàn

[ ] Migration AddCoreBankingSchema được tạo

[ ] Migration apply thành công

[ ] Seeder hoạt động

[ ] Seeder idempotent

[ ] Docker clean build dựng DB tự động

[ ] Clone workflow được document

[ ] Docker volume persistence hoạt động

[ ] Backend build không lỗi

[ ] API health vẫn Healthy

[ ] Database documentation được tạo

[ ] ER diagram được tạo

[ ] feature/database-core được push

[ ] .env không được commit
```

---

# 45. FINAL REPORT

Sau khi hoàn thành trả về:

```text
LOCALINK MILESTONE 2 DATABASE REPORT

Branch:

Migration:

Entities Created:

Tables Created:

Relationships:

Indexes:

Constraints:

Seed Data:

Database Reset Command:

Database Update Command:

Docker Clean Build:

Persistence Test:

Backend Build:

Health Check:

Git Commit:

Git Push:

Warnings:
```

Sau đó in database table list.

In ER diagram.

Cuối cùng DỪNG.

Không bắt đầu Authentication.