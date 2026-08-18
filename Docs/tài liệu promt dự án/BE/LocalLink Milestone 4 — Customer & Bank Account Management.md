# LocalLink Milestone 4 — Customer & Bank Account Management

Bạn đang làm việc trên project **LocalLink / InterLink Banking** đã hoàn thành:

```text
Milestone 1
Foundation & DevOps ✅

Milestone 2
Database Foundation ✅

Milestone 3
Authentication + JWT + Refresh Token + RBAC ✅
```

Tech stack hiện tại:

```text
.NET 10
ASP.NET Core
Entity Framework Core 10
SQL Server 2022
Nuxt 4
Vue 3
TypeScript
Quasar
Docker Compose
```

Database hiện đã có các bảng:

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

Authentication hiện đã có:

```text
POST /api/v1/auth/login
POST /api/v1/auth/refresh
POST /api/v1/auth/logout
GET  /api/v1/auth/me
```

và RBAC:

```text
CUSTOMER
STAFF
ADMIN
```

Không tạo lại foundation.

Không thay đổi auth architecture trừ khi cần bugfix nhỏ.

---

# 1. MỤC TIÊU M4

Triển khai Customer & Bank Account Management foundation.

M4 phải cho phép:

```text
CUSTOMER
→ xem hồ sơ chính mình
→ cập nhật một số thông tin profile
→ xem các tài khoản của chính mình
→ xem chi tiết account
→ xem balance
→ quản lý beneficiaries

STAFF
→ xem danh sách customer
→ search customer
→ xem customer detail
→ xem accounts của customer

ADMIN
→ có toàn bộ quyền STAFF
→ khóa/mở khóa customer
→ khóa/mở khóa account
```

Không implement Transfer trong milestone này.

---

# 2. FEATURE BRANCH

Bắt đầu từ:

```text
dev
```

Chạy:

```bash
git checkout dev
git pull origin dev
git checkout -b feature/customer-account
```

Không làm trực tiếp trên `main`.

---

# 3. CUSTOMER SELF PROFILE

Tạo endpoint:

```http
GET /api/v1/customers/me
```

Require:

```text
[Authorize(Roles = "CUSTOMER")]
```

Flow:

```text
JWT UserId
↓
Find Customer by UserId
↓
Return Customer Profile DTO
```

Response ví dụ:

```json
{
  "id": "...",
  "customerCode": "CUS000001",
  "fullName": "Nguyen Van An",
  "dateOfBirth": "1999-01-01",
  "gender": "MALE",
  "phoneNumber": "0900000001",
  "address": "Da Nang",
  "status": "ACTIVE"
}
```

Không expose:

```text
User.PasswordHash
RefreshTokens
internal security fields
```

---

# 4. UPDATE CUSTOMER PROFILE

Tạo:

```http
PUT /api/v1/customers/me
```

hoặc:

```http
PATCH /api/v1/customers/me
```

nếu architecture hiện tại phù hợp PATCH hơn.

Customer chỉ được phép cập nhật:

```text
FullName
DateOfBirth
Gender
PhoneNumber
Address
```

Không được tự sửa:

```text
CustomerCode
Status
UserId
CreatedAtUtc
```

---

# 5. CUSTOMER OWNERSHIP

Không nhận:

```text
CustomerId
```

từ client để xác định ownership trong customer self-service APIs.

Phải lấy:

```text
UserId
```

từ JWT/current user service.

Flow:

```text
JWT
↓
UserId
↓
Customer.UserId
↓
Customer.Id
```

---

# 6. GET CUSTOMER ACCOUNTS

Tạo:

```http
GET /api/v1/accounts
```

Require:

```text
CUSTOMER
```

Endpoint chỉ trả accounts thuộc customer đang đăng nhập.

Không cho client truyền customerId để đổi phạm vi dữ liệu.

Response:

```json
[
  {
    "id": "...",
    "accountNumber": "1000000001",
    "accountName": "Nguyen Van An",
    "accountType": "CHECKING",
    "balance": 25000000,
    "currency": "VND",
    "status": "ACTIVE"
  }
]
```

---

# 7. ACCOUNT DETAIL

Tạo:

```http
GET /api/v1/accounts/{id}
```

Require:

```text
CUSTOMER
```

Flow:

```text
Get current Customer
↓
Find Account
↓
Verify Account.CustomerId == CurrentCustomer.Id
↓
Return account
```

Nếu account tồn tại nhưng thuộc customer khác:

Không leak dữ liệu.

Return phù hợp:

```text
404
```

hoặc convention security hiện tại.

Không trả 200.

---

# 8. ACCOUNT NUMBER LOOKUP

Nếu cần cho milestone Transfer sau này, có thể chuẩn bị endpoint read-only:

```http
GET /api/v1/accounts/lookup/{accountNumber}
```

Nhưng response chỉ nên trả thông tin tối thiểu:

```json
{
  "accountNumber": "1000000002",
  "accountName": "Tran Thi Binh"
}
```

Không trả:

```text
Balance
CustomerId
UserId
Account internal id
```

Endpoint này có thể require authenticated CUSTOMER.

Nếu cảm thấy chưa cần, không bắt buộc implement ở M4.

---

# 9. BALANCE SECURITY

Cực kỳ quan trọng:

M4 không được tạo endpoint:

```text
PUT /accounts/{id}/balance
PATCH /accounts/{id}/balance
```

Không cho client update:

```text
Balance
```

Balance chỉ được thay đổi bởi:

```text
Transfer
Payment
Deposit simulation
Withdrawal simulation
```

ở milestone tài chính sau.

---

# 10. BENEFICIARIES

Tạo:

```http
GET /api/v1/beneficiaries
POST /api/v1/beneficiaries
DELETE /api/v1/beneficiaries/{id}
```

Require:

```text
CUSTOMER
```

---

# 11. GET BENEFICIARIES

Chỉ trả beneficiaries của customer đang đăng nhập.

Response:

```json
[
  {
    "id": "...",
    "accountNumber": "1000000002",
    "accountName": "Tran Thi Binh",
    "nickname": "Binh"
  }
]
```

---

# 12. ADD BENEFICIARY

Request:

```json
{
  "accountNumber": "1000000002",
  "nickname": "Binh"
}
```

Flow:

```text
Current Customer
↓
Find target BankAccount by AccountNumber
↓
Validate account exists
↓
Validate account status
↓
Check not own account
↓
Check duplicate
↓
Create Beneficiary
```

Rules:

```text
Target account must exist
Target account should not be CLOSED
Cannot add own account
Cannot add duplicate beneficiary
```

Do not require client to provide:

```text
BeneficiaryAccountId
CustomerId
```

Resolve these server-side.

---

# 13. DELETE BENEFICIARY

Endpoint:

```http
DELETE /api/v1/beneficiaries/{id}
```

Customer chỉ được xóa beneficiary thuộc chính mình.

Không physical delete financial data rule không áp dụng bắt buộc cho beneficiary vì đây chỉ là user preference/directory.

Có thể physical delete beneficiary.

---

# 14. STAFF / ADMIN CUSTOMER LIST

Tạo:

```http
GET /api/v1/admin/customers
```

Require:

```text
STAFF or ADMIN
```

Hỗ trợ:

```text
page
pageSize
search
status
```

Ví dụ:

```http
GET /api/v1/admin/customers?page=1&pageSize=20&search=nguyen&status=ACTIVE
```

---

# 15. PAGINATION RESPONSE

Chuẩn hóa response:

```json
{
  "items": [],
  "page": 1,
  "pageSize": 20,
  "totalItems": 100,
  "totalPages": 5
}
```

Không load toàn bộ Customers vào memory.

Pagination phải thực hiện ở SQL query level.

---

# 16. CUSTOMER SEARCH

Search có thể hỗ trợ:

```text
CustomerCode
FullName
Email
PhoneNumber
```

Dùng query có thể translate xuống SQL.

Không gọi:

```text
ToList()
```

trước khi filter/paginate.

---

# 17. ADMIN CUSTOMER DETAIL

Tạo:

```http
GET /api/v1/admin/customers/{id}
```

Require:

```text
STAFF or ADMIN
```

Response có:

```text
Customer Profile
User Status
Roles
Accounts Summary
```

Không trả:

```text
PasswordHash
RefreshTokens
security secrets
```

---

# 18. CUSTOMER ACCOUNTS FOR STAFF/ADMIN

Tạo:

```http
GET /api/v1/admin/customers/{id}/accounts
```

Require:

```text
STAFF or ADMIN
```

Read-only.

---

# 19. UPDATE CUSTOMER STATUS

Tạo:

```http
PATCH /api/v1/admin/customers/{id}/status
```

Require:

```text
ADMIN
```

Request:

```json
{
  "status": "SUSPENDED"
}
```

Allowed transitions:

```text
ACTIVE → SUSPENDED
SUSPENDED → ACTIVE
```

CLOSED không nên cho phép tùy tiện reopen nếu chưa có business rule rõ ràng.

Nếu status khác không hợp lệ:

```text
400
```

---

# 20. ACCOUNT STATUS MANAGEMENT

Tạo:

```http
PATCH /api/v1/admin/accounts/{id}/status
```

Require:

```text
ADMIN
```

Request:

```json
{
  "status": "LOCKED"
}
```

Allowed:

```text
ACTIVE → LOCKED
LOCKED → ACTIVE
```

Không cho update:

```text
Balance
AccountNumber
CustomerId
```

qua endpoint status.

---

# 21. AUDIT LOG

Các hành động Admin phải tạo AuditLog:

```text
CUSTOMER_SUSPEND
CUSTOMER_ACTIVATE
ACCOUNT_LOCK
ACCOUNT_UNLOCK
```

Audit phải ghi:

```text
Admin UserId
Action
EntityType
EntityId
Description
IpAddress
CreatedAtUtc
```

Không log secret.

---

# 22. APPLICATION STRUCTURE

Tổ chức theo module rõ ràng.

Ví dụ:

```text
LocalLink.Application/
├── Customers/
│   ├── DTOs/
│   ├── Interfaces/
│   └── Services/
│
├── Accounts/
│   ├── DTOs/
│   ├── Interfaces/
│   └── Services/
│
└── Beneficiaries/
    ├── DTOs/
    ├── Interfaces/
    └── Services/
```

Hoặc convention hiện tại tương đương.

Không dùng CQRS/MediatR chỉ vì milestone này.

---

# 23. DTOs

Không expose EF Entities trực tiếp từ controller.

Tạo DTO phù hợp:

```text
CustomerProfileDto
UpdateCustomerProfileRequest

AccountSummaryDto
AccountDetailDto

BeneficiaryDto
CreateBeneficiaryRequest

AdminCustomerListItemDto
AdminCustomerDetailDto

UpdateCustomerStatusRequest
UpdateAccountStatusRequest

PagedResult<T>
```

---

# 24. SERVICE LAYER

Có thể tạo:

```text
ICustomerService
IAccountService
IBeneficiaryService
IAdminCustomerService
```

chỉ nếu thực sự được sử dụng.

Không tạo GenericRepository.

Không tạo UnitOfWork abstraction vô nghĩa.

ApplicationDbContext/Infrastructure hiện tại đã đủ.

---

# 25. QUERY OPTIMIZATION

Read-only queries nên dùng:

```text
AsNoTracking()
```

khi phù hợp.

Use:

```text
Select(...)
```

projection trực tiếp ra DTO nếu hợp lý.

Không eager-load cả object graph không cần thiết.

---

# 26. CURRENT USER

Reuse CurrentUserService từ M3.

Không parse JWT bằng tay trong từng controller.

Controller không tự đọc string claim lặp lại nếu đã có abstraction.

---

# 27. SECURITY RULES

CUSTOMER không được:

```text
xem customer khác
xem account khác
xem balance khác
quản lý status
đọc admin endpoints
```

STAFF không được:

```text
đổi customer status
đổi account status
```

nếu business rule quy định chỉ ADMIN được phép.

ADMIN có quyền status management.

---

# 28. DATABASE CHANGES

Ưu tiên không thay schema M2 nếu schema hiện tại đủ.

Chỉ tạo migration nếu thực sự cần:

```text
index
constraint
nullable adjustment
```

Không tạo migration rỗng.

Không sửa migration cũ.

---

# 29. INDEX REVIEW

Kiểm tra index hiện tại.

Đảm bảo query chính có index hợp lý:

```text
Customers.UserId
Customers.CustomerCode
BankAccounts.CustomerId
BankAccounts.AccountNumber
Beneficiaries.CustomerId
Beneficiaries(CustomerId, BeneficiaryAccountId)
```

Nếu thiếu mới tạo migration:

```text
ImproveCustomerAccountIndexes
```

---

# 30. VALIDATION

Validate:

```text
FullName length
PhoneNumber length
Address length
Gender enum/value
PageSize
AccountNumber
Nickname length
Status enum
```

Không cần cài framework validation mới nếu chưa có.

Có thể dùng DataAnnotations hoặc application validation hiện tại.

---

# 31. PHONE NUMBER

Không cần xây international phone library.

Chỉ validate cơ bản:

```text
non-empty nếu provided
reasonable max length
```

Không over-engineer.

---

# 32. PROBLEM DETAILS

Tiếp tục dùng global ProblemDetails.

Expected:

```text
400 validation error
401 unauthenticated
403 forbidden
404 resource not found / hidden ownership
409 duplicate beneficiary nếu phù hợp
```

---

# 33. SWAGGER

Swagger phải hiển thị các groups/endpoints mới.

Bearer authorization tiếp tục hoạt động.

Có thể group:

```text
Customers
Accounts
Beneficiaries
Admin
```

---

# 34. AUTOMATED TESTS

Mở rộng test suite hiện tại.

Tối thiểu test:

```text
Customer can get own profile

Customer can update allowed fields

Customer cannot change status

Customer gets only own accounts

Customer cannot access another customer's account

Customer can get own account detail

Customer can add beneficiary

Customer cannot add own account as beneficiary

Customer cannot add duplicate beneficiary

Customer cannot delete another customer's beneficiary

STAFF can list customers

CUSTOMER cannot access admin customer list

STAFF can view customer detail

STAFF cannot change customer status

ADMIN can suspend customer

ADMIN can reactivate customer

ADMIN can lock account

ADMIN can unlock account
```

---

# 35. INTEGRATION TESTS

Nếu test infrastructure hiện tại cho phép, thêm integration tests cho:

```text
GET /api/v1/customers/me

GET /api/v1/accounts

GET /api/v1/accounts/{id}

POST /api/v1/beneficiaries

GET /api/v1/admin/customers
```

Không bắt buộc xây test framework khổng lồ nếu hiện tại chỉ có UnitTests.

---

# 36. DOCKER VERIFICATION

Sau implementation chạy:

```bash
dotnet test backend/LocalLink.sln
```

Expected:

```text
0 failed
```

Sau đó:

```bash
docker compose down
docker compose up --build -d
```

Không cần `-v` nếu không thay schema lớn.

Nếu có migration mới cần clean verification:

```bash
docker compose down -v
docker compose up --build -d
```

---

# 37. LIVE API VERIFICATION

Login customer:

```http
POST /api/v1/auth/login
```

Sau đó test:

```http
GET /api/v1/customers/me

GET /api/v1/accounts

GET /api/v1/accounts/{id}

GET /api/v1/beneficiaries
```

Expected:

```text
200
```

Test account của customer khác:

```text
not accessible
```

---

# 38. ADMIN VERIFICATION

Login admin.

Test:

```http
GET /api/v1/admin/customers

GET /api/v1/admin/customers/{id}

PATCH /api/v1/admin/customers/{id}/status

PATCH /api/v1/admin/accounts/{id}/status
```

Verify AuditLogs được tạo.

---

# 39. FRONTEND

Không xây full banking UI trong M4.

Chỉ update roadmap/status nếu cần:

```text
M1 COMPLETED
M2 COMPLETED
M3 COMPLETED
M4 IN PROGRESS / COMPLETED
```

Nếu frontend hiện đang ghi:

```text
Nuxt 3
```

trong khi project dùng Nuxt 4, sửa text thành đúng version.

Nếu branding LocalLink/InterLink Banking chưa thống nhất, không tự ý đổi toàn bộ tên project; chỉ ghi lại warning trong report.

---

# 40. README

Update API documentation:

```text
Customer APIs
Account APIs
Beneficiary APIs
Admin Customer APIs
Authorization Matrix
```

Thêm bảng quyền:

```text
Endpoint                         CUSTOMER   STAFF   ADMIN

/customers/me                    YES        NO      NO
/accounts                        YES        NO      NO
/beneficiaries                   YES        NO      NO

/admin/customers                 NO         YES     YES
/admin/customers/{id}            NO         YES     YES
/admin/customer status           NO         NO      YES
/admin/account status            NO         NO      YES
```

---

# 41. NO TRANSFER YET

Không implement:

```text
POST /transfers
transaction debit/credit
balance mutation
payment
bill payment
notification financial events
```

Đây là M5.

---

# 42. GIT

Sau khi hoàn tất:

```bash
git status
```

Ensure:

```text
.env
```

không tracked.

Commit:

```bash
git add .
git commit -m "feat: implement customer and account management"
```

Push:

```bash
git push -u origin feature/customer-account
```

Không merge trực tiếp vào main.

---

# 43. DEFINITION OF DONE

M4 chỉ hoàn thành khi:

```text
[ ] Customer self profile hoạt động

[ ] Customer profile update hoạt động

[ ] Customer cannot change protected fields

[ ] Account list chỉ trả account của current customer

[ ] Account detail ownership được enforce

[ ] Balance read-only

[ ] Beneficiary list hoạt động

[ ] Add beneficiary hoạt động

[ ] Own-account beneficiary bị chặn

[ ] Duplicate beneficiary bị chặn

[ ] Delete beneficiary ownership được enforce

[ ] STAFF customer list hoạt động

[ ] Pagination hoạt động ở SQL level

[ ] Search customer hoạt động

[ ] STAFF customer detail hoạt động

[ ] ADMIN customer suspend/reactivate hoạt động

[ ] ADMIN account lock/unlock hoạt động

[ ] Admin actions tạo AuditLog

[ ] RBAC đúng

[ ] Swagger hoạt động

[ ] Existing Auth không bị regression

[ ] Tests pass

[ ] Docker stack Healthy

[ ] feature/customer-account được push
```

---

# 44. FINAL REPORT

Sau khi hoàn thành trả:

```text
LOCALINK MILESTONE 4 REPORT

Branch:

Endpoints Created:

Customer Self-Service:

Account APIs:

Beneficiary APIs:

Staff APIs:

Admin APIs:

Ownership Strategy:

RBAC Matrix:

Validation:

Audit Events:

Database Changes:

Migration:

Indexes:

Tests Added:

Test Result:

Docker Result:

Live API Verification:

Swagger:

Commit:

Push:

Warnings:
```

Sau đó DỪNG.

Không bắt đầu Transfer Engine.