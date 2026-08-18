# LocalLink M8 — FE5: Staff/Admin Management UI

## 0. CURRENT PROJECT STATUS

Project hiện đã hoàn thành:

### Backend V1

```text
M1 — Foundation & DevOps                     ✅
M2 — Database Foundation                     ✅
M3 — Authentication + JWT + RBAC            ✅
M4 — Customer & Bank Accounts               ✅
M5 — Transfer Engine + Transactions          ✅
M6 — Bills + Payments + Notifications        ✅
M7 — Staff/Admin Backend + Finalization       ✅
```

### Frontend

```text
FE1 — Authentication + App Shell              ✅
FE2 — Dashboard + Accounts + Profile          ✅
FE3 — Transfer + Transactions                 ✅
FE4 — Bills + Payments + Notifications        ✅
FE5 — Staff/Admin Management UI               🔄 IMPLEMENT NOW
FE6 — Final Integration & Polish              ⏳
```

Frontend stack:

```text
Nuxt 4 SPA
Vue 3
TypeScript
Quasar
Pinia
$fetch
```

Backend:

```text
ASP.NET Core .NET 10
EF Core 10
SQL Server 2022
JWT + Refresh Token
RBAC
Docker Compose
```

FE5 là milestone chức năng lớn cuối của Frontend V1.

KHÔNG bắt đầu FE6 tự động.

---

# 1. OBJECTIVE

Xây dựng giao diện vận hành hoàn chỉnh cho hai role:

```text
STAFF
ADMIN
```

Sau FE5, Staff/Admin phải có thể sử dụng giao diện web để:

```text
Dashboard
Customer Management
Customer Detail
Customer Accounts
Global Transactions
Global Payments
User Management (ADMIN)
Audit Logs (ADMIN)
Customer Status Management (ADMIN)
Account Status Management (ADMIN)
User Status Management (ADMIN)
```

Frontend phải consume Backend V1 hiện có.

KHÔNG tạo business logic quản trị ở frontend.

Backend tiếp tục là source of truth.

---

# 2. TESTING RULE — IMPORTANT

KHÔNG sử dụng Playwright/browser automation cho verification mặc định.

KHÔNG:

```text
install Playwright
run Playwright
create Playwright tests
generate browser screenshots
```

Verification ưu tiên:

```text
1. API E2E PowerShell
2. npm run build
3. dotnet test backend/LocalLink.sln
4. Docker health
5. Manual UI smoke check tối thiểu nếu cần
```

Tạo:

```text
scripts/verify-fe5-api.ps1
```

để verify các Admin/Staff API cần cho FE5.

---

# 3. FEATURE BRANCH

Inspect Git state trước:

```bash
git status
git branch
```

Tạo branch từ frontend state mới nhất:

```bash
git checkout feature/hoi
git pull origin feature/hoi
git checkout -b feature/frontend-admin
```

Nếu FE4 chưa được integrate vào `feature/hoi`, branch từ branch FE4 mới nhất thay vì làm mất code.

Không làm trực tiếp trên `main`.

---

# 4. INSPECT EXISTING CODE FIRST

Trước khi implement:

Inspect:

```text
frontend/layouts/admin.vue
frontend/stores/auth.ts
frontend/services/api.ts
frontend/types/
frontend/services/
frontend/middleware/admin.ts
frontend/components/common/
frontend/utils/
```

Reuse:

```text
Pinia auth state
authenticated API client
401 refresh handling
ProblemDetails handling
currency formatter
date formatter
AppAlert
Quasar setup
Admin layout from FE1
```

Không duplicate:

```text
API client
auth store
error framework
pagination types
currency/date utilities
```

---

# 5. READ ACTUAL BACKEND CONTRACT

Đọc trước:

```text
Docs/api/frontend-contract.md
Docs/api/admin-dashboard.md
Docs/api/admin-transactions.md
Docs/api/admin-payments.md
Docs/api/admin-users.md
Docs/api/admin-audit.md
```

Nếu tên file khác, tìm tài liệu Admin thực tế hiện có.

Inspect actual Backend controllers/DTOs:

```text
AdminDashboardController
AdminCustomersController / existing customer admin controller
AdminTransactionsController
AdminPaymentsController
AdminUsersController
AdminAuditLogsController
```

Nếu docs khác implementation:

```text
BACKEND IMPLEMENTATION = SOURCE OF TRUTH
```

Không đoán DTO hoặc query parameter.

---

# 6. AUTHORIZATION MODEL

Existing roles:

```text
CUSTOMER
STAFF
ADMIN
```

FE5 route scope:

```text
/admin/*
```

Only:

```text
STAFF
ADMIN
```

CUSTOMER truy cập `/admin/*`:

```text
redirect /dashboard
```

Frontend guard chỉ phục vụ UX.

Backend RBAC vẫn là security boundary chính.

---

# 7. STAFF VS ADMIN PERMISSION MATRIX

UI phải tuân thủ:

```text
Feature                        STAFF       ADMIN
-------------------------------------------------
Admin Dashboard                YES         YES
Customers List                 YES         YES
Customer Detail                YES         YES
Customer Accounts              YES         YES
Global Transactions            YES         YES
Global Payments                YES         YES

Change Customer Status         NO          YES
Change Account Status          NO          YES

Users List                     NO          YES
User Detail                    NO          YES
Change User Status             NO          YES

Audit Logs                     NO          YES
```

Nếu actual Backend RBAC khác:

Follow Backend.

Không hiển thị action mà role không có quyền.

---

# 8. ADMIN ROUTES

Implement/reuse routes:

```text
/admin
/admin/customers
/admin/customers/[id]
/admin/transactions
/admin/transactions/[id]
/admin/payments
/admin/payments/[id]
/admin/users
/admin/users/[id]
/admin/audit-logs
/admin/audit-logs/[id]
```

All routes use admin middleware.

Additional role handling inside route/page where ADMIN-only.

---

# 9. UPDATE ADMIN LAYOUT

Update:

```text
frontend/layouts/admin.vue
```

Navigation:

```text
Dashboard
Customers
Transactions
Payments
Users
Audit Logs
```

For STAFF:

```text
Dashboard        visible
Customers        visible
Transactions     visible
Payments         visible
Users            HIDDEN
Audit Logs       HIDDEN
```

For ADMIN:

```text
All visible
```

Navigation based on authenticated roles.

No hardcoded role assumption.

---

# 10. ADMIN HEADER

Header should display:

```text
Current user
Role
Profile/User menu
Logout
```

Example:

```text
staff@locallink.local
STAFF
```

or Admin profile information returned by `/auth/me`.

Do not display sensitive data.

---

# 11. RESPONSIVE ADMIN SHELL

Desktop:

```text
Persistent sidebar
Header
Main content
```

Mobile/tablet:

```text
Drawer
Hamburger
Responsive cards/tables
```

No essential horizontal overflow.

Wide tables may use responsive alternatives when necessary.

---

# 12. TYPES — ADMIN DASHBOARD

Create:

```text
frontend/types/adminDashboard.ts
```

Use actual DTO.

Expected conceptual sections:

```text
customers
accounts
today transfers
transfer volume
payments
payment volume
total balance
generatedAtUtc
```

Do not assume exact property names without inspecting Backend.

---

# 13. TYPES — ADMIN CUSTOMER

Reuse existing customer/account types where possible.

Create only Admin-specific types if necessary:

```text
AdminCustomerListItem
AdminCustomerDetail
AdminCustomerAccount
UpdateCustomerStatusRequest
UpdateAccountStatusRequest
```

Avoid duplicate structures where shared DTO is identical.

---

# 14. TYPES — ADMIN TRANSACTION

Create/update:

```text
frontend/types/adminTransaction.ts
```

Conceptually:

```text
AdminTransactionListItem
AdminTransactionDetail
AdminTransactionFilters
```

Follow actual contract.

---

# 15. TYPES — ADMIN PAYMENT

Create:

```text
frontend/types/adminPayment.ts
```

Conceptually:

```text
AdminPaymentListItem
AdminPaymentDetail
AdminPaymentFilters
```

---

# 16. TYPES — ADMIN USER

Create:

```text
frontend/types/adminUser.ts
```

Conceptually:

```text
AdminUserListItem
AdminUserDetail
UpdateUserStatusRequest
```

Never include frontend fields for:

```text
PasswordHash
RefreshTokenHash
JWT Secret
```

---

# 17. TYPES — AUDIT LOG

Create:

```text
frontend/types/auditLog.ts
```

Follow Backend DTO.

Expected conceptual fields:

```text
id
userId
action
entityType
entityId
description
ipAddress
createdAtUtc
```

Audit UI is read-only.

---

# 18. SERVICES

Create/reuse:

```text
frontend/services/adminDashboardService.ts
frontend/services/adminCustomerService.ts
frontend/services/adminTransactionService.ts
frontend/services/adminPaymentService.ts
frontend/services/adminUserService.ts
frontend/services/adminAuditService.ts
```

ALL use:

```text
services/api.ts
```

Do NOT create separate auth interceptor.

---

# 19. ADMIN DASHBOARD SERVICE

Functions conceptually:

```text
getDashboard()
```

Call actual:

```http
GET /api/v1/admin/dashboard
```

No client-side aggregation from huge datasets.

Use Backend aggregated metrics.

---

# 20. ADMIN CUSTOMER SERVICE

Reuse M4 Admin endpoints.

Functions:

```text
getCustomers(filters)
getCustomer(id)
getCustomerAccounts(id)
updateCustomerStatus(id, status)   // ADMIN only
updateAccountStatus(id, status)    // ADMIN only
```

Follow actual endpoint paths.

---

# 21. ADMIN TRANSACTION SERVICE

Functions:

```text
getTransactions(filters)
getTransaction(id)
```

Filters only actual Backend-supported parameters such as:

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

Do not invent filters.

---

# 22. ADMIN PAYMENT SERVICE

Functions:

```text
getPayments(filters)
getPayment(id)
```

Use actual filters:

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

only if Backend supports them.

---

# 23. ADMIN USER SERVICE

ADMIN only:

```text
getUsers(filters)
getUser(id)
updateUserStatus(id, status)
```

Do not implement password change/reset unless Backend has endpoint.

Do not invent user creation flow.

---

# 24. ADMIN AUDIT SERVICE

ADMIN only:

```text
getAuditLogs(filters)
getAuditLog(id)
```

No:

```text
update
delete
create
```

Audit logs remain read-only.

---

# 25. ADMIN DASHBOARD PAGE

Replace placeholder:

```text
pages/admin/index.vue
```

with real operations dashboard.

Use:

```http
GET /api/v1/admin/dashboard
```

---

# 26. DASHBOARD KPI CARDS

Display meaningful metrics from Backend.

Examples according to actual DTO:

```text
Total Customers
Active Customers
Suspended Customers

Total Accounts
Active Accounts
Locked Accounts
Total Balance

Transfers Today
Transfer Volume Today

Payments Today
Payment Volume Today
```

Use existing currency formatter.

Financial metrics displayed as VND.

---

# 27. DASHBOARD VISUAL HIERARCHY

Priority:

```text
1. Customers
2. Accounts / Total Balance
3. Transfers Today
4. Payments Today
5. Operational shortcuts
```

Do not make system telemetry dominate the Admin Dashboard.

---

# 28. DASHBOARD QUICK ACTIONS

Provide links:

```text
Xem khách hàng
Xem giao dịch
Xem thanh toán
```

ADMIN additionally:

```text
Quản lý users
Xem Audit Logs
```

---

# 29. DASHBOARD LOADING / ERROR

Use:

```text
QSkeleton
AppAlert
Retry
```

Dashboard failure should not crash Admin shell.

---

# 30. CUSTOMERS PAGE

Implement:

```text
pages/admin/customers/index.vue
```

Use existing M4 endpoint:

```http
GET /api/v1/admin/customers
```

---

# 31. CUSTOMER SEARCH

Support Backend filters.

Expected:

```text
search
status
page
pageSize
```

Search could match:

```text
CustomerCode
FullName
Email
Phone
```

but Backend decides actual semantics.

Do not filter full result client-side.

---

# 32. CUSTOMER TABLE

Desktop display:

```text
Customer Code
Full Name
Email
Phone
Status
Created/other useful field
Action
```

according to API response.

Click:

```text
/admin/customers/{id}
```

---

# 33. CUSTOMER MOBILE VIEW

On narrow screens:

Use responsive cards/list.

Avoid unusable wide table.

Display important information first:

```text
Name
Customer Code
Status
Contact
```

---

# 34. CUSTOMER STATUS BADGES

Render:

```text
ACTIVE
SUSPENDED
CLOSED
```

as readable Vietnamese labels.

Do not rely on color alone.

---

# 35. CUSTOMER DETAIL PAGE

Implement:

```text
pages/admin/customers/[id].vue
```

Show:

```text
Customer profile
User status
Roles
Customer status
Accounts
```

according to actual DTO.

No secrets.

---

# 36. CUSTOMER ACCOUNTS

Show account cards/table:

```text
Account Number
Account Name
Account Type
Balance
Currency
Status
```

STAFF:

read-only.

ADMIN:

can manage allowed account status transitions.

---

# 37. ADMIN CUSTOMER STATUS ACTION

Only ADMIN sees:

```text
Suspend Customer
Activate Customer
```

according to current state.

Before mutation:

Show confirmation dialog.

Example:

```text
Bạn có chắc muốn tạm khóa khách hàng này?
```

Do not immediately mutate without confirmation.

---

# 38. CUSTOMER STATUS REQUEST

Call existing:

```http
PATCH /api/v1/admin/customers/{id}/status
```

with actual request DTO.

After success:

Refetch Customer detail.

Do not manually trust optimistic local status.

Backend is source of truth.

---

# 39. STAFF CUSTOMER DETAIL

STAFF must see same read-only operational information where permitted.

Hide:

```text
status mutation buttons
```

Do not merely disable them if hiding gives cleaner UX.

Backend still returns 403 if manually called.

---

# 40. ACCOUNT STATUS MANAGEMENT

ADMIN only.

Allowed existing operations:

```text
ACTIVE → LOCKED
LOCKED → ACTIVE
```

Do not expose Balance editing.

Do not expose AccountNumber editing.

Do not expose CustomerId editing.

---

# 41. ACCOUNT STATUS CONFIRMATION

Before lock/unlock:

Use QDialog.

For locking, explain consequence:

```text
Tài khoản bị khóa sẽ không thể thực hiện chuyển tiền hoặc thanh toán.
```

Do not invent further side effects.

---

# 42. GLOBAL TRANSACTIONS PAGE

Implement:

```text
pages/admin/transactions/index.vue
```

STAFF + ADMIN.

Call actual:

```http
GET /api/v1/admin/transactions
```

---

# 43. TRANSACTION FILTERS

Use only Backend-supported filters:

```text
search
type
status
fromDate
toDate
accountNumber
page
pageSize
```

Validate dates:

```text
fromDate <= toDate
```

---

# 44. TRANSACTIONS TABLE

Desktop:

```text
Reference
Type
Source
Destination
Amount
Status
Time
```

according to DTO.

Use currency/date utilities.

---

# 45. ADMIN TRANSACTION DETAIL

Implement:

```text
pages/admin/transactions/[id].vue
```

Show operational transaction data:

```text
Reference
Type
Status
Amount
Currency
Source
Destination
Customer summary if returned
Description
Created time
Completed time
```

No modification actions.

Financial history remains immutable.

---

# 46. GLOBAL PAYMENTS PAGE

Implement:

```text
pages/admin/payments/index.vue
```

STAFF + ADMIN.

Call:

```http
GET /api/v1/admin/payments
```

---

# 47. PAYMENT FILTERS

Use actual Backend filters.

Possible:

```text
status
billType
reference
accountNumber
fromDate
toDate
page
pageSize
```

No client-side full dataset filtering.

---

# 48. PAYMENT TABLE

Display:

```text
Reference
Customer
Provider
Bill
Account
Amount
Status
Paid Time
```

according to actual DTO.

---

# 49. ADMIN PAYMENT DETAIL

Implement:

```text
pages/admin/payments/[id].vue
```

Show:

```text
Payment reference
Customer
Account
Bill
Provider
Amount
Status
PaidAt
Transaction reference
```

Read-only.

---

# 50. USERS PAGE — ADMIN ONLY

Implement:

```text
pages/admin/users/index.vue
```

ADMIN only.

STAFF opening URL manually:

Frontend should redirect to:

```text
/admin
```

with permission message.

Backend remains authoritative 403.

---

# 51. USER FILTERS

Use Backend-supported:

```text
search
status
role
page
pageSize
```

Search actual fields supported by Backend.

---

# 52. USER TABLE

Display safe fields:

```text
Email
Full Name / Customer Code if available
Roles
Status
CreatedAt
LastLoginAt if available
```

Never display:

```text
PasswordHash
RefreshToken hashes
```

---

# 53. USER DETAIL

Implement:

```text
pages/admin/users/[id].vue
```

ADMIN only.

Show:

```text
Email
Status
Roles
Linked Customer summary
CreatedAt
Last Login
```

according to DTO.

---

# 54. USER STATUS MANAGEMENT

ADMIN only.

Existing flow:

```text
ACTIVE ↔ SUSPENDED
```

Use actual Backend allowed transitions.

Before suspend:

Show confirmation.

Explain:

```text
User sẽ không thể đăng nhập hoặc refresh session sau khi bị tạm khóa.
```

This matches Backend M7 behavior.

---

# 55. ADMIN SELF-SUSPENSION UX

Backend blocks Admin suspending themselves.

Frontend should proactively disable/hide self-suspend action if current user ID equals target user ID.

Show tooltip/message:

```text
Bạn không thể tạm khóa tài khoản quản trị đang đăng nhập.
```

Backend remains final enforcement.

---

# 56. AFTER USER STATUS UPDATE

After success:

Refetch User.

Do not manually assume backend state.

Use QNotify for success.

Example:

```text
Đã tạm khóa tài khoản.
```

---

# 57. AUDIT LOGS PAGE — ADMIN ONLY

Implement:

```text
pages/admin/audit-logs/index.vue
```

ADMIN only.

Use:

```http
GET /api/v1/admin/audit-logs
```

---

# 58. AUDIT FILTERS

Use actual filters:

```text
action
entityType
userId
fromDate
toDate
search
page
pageSize
```

Newest first according to Backend.

---

# 59. AUDIT TABLE

Display:

```text
Action
User
Entity Type
Entity ID
Description
IP Address
CreatedAt
```

according to DTO.

Audit logs are read-only.

No Edit/Delete UI.

---

# 60. AUDIT DETAIL

Implement:

```text
pages/admin/audit-logs/[id].vue
```

if detail endpoint exists and adds value.

Otherwise a QDialog/detail panel is acceptable.

Do not invent endpoint.

---

# 61. AUDIT SECURITY

STAFF:

```text
Audit navigation hidden
Audit route denied
```

ADMIN:

allowed.

No audit mutation buttons for any role.

---

# 62. PAGINATION

Reuse shared:

```text
PagedResult<T>
```

All Admin list pages must use server-side pagination.

Standard:

```text
page
pageSize
totalItems
totalPages
```

No client-side fake pagination.

---

# 63. PAGE SIZE

Options:

```text
10
20
50
```

Do not request above Backend maximum.

---

# 64. FILTER URL STATE

Where practical, sync important filters with query string.

Example:

```text
/admin/transactions?type=TRANSFER&page=2
```

Useful for:

```text
refresh persistence
sharing
browser back
```

Do not over-engineer.

---

# 65. DATE VALIDATION

Before API call:

```text
fromDate <= toDate
```

Invalid:

show friendly error.

Do not send knowingly invalid query.

---

# 66. LOADING UX

All pages need:

```text
loading
empty
error
retry
```

Use:

```text
QSkeleton
QInnerLoading
QSpinner
```

appropriately.

Do not block whole app shell unnecessarily.

---

# 67. EMPTY STATES

Examples:

Customers:

```text
Không tìm thấy khách hàng phù hợp.
```

Transactions:

```text
Không tìm thấy giao dịch.
```

Payments:

```text
Không tìm thấy thanh toán.
```

Users:

```text
Không tìm thấy người dùng.
```

Audit:

```text
Không tìm thấy nhật ký phù hợp.
```

---

# 68. ERROR HANDLING

Reuse FE1 ProblemDetails parsing.

Map:

```text
401 → auth refresh/session handling
403 → permission
404 → resource not found
409 → state conflict
429 → too many requests
500/network → friendly server error
```

Never show stack trace.

---

# 69. 403 UX

If STAFF reaches ADMIN-only page:

Display/notify:

```text
Bạn không có quyền truy cập chức năng này.
```

Redirect `/admin`.

Do not redirect STAFF to customer `/dashboard`.

---

# 70. 429 UX

Backend M7 has rate limiting.

When API returns:

```text
429 Too Many Requests
```

Display:

```text
Bạn đang thao tác quá nhanh. Vui lòng thử lại sau.
```

Do not auto-spam retries.

---

# 71. DASHBOARD REFRESH

Provide manual refresh button if useful.

Do not poll aggressively.

Admin dashboard data can refresh:

```text
on page load
manual refresh
```

No WebSocket required.

---

# 72. ADMIN KPI FINANCIAL FORMAT

Use currency utility.

Example:

```text
2.400.000.000 ₫
185.000.000 ₫
```

Do not use float arithmetic for business calculations.

Frontend only formats Backend totals.

---

# 73. ADMIN STATUS COMPONENT

Recommended reusable component:

```text
components/admin/StatusBadge.vue
```

or a generalized shared badge if existing.

Avoid duplicating status color mappings across many pages.

---

# 74. ADMIN PAGE HEADER COMPONENT

Optional reusable:

```text
components/admin/AdminPageHeader.vue
```

Can provide:

```text
title
description
actions
```

Only create if it actually removes repetition.

---

# 75. CONFIRM ACTION COMPONENT

Optional reusable:

```text
components/common/ConfirmDialog.vue
```

for:

```text
Suspend customer
Activate customer
Lock account
Unlock account
Suspend user
Activate user
```

Reuse Quasar dialog.

Do not build unnecessary abstraction if existing pattern works.

---

# 76. ADMIN TABLE DESIGN

Desktop tables should prioritize:

```text
readability
operational scanning
filters
pagination
```

Avoid overly decorative banking cards for large admin datasets.

Admin UI can be more data-dense than Customer UI.

---

# 77. MOBILE ADMIN UX

Admin mobile is secondary but must remain usable.

On narrow screens:

Use cards/list or responsive columns.

Do not attempt to display 10-column table at 375px.

---

# 78. ACCESSIBILITY

Buttons need readable labels/tooltips.

Status cannot be communicated only by color.

Confirmation dialogs keyboard usable.

Inputs have labels.

---

# 79. SECURITY — FRONTEND

Never expose/log:

```text
AccessToken
RefreshToken
JWT Secret
DB password
PasswordHash
RefreshTokenHash
Authorization header
```

No security secrets in Nuxt public runtime config.

---

# 80. NO FAKE ADMIN FEATURES

Do NOT implement UI for features Backend does not support:

```text
Create Admin
Delete User
Edit Balance
Delete Transaction
Delete Payment
Delete Audit Log
Modify financial ledger
Reset Password
Change Role
```

unless actual Backend API exists.

---

# 81. NO FINANCIAL MUTATION

Staff/Admin FE must never provide direct:

```text
balance editor
transaction editor
payment editor
```

Financial records remain operational read-only.

Only supported status management can mutate state.

---

# 82. API VERIFICATION SCRIPT

Create:

```text
scripts/verify-fe5-api.ps1
```

No Playwright.

Script should verify Staff/Admin contracts used by frontend.

---

# 83. API TEST — STAFF LOGIN

Login:

```text
staff@locallink.local
LocalLink@123
```

Capture JWT.

Verify:

```text
GET /api/v1/admin/dashboard                → 200
GET /api/v1/admin/customers                → 200
GET /api/v1/admin/transactions             → 200
GET /api/v1/admin/payments                 → 200
```

Verify ADMIN-only:

```text
GET /api/v1/admin/users                    → 403
GET /api/v1/admin/audit-logs               → 403
```

---

# 84. API TEST — ADMIN LOGIN

Login:

```text
admin@locallink.local
LocalLink@123
```

Capture JWT.

Verify:

```text
GET /api/v1/admin/dashboard                → 200
GET /api/v1/admin/customers                → 200
GET /api/v1/admin/transactions             → 200
GET /api/v1/admin/payments                 → 200
GET /api/v1/admin/users                    → 200
GET /api/v1/admin/audit-logs               → 200
```

---

# 85. API TEST — DASHBOARD

Assert response exists and contains valid non-negative metrics.

Do not hardcode counts unless test runs on explicitly clean seeded database.

Examples of invariants:

```text
customers.total >= 0
accounts.total >= 0
totalBalance >= 0
transferVolume >= 0
paymentVolume >= 0
```

Use actual DTO.

---

# 86. API TEST — CUSTOMER SEARCH

Call:

```http
GET /api/v1/admin/customers?page=1&pageSize=10
```

Then search known demo customer if supported:

```text
Nguyen
Tran
CUS000001
```

Assert valid result.

---

# 87. API TEST — TRANSACTION SEARCH

Call global transactions endpoint.

If data exists:

Verify response shape/pagination.

Do not require transaction count > 0 unless test setup guarantees it.

---

# 88. API TEST — PAYMENT SEARCH

Same approach:

Validate:

```text
HTTP success
pagination
response fields
```

Use seeded/payment data if available.

---

# 89. API TEST — ADMIN USER ACCESS

Admin:

```text
GET users → 200
```

Find non-admin demo user.

Do not choose currently logged-in Admin for suspension test.

---

# 90. API TEST — USER SUSPENSION

ADMIN only.

Choose safe demo user such as Customer 2 if appropriate.

Record original status.

Suspend:

```text
PATCH /api/v1/admin/users/{id}/status
```

Verify:

```text
status = SUSPENDED
```

Attempt login as target user:

Expected rejection.

Reactivate afterward.

Restore original test state.

Do not leave seed demo account suspended after script finishes.

---

# 91. API TEST — SELF SUSPENSION

Using Admin JWT:

Attempt to suspend current Admin.

Expected:

```text
400/appropriate business rejection
```

Assert Admin remains ACTIVE.

---

# 92. API TEST — CUSTOMER STATUS

If safe:

Admin toggles Customer demo:

```text
ACTIVE → SUSPENDED → ACTIVE
```

Assert.

Restore state.

If it could interfere with user suspension test, order carefully.

---

# 93. API TEST — ACCOUNT STATUS

If safe:

Choose demo account.

```text
ACTIVE → LOCKED → ACTIVE
```

Assert.

Restore state.

Do not leave account locked.

---

# 94. API TEST — AUDIT

After administrative actions:

```text
GET audit logs
```

Verify relevant action exists if Backend logs it.

Examples:

```text
USER_SUSPEND
USER_ACTIVATE
CUSTOMER_SUSPEND
CUSTOMER_ACTIVATE
ACCOUNT_LOCK
ACCOUNT_UNLOCK
```

Use actual action strings.

---

# 95. API SCRIPT CLEANUP

Use:

```text
try/finally
```

or equivalent PowerShell handling.

If script modifies:

```text
user status
customer status
account status
```

it should attempt to restore original states even if later assertion fails.

Do not leave development database broken.

---

# 96. API SCRIPT RESULT

Script:

```text
scripts/verify-fe5-api.ps1
```

must:

```text
print clear PASS/FAIL
exit 0 on success
exit non-zero on failure
```

Do not log JWT values.

---

# 97. FRONTEND BUILD

Run:

```bash
cd frontend
npm run build
```

Must:

```text
PASS
0 TypeScript build errors
```

---

# 98. BACKEND REGRESSION

Run:

```bash
dotnet test backend/LocalLink.sln
```

All current Backend tests must pass.

Do not rely on exact historical test count.

Report actual number.

Do not delete tests to make suite pass.

---

# 99. DOCKER

Run:

```bash
docker compose up --build -d
```

Verify:

```text
locallink-web         Running
locallink-api         Healthy
locallink-sqlserver   Healthy
```

Do not run:

```bash
docker compose down -v
```

unless necessary.

Preserve current development data.

---

# 100. MANUAL UI SMOKE ROUTES

No automated browser.

Provide/check manually if required:

```text
http://localhost:3000/admin

http://localhost:3000/admin/customers

http://localhost:3000/admin/transactions

http://localhost:3000/admin/payments

http://localhost:3000/admin/users

http://localhost:3000/admin/audit-logs
```

Verify basic page rendering only.

No Playwright.

---

# 101. STAFF UI SMOKE

Manual if required:

Login Staff.

Verify:

```text
Dashboard visible
Customers visible
Transactions visible
Payments visible

Users hidden
Audit Logs hidden
```

Do not automate browser.

---

# 102. ADMIN UI SMOKE

Login Admin manually if required.

Verify:

```text
All menu items visible
Status actions available
Users visible
Audit visible
```

No browser automation required.

---

# 103. DOCUMENTATION

Create/update:

```text
Docs/frontend/admin-dashboard.md
Docs/frontend/admin-customers.md
Docs/frontend/admin-transactions.md
Docs/frontend/admin-payments.md
Docs/frontend/admin-users.md
Docs/frontend/admin-audit.md
```

Document:

```text
Routes
Roles
Services
API endpoints
Filters
Pagination
Mutations
Confirmation behavior
Security behavior
Responsive behavior
```

---

# 104. UPDATE FRONTEND ROADMAP

After success:

```text
FE1 — Auth + App Shell                    ✅
FE2 — Dashboard + Accounts + Profile      ✅
FE3 — Transfer + Transactions             ✅
FE4 — Bills + Payments + Notifications    ✅
FE5 — Staff/Admin UI                      ✅
FE6 — Final Integration & Polish          TODO
```

---

# 105. CODE QUALITY REVIEW

Before finishing:

Review for:

```text
duplicate types
duplicate services
duplicate formatter logic
unused imports
unused components
console.log
debug code
hardcoded API URLs
hardcoded demo data in actual UI
```

Remove obvious leftovers.

Do not perform huge architecture refactor.

---

# 106. FRONTEND CONTRACT CONSISTENCY

Ensure FE5 types/services match actual Backend contract.

No `any` unless unavoidable.

No assumed DTO properties.

TypeScript strict build must pass.

---

# 107. DESIGN CONSISTENCY

Keep:

```text
InterLink Banking product branding
Premium dark UI
Quasar
same typography
same spacing
same status semantics
```

Admin should look like same product, but operational/data-heavy.

---

# 108. KNOWN LIMITATIONS

Do not treat missing features as bugs if Backend intentionally lacks them.

Examples acceptable for V1:

```text
No custom role editor
No password reset admin flow
No transaction deletion
No payment reversal
No audit export
No realtime dashboard
```

Document real limitations.

---

# 109. DEFINITION OF DONE

FE5 is COMPLETE only when:

```text
[ ] Admin dashboard implemented
[ ] Dashboard metrics consume actual Backend

[ ] Customer list implemented
[ ] Customer search/filter/pagination works
[ ] Customer detail implemented
[ ] Customer accounts visible

[ ] STAFF customer view is read-only
[ ] ADMIN customer status actions work
[ ] ADMIN account lock/unlock works
[ ] Confirmation dialogs implemented

[ ] Global transaction list implemented
[ ] Transaction filters work
[ ] Transaction pagination works
[ ] Transaction detail implemented

[ ] Global payment list implemented
[ ] Payment filters work
[ ] Payment pagination works
[ ] Payment detail implemented

[ ] Users page implemented
[ ] Users page ADMIN only
[ ] User detail implemented
[ ] User suspend/activate works
[ ] Admin self-suspension prevented in UI

[ ] Audit Logs implemented
[ ] Audit Logs ADMIN only
[ ] Audit logs read-only

[ ] STAFF navigation correct
[ ] ADMIN navigation correct
[ ] CUSTOMER blocked from admin routes

[ ] 403 UX implemented
[ ] 429 UX implemented

[ ] Desktop responsive
[ ] Tablet responsive
[ ] Mobile usable

[ ] verify-fe5-api.ps1 created
[ ] Staff API verification PASS
[ ] Admin API verification PASS
[ ] RBAC verification PASS
[ ] Status mutation verification PASS
[ ] State restored after API verification

[ ] npm run build PASS
[ ] dotnet test PASS
[ ] Docker healthy

[ ] Playwright NOT USED

[ ] Documentation complete
[ ] Git commit complete
[ ] Branch pushed
```

---

# 110. GIT

After all verification:

```bash
git status
```

Ensure:

```text
.env not tracked
no JWT
no access token
no refresh token
no temporary API output
no test secrets beyond approved development configuration
```

Commit:

```bash
git add .
git commit -m "feat: implement staff and admin frontend"
```

Push:

```bash
git push -u origin feature/frontend-admin
```

Follow current integration workflow to `feature/hoi`.

Do not merge `main` automatically.

---

# 111. FINAL REPORT

Return:

```text
LOCALINK M8 — FE5 STAFF/ADMIN REPORT

Branch:

Routes Implemented:

Admin Layout:

Role Navigation:

STAFF Permissions:

ADMIN Permissions:

Admin Dashboard:

Dashboard Metrics:

Customers Page:

Customer Search:

Customer Detail:

Customer Accounts:

Customer Status Management:

Account Status Management:

Transactions Page:

Transaction Filters:

Transaction Detail:

Payments Page:

Payment Filters:

Payment Detail:

Users Page:

User Detail:

User Status Management:

Self-Suspension Protection:

Audit Logs:

Audit Detail:

Audit Read-Only:

Pagination:

Loading UX:

Empty States:

403 Handling:

429 Handling:

Responsive Desktop:

Responsive Tablet:

Responsive Mobile:

API Verification Script:

Staff API Verification:

Admin API Verification:

RBAC Verification:

Status Mutation Verification:

Audit Verification:

Database State Restoration:

Frontend Build:

Backend Tests:

Docker:

Playwright:
NOT USED

Documentation:

Files Created/Modified:

Commit:

Push:

Warnings / Known Limitations:

FE5 STATUS:
COMPLETE / NOT COMPLETE
```

Then STOP.

Do NOT begin FE6 automatically.
