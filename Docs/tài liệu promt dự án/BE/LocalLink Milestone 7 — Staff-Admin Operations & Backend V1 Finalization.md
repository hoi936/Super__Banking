# LocalLink Milestone 7 — Staff/Admin Operations & Backend V1 Finalization

## 0. CURRENT PROJECT STATE

Project đã hoàn thành và verify:

```text
M1 Foundation & DevOps                    ✅
M2 Database Foundation                    ✅
M3 Authentication + JWT + RBAC            ✅
M4 Customer & Bank Account Management     ✅
M5 Transfer Engine + Transactions         ✅
M6 Bill Payment + Notifications           ✅
```

Current backend capabilities:

```text
Authentication
JWT + Refresh Token
RBAC: CUSTOMER / STAFF / ADMIN

Customer Profile
Bank Accounts
Beneficiaries

Transfers
Transaction History
Atomic Financial Transactions
Idempotency
RowVersion Optimistic Concurrency

Bills
Payments
Notifications

Audit Logs
SQL Server
EF Core
Docker
Swagger
61 automated tests
```

Tech stack:

```text
.NET 10
ASP.NET Core
EF Core 10
SQL Server 2022
Docker Compose

Nuxt 4
Vue 3
TypeScript
Quasar
```

Milestone này là:

```text
M7 = Staff/Admin Operations
   + Reporting APIs
   + Audit Management
   + User Administration
   + Security Hardening
   + Integration/Regression Testing
   + Backend V1 Finalization
```

Không redesign architecture hiện tại.

Không implement frontend Admin UI.

Không implement nghiệp vụ tài chính mới.

---

# 1. FEATURE BRANCH

Theo workflow hiện tại:

```bash
git checkout feature/hoi
git pull origin feature/hoi
git checkout -b feature/admin-backend
```

Sau khi hoàn thành và verify mới merge theo workflow repository hiện tại.

Không làm trực tiếp trên `main`.

---

# 2. M7 OBJECTIVE

Sau M7 backend phải đủ API cho:

```text
CUSTOMER
   └── Existing customer banking APIs

STAFF
   ├── Dashboard
   ├── Customer lookup
   ├── Account lookup
   ├── Transaction lookup
   └── Payment lookup

ADMIN
   ├── Everything STAFF can read
   ├── User management
   ├── Customer/account status management
   ├── Audit log
   ├── Dashboard
   └── System monitoring
```

M7 phải tập trung vào:

```text
Operations
Observability
Security
Consistency
Testing
Documentation
```

---

# 3. IMPORTANT RULE — DO NOT DUPLICATE M4

M4 đã có:

```text
GET /api/v1/admin/customers
GET /api/v1/admin/customers/{id}
GET /api/v1/admin/customers/{id}/accounts

PATCH /api/v1/admin/customers/{id}/status
PATCH /api/v1/admin/accounts/{id}/status
```

KHÔNG tạo lại.

Review và reuse chúng.

Chỉ refactor nếu có bug/security issue.

Existing tests phải tiếp tục pass.

---

# 4. ADMIN DASHBOARD

Implement:

```http
GET /api/v1/admin/dashboard
```

Authorize:

```text
STAFF
ADMIN
```

Response conceptually:

```json
{
  "customers": {
    "total": 120,
    "active": 110,
    "suspended": 10
  },
  "accounts": {
    "total": 168,
    "active": 160,
    "locked": 8,
    "totalBalance": 2400000000
  },
  "today": {
    "transfers": 315,
    "transferVolume": 185000000,
    "payments": 87,
    "paymentVolume": 42000000
  },
  "notifications": {
    "createdToday": 250
  },
  "generatedAtUtc": "..."
}
```

Adapt field names to existing entities/enums.

Do not expose sensitive data.

---

# 5. DASHBOARD QUERY RULES

Dashboard aggregation must happen primarily in SQL.

Do NOT:

```text
ToList entire Customers
ToList entire Accounts
ToList entire Transactions
```

then calculate in memory.

Prefer:

```text
CountAsync
SumAsync
GroupBy
Select
AsNoTracking
```

Use separate efficient queries if that produces clearer SQL.

---

# 6. FINANCIAL STATISTICS

Statistics must distinguish:

```text
TRANSFER
PAYMENT
```

Do not count failed/cancelled financial operations as completed volume.

For transfer volume:

```text
only COMPLETED transfers
```

For payment volume:

```text
only COMPLETED payments
```

Use UTC date boundaries consistently.

---

# 7. ADMIN TRANSACTION SEARCH

Implement:

```http
GET /api/v1/admin/transactions
```

Authorize:

```text
STAFF
ADMIN
```

Support:

```text
page
pageSize
search
type
status
fromDate
toDate
accountNumber
```

Search can include appropriate fields such as:

```text
ReferenceNumber
Source Account Number
Destination Account Number
```

depending on existing schema.

SQL-level filtering and pagination required.

---

# 8. ADMIN TRANSACTION DETAIL

Implement:

```http
GET /api/v1/admin/transactions/{id}
```

Authorize:

```text
STAFF
ADMIN
```

Response may contain:

```text
Reference
Type
Status
Amount
Currency
Source account
Destination account
Customer summary
Description
CreatedAtUtc
```

Do NOT expose:

```text
PasswordHash
RefreshTokens
JWT
security secrets
```

---

# 9. ADMIN TRANSFER SEARCH

If useful and not redundant with transaction search, implement:

```http
GET /api/v1/admin/transfers
GET /api/v1/admin/transfers/{id}
```

Authorize STAFF / ADMIN.

Filters:

```text
page
pageSize
status
reference
sourceAccount
destinationAccount
fromDate
toDate
```

If transaction endpoints already provide sufficient operational visibility, avoid unnecessary duplicate APIs.

Document the decision.

---

# 10. ADMIN PAYMENT SEARCH

Implement:

```http
GET /api/v1/admin/payments
```

Authorize:

```text
STAFF
ADMIN
```

Support:

```text
page
pageSize
status
billType
reference
accountNumber
fromDate
toDate
```

SQL-level pagination.

---

# 11. ADMIN PAYMENT DETAIL

Implement:

```http
GET /api/v1/admin/payments/{id}
```

Authorize:

```text
STAFF
ADMIN
```

Return operational information:

```text
Payment reference
Customer summary
Account
Bill
Provider
Amount
Status
PaidAtUtc
Transaction reference
```

No secrets.

---

# 12. AUDIT LOG API

Implement:

```http
GET /api/v1/admin/audit-logs
```

Authorize:

```text
ADMIN ONLY
```

STAFF must NOT access audit logs unless existing business rules explicitly require it.

---

# 13. AUDIT LOG FILTERS

Support:

```text
page
pageSize
action
entityType
userId
fromDate
toDate
search
```

Newest first.

SQL-level pagination.

---

# 14. AUDIT DETAIL

Implement:

```http
GET /api/v1/admin/audit-logs/{id}
```

ADMIN only.

Return:

```text
Id
UserId
Action
EntityType
EntityId
Description
IpAddress
CreatedAtUtc
```

Never include secrets.

---

# 15. AUDIT LOG IMMUTABILITY

Do NOT create:

```text
PUT /audit-logs
PATCH /audit-logs
DELETE /audit-logs
```

Audit logs are read-only through public/admin API.

No client can edit or delete audit history.

---

# 16. USER MANAGEMENT

Implement:

```http
GET /api/v1/admin/users
GET /api/v1/admin/users/{id}
```

ADMIN only.

Support user list filters:

```text
page
pageSize
search
status
role
```

Search:

```text
Email
CustomerCode
FullName
```

where relationships allow.

---

# 17. USER DTO SECURITY

Admin user DTO can contain:

```text
Id
Email
Status
Roles
CustomerCode
FullName
CreatedAtUtc
LastLoginAtUtc (if existing)
```

Never return:

```text
PasswordHash
RefreshToken hashes
security stamps/secrets
```

---

# 18. USER STATUS MANAGEMENT

Review existing UserStatus enum.

If compatible, implement:

```http
PATCH /api/v1/admin/users/{id}/status
```

ADMIN only.

Allowed operations should follow existing statuses.

Example:

```text
ACTIVE ↔ SUSPENDED
```

Do not invent unsupported statuses unnecessarily.

---

# 19. SECURITY EFFECT OF SUSPENSION

When a user becomes suspended:

New login must fail.

Additionally review refresh-token behavior.

Preferred behavior:

```text
Suspend User
↓
Revoke active RefreshTokens
↓
Future refresh fails
↓
New login fails
```

Existing access JWT may remain valid until its short expiry unless project architecture already supports token revocation.

Do NOT build a distributed JWT blacklist just for this milestone.

Document this behavior.

---

# 20. USER STATUS AUDIT

Admin status changes must create:

```text
USER_SUSPEND
USER_ACTIVATE
```

Audit:

```text
Admin UserId
Target UserId
Action
EntityType = User
EntityId
Description
IpAddress
CreatedAtUtc
```

---

# 21. SELF-PROTECTION

Admin should not accidentally lock themselves out.

For the current simulation, block:

```text
ADMIN suspending their own user
```

unless there is another explicit super-admin design.

Return business validation error.

---

# 22. ROLE MANAGEMENT — KEEP SIMPLE

Do NOT build a complex IAM system.

Existing roles:

```text
CUSTOMER
STAFF
ADMIN
```

If role management is not necessary for V1, keep roles read-only.

Do NOT add arbitrary custom roles/permissions.

If existing seeder lacks a STAFF demo account, add one.

---

# 23. STAFF DEMO ACCOUNT

Ensure development seeder has:

```text
staff@locallink.local
```

Role:

```text
STAFF
```

Use existing development password convention.

Seeder must be idempotent.

Do not store plaintext password outside development documentation.

---

# 24. STAFF PERMISSION MATRIX

Verify:

```text
Feature                         CUSTOMER   STAFF   ADMIN

Own profile                        YES       -       -
Own accounts                       YES       -       -
Own transfers                      YES       -       -
Own payments                       YES       -       -

Customer search                    NO       YES     YES
Customer detail                    NO       YES     YES
Customer accounts                  NO       YES     YES

Admin transaction search           NO       YES     YES
Admin payment search               NO       YES     YES
Dashboard                          NO       YES     YES

Change customer status             NO       NO      YES
Change account status              NO       NO      YES
User management                    NO       NO      YES
Audit logs                         NO       NO      YES
```

Adjust only where existing project rules explicitly differ.

---

# 25. SYSTEM INFORMATION

Existing:

```http
GET /api/system
GET /health
```

Review them.

Do not duplicate.

Do not expose:

```text
connection string
DB password
JWT secret
environment secrets
filesystem paths
```

Health endpoint should remain safe.

---

# 26. PAGINATION STANDARDIZATION

Review ALL paginated endpoints from M4–M7.

Use one consistent model:

```json
{
  "items": [],
  "page": 1,
  "pageSize": 20,
  "totalItems": 100,
  "totalPages": 5
}
```

Set validation:

```text
page >= 1

pageSize >= 1
pageSize <= 100
```

Do not allow:

```text
pageSize=1000000
```

---

# 27. SORTING

For admin operational lists, provide sensible default ordering:

```text
Transactions → newest first
Payments → newest first
AuditLogs → newest first
Users → newest or email
Customers → newest or customer code
```

Do not implement arbitrary dynamic SQL sorting unless needed.

---

# 28. DATE FILTER VALIDATION

For:

```text
fromDate
toDate
```

Validate:

```text
fromDate <= toDate
```

Use UTC consistently.

Invalid range:

```text
400
```

---

# 29. ERROR RESPONSE STANDARDIZATION

Review exceptions from M3–M6.

All APIs should use existing ProblemDetails architecture.

Standard HTTP semantics:

```text
400 Invalid business request
401 Not authenticated
403 Authenticated but forbidden
404 Resource not found
409 Conflict / concurrency / idempotency
500 Unexpected server error
```

Do not leak stack traces in normal API responses.

---

# 30. BUSINESS ERROR CODES

Where architecture supports error codes, standardize important codes such as:

```text
VALIDATION_ERROR
RESOURCE_NOT_FOUND
FORBIDDEN
INSUFFICIENT_FUNDS
IDEMPOTENCY_CONFLICT
CONCURRENCY_CONFLICT
USER_SUSPENDED
ACCOUNT_LOCKED
BILL_ALREADY_PAID
```

Do not rewrite the entire exception architecture if current implementation is already consistent.

---

# 31. RATE LIMITING

Add basic ASP.NET Core rate limiting only if it integrates cleanly.

Priority endpoints:

```text
POST /api/v1/auth/login
POST /api/v1/auth/refresh
POST /api/v1/transfers
POST /api/v1/payments
```

Use built-in ASP.NET Core RateLimiter.

Do NOT add Redis.

Do NOT create distributed rate limiting.

For local portfolio V1, in-memory limiter is sufficient.

---

# 32. LOGIN RATE LIMIT

Recommended conservative local configuration:

```text
Login:
approximately 10 requests / minute / client
```

Do not hardcode an unreasonable production policy.

Make configuration adjustable.

---

# 33. FINANCIAL ENDPOINT RATE LIMIT

For simulation:

```text
Transfers / Payments:
reasonable per-user/request limit
```

Do not interfere with idempotency.

Rate limiting is defense-in-depth, not financial correctness.

---

# 34. SECURITY HEADERS

Review API response headers.

Where appropriate add safe defaults such as:

```text
X-Content-Type-Options: nosniff
X-Frame-Options: DENY
```

Do not add random headers without understanding them.

CORS must remain configurable.

---

# 35. CORS REVIEW

Development should allow configured Nuxt origin:

```text
http://localhost:3000
```

Do NOT combine:

```text
AllowAnyOrigin
+
AllowCredentials
```

Review existing CorsExtensions.

Do not break Docker/local development.

---

# 36. JWT CONFIG REVIEW

Verify:

```text
Issuer validation
Audience validation
Signature validation
Expiration validation
Clock skew reasonable
JWT secret from configuration/environment
```

No hardcoded production secret in source code.

---

# 37. REFRESH TOKEN REVIEW

Verify:

```text
Raw token not stored
SHA-256 hash stored
Rotation works
Old token reuse rejected
Logout revokes token
Suspended user cannot refresh
```

Existing M3 behavior must not regress.

---

# 38. PASSWORD REVIEW

Verify:

```text
No plaintext passwords in DB
PBKDF2 via ASP.NET Identity PasswordHasher
```

Do not replace working password hashing.

---

# 39. FINANCIAL SECURITY REGRESSION

Re-test M5/M6 invariants:

```text
Customer cannot debit another customer's account

Customer cannot change Balance directly

Transfer duplicate cannot double debit

Payment duplicate cannot double debit

Concurrent balance updates protected by RowVersion

Locked account cannot transfer/pay

Paid bill cannot be paid again

Payment amount comes from server-side Bill
```

---

# 40. DATABASE CONSTRAINT REVIEW

Review important constraints/indexes:

```text
BankAccount balance >= 0
Transaction amount > 0
Transfer amount > 0
Payment amount > 0

AccountNumber unique
CustomerCode unique
Transaction Reference unique
Transfer IdempotencyKey unique filtered
Payment IdempotencyKey unique filtered
```

Do not create duplicate indexes.

---

# 41. DASHBOARD TOTAL BALANCE

When calculating:

```text
totalBalance
```

use:

```text
decimal
```

Do not cast financial values to:

```text
float
double
```

---

# 42. ADMIN SERVICES

Recommended organization:

```text
Application/
└── Admin/
    ├── DTOs/
    └── Interfaces/

Infrastructure/
└── Services/
    ├── AdminDashboardService.cs
    ├── AdminTransactionService.cs
    ├── AdminPaymentService.cs
    ├── AdminAuditService.cs
    └── AdminUserService.cs
```

Follow existing project convention if different.

Do not introduce CQRS/MediatR.

---

# 43. CONTROLLERS

Prefer clear controllers such as:

```text
AdminDashboardController
AdminTransactionsController
AdminPaymentsController
AdminAuditLogsController
AdminUsersController
```

or equivalent existing convention.

Do not create one 2,000-line AdminController.

---

# 44. DTO ONLY

Never return EF entities directly.

Create focused DTOs.

Example:

```text
AdminDashboardDto
AdminTransactionListItemDto
AdminTransactionDetailDto
AdminPaymentListItemDto
AdminPaymentDetailDto
AuditLogDto
AdminUserListItemDto
AdminUserDetailDto
UpdateUserStatusRequest
```

Reuse existing:

```text
PagedResult<T>
```

---

# 45. QUERY PERFORMANCE

Read-only:

```text
AsNoTracking()
```

Projection:

```text
Select(...)
```

Pagination:

```text
Skip
Take
```

Avoid unnecessary:

```text
Include()
```

of huge object graphs.

---

# 46. N+1 REVIEW

Review admin list endpoints for N+1 query problems.

Do not execute one query per row for:

```text
Customer
Account
Role
Transaction
Payment
```

Use projection/join appropriately.

---

# 47. SWAGGER FINALIZATION

Swagger should clearly group:

```text
Auth
Customers
Accounts
Beneficiaries
Transfers
Transactions
Bills
Payments
Notifications
Admin Dashboard
Admin Transactions
Admin Payments
Admin Users
Admin Audit
System
```

Bearer Authorize remains functional.

---

# 48. SWAGGER SECURITY

Do not put real secrets in Swagger examples.

Demo credentials may be documented only as clearly development-only credentials.

---

# 49. INTEGRATION TEST PROJECT

Review current test architecture.

If only UnitTests exist, create:

```text
backend/tests/LocalLink.IntegrationTests
```

only if it can be implemented cleanly.

Add project to:

```text
LocalLink.sln
```

Prefer testing against actual SQL Server/container where financial concurrency behavior matters.

Do not fake SQL Server RowVersion guarantees using EF InMemory and claim production-level concurrency verification.

---

# 50. CRITICAL INTEGRATION FLOWS

At minimum verify:

```text
Login
↓
GET /me

Login Customer
↓
GET accounts

Customer
↓
Transfer

Customer
↓
Pay Bill

Staff
↓
Admin dashboard

Staff
↓
Search customers

Admin
↓
Suspend customer/user

Suspended user
↓
Login rejected

Admin
↓
Audit log
```

---

# 51. AUTHORIZATION TEST MATRIX

Automated tests should verify:

```text
CUSTOMER → Admin Dashboard = 403
CUSTOMER → Admin Transactions = 403
CUSTOMER → Admin Payments = 403
CUSTOMER → Admin Users = 403
CUSTOMER → Audit Logs = 403

STAFF → Dashboard = 200
STAFF → Transactions = 200
STAFF → Payments = 200
STAFF → Users = 403
STAFF → Audit Logs = 403
STAFF → Status mutation = 403

ADMIN → all authorized admin endpoints = success
```

---

# 52. ADMIN DASHBOARD TESTS

Test:

```text
customer totals
active/suspended counts
account totals
active/locked counts
total balance
transfer count
transfer volume
payment count
payment volume
```

Use deterministic seeded data where possible.

---

# 53. USER SUSPENSION TEST

Test:

```text
Admin suspends User A
↓
Audit USER_SUSPEND created
↓
Refresh tokens revoked
↓
User A login rejected
↓
User A refresh rejected
```

Then:

```text
Admin activates User A
↓
Audit USER_ACTIVATE
↓
Login works again
```

---

# 54. ADMIN SELF-SUSPENSION TEST

Admin:

```text
PATCH own status → SUSPENDED
```

Expected:

```text
400
```

or appropriate business error.

Admin remains active.

---

# 55. PAGINATION TESTS

Test:

```text
page < 1 → validation error

pageSize < 1 → validation error

pageSize > 100 → validation error
```

Verify SQL pagination still works.

---

# 56. AUDIT IMMUTABILITY TEST

Verify there are no public/admin endpoints capable of:

```text
editing
deleting
```

AuditLog.

---

# 57. EXISTING TESTS

Current baseline:

```text
61 / 61 passed
```

ALL must continue to pass.

Do not delete tests merely to make build green.

---

# 58. TARGET TESTS

Add meaningful tests for:

```text
Dashboard
Admin transaction search
Admin payment search
Audit search
Admin user search
User suspension
Refresh revocation
Self-suspension protection
STAFF permissions
CUSTOMER denial
Pagination validation
Date range validation
```

Test count is secondary to correctness.

---

# 59. BUILD

Run:

```bash
dotnet restore backend/LocalLink.sln
dotnet build backend/LocalLink.sln
dotnet test backend/LocalLink.sln
```

Expected:

```text
0 build errors
0 failed tests
```

Prefer:

```text
0 warnings
```

unless unavoidable third-party warnings are documented.

---

# 60. CLEAN DOCKER VERIFICATION

Because this is backend finalization, perform clean verification:

```bash
docker compose down -v
docker compose up --build -d
```

Verify:

```text
SQL Server = Healthy
API = Healthy
Frontend = Operational
```

Verify all migrations applied.

Verify seed is idempotent.

---

# 61. LIVE CUSTOMER REGRESSION

Login:

```text
customer1@locallink.local
```

Verify:

```text
GET /me
GET /customers/me
GET /accounts
GET /bills
GET /payments
GET /transfers
GET /transactions
GET /notifications
```

No regression.

---

# 62. LIVE STAFF TEST

Login:

```text
staff@locallink.local
```

Verify:

```text
GET /api/v1/admin/dashboard
→ 200

GET /api/v1/admin/customers
→ 200

GET /api/v1/admin/transactions
→ 200

GET /api/v1/admin/payments
→ 200

GET /api/v1/admin/users
→ 403

GET /api/v1/admin/audit-logs
→ 403
```

---

# 63. LIVE ADMIN TEST

Login:

```text
admin@locallink.local
```

Verify:

```text
GET /api/v1/admin/dashboard
GET /api/v1/admin/customers
GET /api/v1/admin/transactions
GET /api/v1/admin/payments
GET /api/v1/admin/users
GET /api/v1/admin/audit-logs
```

Expected:

```text
200
```

---

# 64. LIVE USER SUSPENSION

Use ADMIN.

Suspend a non-admin demo user.

Verify:

```text
status changed
audit created
refresh revoked
login rejected
```

Reactivate.

Verify login works again.

Restore seed/demo state after verification if necessary.

---

# 65. RATE LIMIT LIVE TEST

If rate limiting was implemented:

Verify normal usage works.

Verify abusive repeated login requests eventually return:

```text
429 Too Many Requests
```

Do not leave rate limit so strict that E2E tests become unreliable.

---

# 66. README FINAL BACKEND DOCUMENTATION

Update:

```text
README.md
Docs/README.md
Docs/api/README.md
```

Add:

```text
Architecture
RBAC Matrix
Customer APIs
Staff APIs
Admin APIs
Financial Integrity
Idempotency
Concurrency
Audit Strategy
Rate Limiting
Docker Setup
Demo Accounts
```

---

# 67. ADMIN API DOCUMENTATION

Create if appropriate:

```text
Docs/api/admin-dashboard.md
Docs/api/admin-transactions.md
Docs/api/admin-payments.md
Docs/api/admin-users.md
Docs/api/admin-audit.md
```

Include TypeScript response interfaces where useful for upcoming Nuxt frontend.

---

# 68. FRONTEND CONTRACT DOCUMENT

Create/update a concise frontend contract:

```text
Docs/api/frontend-contract.md
```

Include:

```text
Base URL

Authentication
Authorization roles

Pagination model
ProblemDetails model

Customer endpoints
Transfer endpoints
Transaction endpoints
Bill endpoints
Payment endpoints
Notification endpoints

Admin endpoints

Idempotency-Key requirements
```

This document will become the contract for M8 Nuxt development.

---

# 69. DO NOT BUILD FRONTEND YET

Only update project milestone status.

Do NOT implement:

```text
Login UI
Dashboard UI
Transfer UI
Bill UI
Admin UI
```

Those belong to M8.

---

# 70. FRONTEND ROADMAP STATUS

After M7 passes:

```text
M1 COMPLETED
M2 COMPLETED
M3 COMPLETED
M4 COMPLETED
M5 COMPLETED
M6 COMPLETED
M7 COMPLETED

BACKEND V1 COMPLETE
```

---

# 71. DATABASE MIGRATIONS

Only add migration if M7 genuinely changes schema.

Possible legitimate reasons:

```text
missing index
LastLoginAtUtc
necessary constraint
```

Do not create migration just because this is a new milestone.

Do not edit old migrations.

---

# 72. NO NEW FINANCIAL FEATURES

Do NOT implement:

```text
Loans
Credit Cards
Interest
Deposits
Withdrawals
External bank integration
FX
Crypto
Recurring payments
Merchant settlement
```

These are outside V1.

---

# 73. NO OVER-ENGINEERING

Do NOT introduce:

```text
Kafka
RabbitMQ
Redis
Kubernetes
Microservices
Event Sourcing
CQRS
MediatR
Elasticsearch
```

just for M7.

Current Modular Monolith is sufficient.

---

# 74. FINAL SECURITY CHECKLIST

Verify:

```text
[ ] Passwords hashed
[ ] JWT secret not hardcoded
[ ] Refresh tokens hashed
[ ] Refresh rotation works
[ ] Suspended users cannot login
[ ] Suspended users cannot refresh
[ ] Customer ownership enforced
[ ] Staff RBAC enforced
[ ] Admin RBAC enforced
[ ] Balance cannot be directly edited
[ ] Transfer idempotency works
[ ] Payment idempotency works
[ ] RowVersion remains active
[ ] Audit logs read-only
[ ] Pagination capped
[ ] CORS safe
[ ] ProblemDetails does not expose stack traces
[ ] Swagger does not expose secrets
```

---

# 75. BACKEND V1 DEFINITION OF DONE

M7 is COMPLETE only when:

```text
[ ] Admin dashboard works

[ ] STAFF dashboard access works
[ ] CUSTOMER dashboard denied

[ ] Admin transaction search works
[ ] Admin transaction detail works

[ ] Admin payment search works
[ ] Admin payment detail works

[ ] AuditLog search works
[ ] AuditLog ADMIN-only

[ ] User list works
[ ] User detail works

[ ] User suspension works
[ ] User activation works
[ ] Refresh tokens revoked on suspension
[ ] Self-suspension blocked

[ ] STAFF permission matrix verified
[ ] ADMIN permission matrix verified
[ ] CUSTOMER isolation verified

[ ] Pagination standardized
[ ] Date validation standardized

[ ] Security configuration reviewed
[ ] CORS reviewed
[ ] JWT reviewed
[ ] Refresh token security reviewed

[ ] Financial regression tests pass
[ ] Existing 61 tests pass
[ ] New tests pass

[ ] Clean Docker rebuild passes
[ ] SQL Server healthy
[ ] API healthy
[ ] Frontend operational

[ ] Swagger finalized
[ ] Backend docs finalized
[ ] Frontend API contract created

[ ] Feature branch pushed

[ ] BACKEND V1 COMPLETE
```

---

# 76. GIT

After ALL verification passes:

```bash
git status
```

Verify:

```text
.env NOT tracked
secrets NOT tracked
```

Then:

```bash
git add .
git commit -m "feat: finalize admin operations and backend v1"
git push -u origin feature/admin-backend
```

Follow current repository workflow to integrate into:

```text
feature/hoi
```

Do not merge into `main` automatically unless explicitly instructed.

---

# 77. FINAL REPORT

After implementation, return exactly a structured report:

```text
LOCALINK MILESTONE 7 — BACKEND V1 FINAL REPORT

Branch:

Backend V1 Status:

Admin Dashboard:

Dashboard Metrics:

Admin Transaction APIs:

Admin Payment APIs:

Audit APIs:

User Management APIs:

STAFF Permissions:

ADMIN Permissions:

CUSTOMER Isolation:

User Suspension Strategy:

Refresh Token Revocation:

Self-Suspension Protection:

Rate Limiting:

Security Headers:

CORS Review:

JWT Review:

Refresh Token Review:

Financial Integrity Regression:

Pagination Standard:

ProblemDetails Standard:

Database Changes:

Migration:

Indexes:

Staff Seeder:

Tests Before:
61

Tests Added:

Tests Total:

Tests Result:

Integration Tests:

Docker Clean Rebuild:

Customer Regression:

Staff E2E:

Admin E2E:

Suspension E2E:

Swagger:

Documentation:

Frontend API Contract:

Git Commit:

Git Push:

Warnings / Known Limitations:

BACKEND V1:
COMPLETE / NOT COMPLETE
```

If any critical test fails:

```text
BACKEND V1 = NOT COMPLETE
```

Do not hide failures.

Do not claim 100% success unless actually verified.

Then STOP.

Do not begin M8 Frontend automatically.