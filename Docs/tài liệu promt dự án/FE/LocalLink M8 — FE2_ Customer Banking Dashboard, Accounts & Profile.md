# LocalLink M8 — FE2: Customer Banking Dashboard, Accounts & Profile

## 0. CURRENT PROJECT STATE

Project hiện đã hoàn thành:

```text
BACKEND

M1 Foundation & DevOps ✅
M2 Database Foundation ✅
M3 Authentication + JWT + RBAC ✅
M4 Customer + Bank Accounts ✅
M5 Transfer Engine ✅
M6 Bill Payment + Notifications ✅
M7 Staff/Admin + Backend V1 ✅

FRONTEND

FE1 Authentication + App Shell + RBAC ✅
FE2 Customer Dashboard ← BẮT ĐẦU
```

Frontend hiện tại:

```text
Nuxt 4 SPA
Vue 3
TypeScript
Quasar
Pinia
$fetch
```

Authentication hiện đã hoạt động:

```text
Login
JWT Access Token
Refresh Token
Session Restore
Current User
Route Middleware
CUSTOMER / STAFF / ADMIN RBAC
Customer Layout
Admin Layout
```

KHÔNG redesign authentication.

KHÔNG thay đổi token strategy ở milestone này.

KHÔNG implement Transfer form.

---

# 1. MỤC TIÊU FE2

Xây dựng Customer Banking experience:

```text
Login
 ↓
Dashboard
 ↓
Accounts
 ↓
Account Detail
 ↓
Profile
```

Dashboard phải lấy dữ liệu thật từ Backend API.

Không hardcode banking data.

Không dùng mock JSON nếu API đã tồn tại.

---

# 2. FEATURE BRANCH

Theo workflow hiện tại:

```bash
git checkout feature/hoi
git pull origin feature/hoi
git checkout -b feature/frontend-dashboard
```

Nếu FE1 chưa merge vào `feature/hoi`, bắt đầu từ branch chứa FE1 mới nhất.

Không code trên main.

---

# 3. READ API CONTRACT FIRST

Trước khi code, đọc:

```text
Docs/api/frontend-contract.md
Docs/api/transfers.md
Docs/api/transactions.md
Docs/api/bills.md
Docs/api/notifications.md
```

và documentation Customer/Accounts hiện có.

Không đoán DTO.

Inspect actual Swagger nếu cần.

Reuse backend endpoints hiện có.

---

# 4. CUSTOMER ENDPOINTS

FE2 sử dụng tối thiểu:

```http
GET /api/v1/customers/me

GET /api/v1/accounts
GET /api/v1/accounts/{id}

GET /api/v1/transactions

GET /api/v1/bills

GET /api/v1/notifications/unread-count
```

Nếu contract thực tế khác, follow contract thực tế.

---

# 5. FRONTEND SERVICES

Tạo/reuse:

```text
services/customer.ts
services/accounts.ts
services/transactions.ts
services/bills.ts
services/notifications.ts
```

Không gọi `$fetch` trực tiếp khắp các Vue page nếu shared API client đã tồn tại.

Reuse:

```text
services/api.ts
```

từ FE1 để:

```text
Authorization header
401 refresh
ProblemDetails handling
base URL
```

---

# 6. TYPES

Tạo/reuse:

```text
types/customer.ts
types/account.ts
types/transaction.ts
types/bill.ts
types/notification.ts
```

Các type phải khớp Backend Contract.

Không duplicate cùng một interface ở nhiều file.

---

# 7. DASHBOARD PAGE

Replace placeholder:

```text
/pages/dashboard.vue
```

bằng Customer Banking Dashboard thật.

Require:

```text
customer middleware
```

và Customer Layout.

---

# 8. DASHBOARD HEADER

Hiển thị:

```text
Xin chào, Nguyen Van An
```

và subtitle:

```text
Tổng quan tài chính của bạn
```

Dữ liệu FullName lấy từ:

```http
GET /api/v1/customers/me
```

Không hardcode tên.

---

# 9. TOTAL BALANCE

Dashboard phải tính:

```text
Total Balance
=
SUM(account.balance)
```

từ accounts của current customer.

Ví dụ:

```text
25,000,000 VND
```

Dùng JavaScript number/formatting chỉ để display.

Không thay đổi financial calculation backend.

Không gửi balance mutation.

---

# 10. VND FORMATTER

Tạo utility:

```text
utils/currency.ts
```

Function concept:

```text
formatCurrency(amount, "VND")
```

Output:

```text
25.000.000 ₫
```

hoặc:

```text
25,000,000 VND
```

Chọn một style nhất quán.

Ưu tiên:

```ts
Intl.NumberFormat("vi-VN", {
  style: "currency",
  currency: "VND"
})
```

Nếu browser output không đẹp, có thể dùng formatter phù hợp.

---

# 11. ACCOUNT CARDS

Dashboard hiển thị account cards.

Mỗi card:

```text
Account Name
Account Number
Account Type
Balance
Currency
Status
```

Ví dụ:

```text
Tài khoản thanh toán

1000000001

24.150.000 ₫

ACTIVE
```

---

# 12. MASK ACCOUNT NUMBER

Trên dashboard có thể hiển thị:

```text
•••• •••• 0001
```

hoặc full account number nếu design hiện tại ưu tiên demo.

Account detail page phải hiển thị full number.

Nếu implement masking:

Tạo reusable utility/component.

---

# 13. ACCOUNT STATUS VISUAL

Status:

```text
ACTIVE
LOCKED
CLOSED
```

Render bằng QBadge/QChip.

Không cho customer thay đổi status.

---

# 14. QUICK ACTIONS

Dashboard có:

```text
Chuyển tiền
Thanh toán hóa đơn
Lịch sử giao dịch
```

Buttons:

```text
Chuyển tiền → /transfer
Thanh toán → /bills
Giao dịch → /transactions
```

Các pages chưa implement có thể tồn tại placeholder hoặc được tạo trong milestone tương ứng.

Không implement transfer form trong FE2.

---

# 15. RECENT TRANSACTIONS

Dashboard lấy:

```http
GET /api/v1/transactions?page=1&pageSize=5
```

hoặc query tương đương theo backend contract.

Hiển thị tối đa khoảng:

```text
5 recent transactions
```

Newest first.

---

# 16. TRANSACTION ITEM

Mỗi item hiển thị:

```text
Type
Description / Counterparty
Amount
Timestamp
Status
```

Ví dụ:

```text
TRANSFER

Tran Thi Binh

-500.000 ₫

COMPLETED
```

Payment:

```text
PAYMENT

Da Nang Electricity

-850.000 ₫
```

---

# 17. INCOMING VS OUTGOING

Nếu transaction DTO có đủ source/destination information:

Xác định incoming/outgoing dựa trên account ownership.

Display:

```text
Outgoing → negative visual
Incoming → positive visual
```

Nếu backend contract không đủ thông tin:

Không đoán.

Display neutral amount và transaction type.

Document limitation.

---

# 18. RECENT TRANSACTION EMPTY STATE

Nếu chưa có transaction:

Hiển thị:

```text
Chưa có giao dịch nào.
```

Không hiển thị blank card.

---

# 19. BILL SUMMARY

Dashboard gọi:

```http
GET /api/v1/bills?page=1&pageSize=...
```

hoặc filter:

```text
status=UNPAID
```

nếu backend contract hỗ trợ.

Hiển thị:

```text
Hóa đơn chưa thanh toán
```

Count:

```text
2
```

và tổng amount nếu có thể tính an toàn từ response hiện tại.

---

# 20. BILL CARD

Hiển thị tối đa 2–3 bills:

```text
Da Nang Electricity
850.000 ₫
Due: 30/08/2026
UNPAID
```

Button:

```text
Xem tất cả
→ /bills
```

Không implement payment action tại dashboard nếu FE4 chưa làm.

---

# 21. NOTIFICATION BADGE

Customer layout hiện có Notification shortcut.

Connect:

```http
GET /api/v1/notifications/unread-count
```

Hiển thị unread badge.

Ví dụ:

```text
🔔 3
```

Không polling quá thường xuyên.

Fetch khi:

```text
layout/dashboard initializes
```

Có thể refresh khi route changes nếu đơn giản.

Realtime websocket không cần.

---

# 22. DASHBOARD DATA LOADING

Không sequentially chờ từng API nếu không cần.

Có thể sử dụng:

```ts
Promise.all(...)
```

cho:

```text
customer
accounts
recent transactions
bills
unread notification count
```

nhưng handle individual failures hợp lý.

Không để một Bill API failure làm toàn dashboard trắng nếu Accounts vẫn hoạt động.

---

# 23. DASHBOARD LOADING STATE

Khi load:

Use:

```text
QSkeleton
```

cho:

```text
balance card
account card
recent transactions
bills
```

Không chỉ hiển thị spinner full screen nếu có thể tránh.

---

# 24. PARTIAL ERROR STATE

Ví dụ Accounts fail:

Hiển thị:

```text
Không thể tải thông tin tài khoản.
```

Có button:

```text
Thử lại
```

Không crash toàn page.

---

# 25. ACCOUNTS PAGE

Tạo:

```text
/pages/accounts/index.vue
```

CUSTOMER only.

Hiển thị tất cả account của current customer.

---

# 26. ACCOUNT LIST

Mỗi account card:

```text
Account Name
Account Number
Account Type
Balance
Currency
Status
```

Click:

```text
/accounts/{id}
```

---

# 27. ACCOUNT DETAIL PAGE

Tạo:

```text
/pages/accounts/[id].vue
```

Use:

```http
GET /api/v1/accounts/{id}
```

CUSTOMER only.

Backend ownership already authoritative.

---

# 28. ACCOUNT DETAIL

Hiển thị:

```text
Account Name
Account Number
Account Type
Available Balance
Currency
Status
Created Date nếu API trả
```

Không hiển thị:

```text
RowVersion
CustomerId
Internal fields
```

---

# 29. ACCOUNT DETAIL QUICK ACTIONS

Buttons:

```text
Chuyển tiền
Xem giao dịch
```

Routes:

```text
/transfer?sourceAccount=<id>

/transactions?accountId=<id>
```

Nếu destination page chưa implement:

route/link vẫn có thể chuẩn bị.

---

# 30. BALANCE PRIVACY TOGGLE

Optional nhưng rất hợp banking UI.

Add:

```text
👁 Show / Hide balance
```

Có thể dùng local component state.

Không cần persist.

Nếu làm, create reusable:

```text
BalanceDisplay.vue
```

---

# 31. PROFILE PAGE

Replace placeholder:

```text
/pages/profile.vue
```

Use:

```http
GET /api/v1/customers/me
```

Hiển thị:

```text
Customer Code
Full Name
Date of Birth
Gender
Phone
Address
Status
Email nếu auth/current user có
```

---

# 32. PROFILE EDIT

Use backend existing:

```http
PUT /api/v1/customers/me
```

hoặc actual contract.

Allow update only:

```text
FullName
DateOfBirth
Gender
PhoneNumber
Address
```

Do not provide inputs for:

```text
CustomerCode
Status
UserId
```

---

# 33. PROFILE FORM

Use Quasar:

```text
QInput
QSelect
QDate or native date input
QBtn
```

Have:

```text
Edit
Save
Cancel
```

Do not always leave profile in edit state.

---

# 34. PROFILE VALIDATION

Validate client-side according to backend rules.

But backend remains authoritative.

Display ProblemDetails cleanly.

---

# 35. PROFILE SUCCESS UX

After successful update:

```text
Cập nhật thông tin thành công.
```

Use:

```text
QNotify
```

or project's existing alert pattern.

Do not invent another notification framework.

---

# 36. CUSTOMER STORE — OPTIONAL

Do not automatically create huge stores for all domain data.

If dashboard data benefits from shared state, you may create:

```text
stores/customer.ts
```

or:

```text
stores/banking.ts
```

only if it reduces duplication.

Otherwise composables/services are sufficient.

Avoid overusing Pinia.

---

# 37. DASHBOARD COMPOSABLE

Recommended:

```text
composables/useDashboard.ts
```

Could coordinate:

```text
customer
accounts
recentTransactions
bills
unreadCount
loading
error
refresh()
```

Keep page component readable.

---

# 38. ACCOUNT COMPOSABLE

Optional:

```text
composables/useAccounts.ts
```

if useful.

Do not create abstractions only to increase file count.

---

# 39. DATE FORMATTER

Create/reuse:

```text
utils/date.ts
```

Display user-facing Vietnamese date/time.

Examples:

```text
18/08/2026
18/08/2026 21:15
```

Backend UTC timestamps must be interpreted correctly.

---

# 40. RESPONSIVE DASHBOARD

Desktop:

```text
Summary cards
Account cards
Recent transactions
Bills
```

Mobile:

Stack vertically.

No horizontal overflow.

Account cards must remain readable on ~375px width.

---

# 41. CUSTOMER LAYOUT IMPROVEMENT

Update existing:

```text
layouts/default.vue
```

to support real pages.

Navigation:

```text
Tổng quan
Tài khoản
Chuyển tiền
Giao dịch
Hóa đơn
Thông báo
Hồ sơ
```

Active route visually highlighted.

---

# 42. HEADER

Header can show:

```text
InterLink Banking
Customer name
Notification badge
Profile menu
Logout
```

Do not overcrowd.

---

# 43. BRANDING CONSISTENCY

Visible UI currently uses:

```text
InterLink Banking
```

Keep this branding consistently in FE2.

Do NOT rename backend solution/classes from LocalLink.

Treat:

```text
LocalLink = project/repository name
InterLink Banking = product-facing UI brand
```

unless repository already decided otherwise.

Document this convention if needed.

---

# 44. UI DESIGN DIRECTION

Aim for:

```text
Premium
Modern
Minimal
Banking
Trustworthy
```

Use existing dark theme.

Avoid:

```text
too many gradients
neon-heavy effects
gaming UI
excessive animation
```

Visual polish matters because this is portfolio-facing.

---

# 45. CARD HIERARCHY

Dashboard visual priority:

```text
1. Total balance
2. Bank accounts
3. Quick actions
4. Recent transactions
5. Bills
```

Do not make system telemetry more prominent than banking data anymore.

---

# 46. REMOVE/DEMOTE OLD TELEMETRY PAGE

Existing original homepage/dashboard may contain:

```text
Backend Connected
Database Healthy
Web Operational
```

These are development telemetry.

Do NOT make them the main banking experience.

Move/de-emphasize them.

They may remain at:

```text
/
```

as landing/development page if useful.

Authenticated `/dashboard` should be banking-focused.

---

# 47. HOME PAGE

Existing:

```text
/
```

can remain public landing page.

If authenticated:

Provide:

```text
Vào ứng dụng
```

Customer → dashboard.

Staff/Admin → admin.

Do not duplicate banking dashboard at `/`.

---

# 48. TRANSACTION ROUTE PLACEHOLDER

If `/transactions` does not yet exist:

Create a minimal placeholder only if required to avoid broken navigation.

Full implementation belongs FE3.

Same for:

```text
/transfer
/bills
/notifications
```

Do not accidentally implement FE3/FE4.

---

# 49. TYPESCRIPT STRICT

No unnecessary:

```text
any
```

Use backend types.

`npm run build` must pass TypeScript.

---

# 50. MONEY SAFETY FRONTEND

Frontend displays amount.

Frontend must NEVER:

```text
calculate new account balance and submit it
send balance values
override payment amount
```

Backend remains source of truth.

---

# 51. ACCOUNT SECURITY

CUSTOMER UI never offers:

```text
Lock Account
Unlock Account
Modify Balance
Change Customer ID
```

These are not customer capabilities.

---

# 52. API ERROR HANDLING

Reuse FE1 ProblemDetails handling.

Examples:

```text
401 → session handling
403 → permission error
404 account → not found
network → retry state
```

Do not expose raw backend error objects.

---

# 53. AUTH REGRESSION

Verify FE1 still works:

```text
Customer login
Staff login
Admin login
F5 restore session
Logout
Route guards
Token refresh
```

Do not break auth while creating dashboard.

---

# 54. CUSTOMER DASHBOARD E2E

Use clean seeded DB if needed.

Login:

```text
customer1@locallink.local
LocalLink@123
```

Expected dashboard:

```text
Nguyen Van An

Account:
1000000001

Balance:
25,000,000 VND
```

If current persistent DB contains previous live transactions/payments, expected balance may differ.

Do NOT hardcode test to 25M unless database was reset.

Prefer verification against API result.

---

# 55. ACCOUNT LIST E2E

Navigate:

```text
/accounts
```

Expected:

Only Customer 1 accounts visible.

Customer 2 account must not appear.

---

# 56. ACCOUNT DETAIL E2E

Open Customer 1 account.

Expected:

```text
200
correct balance
correct account info
```

Attempt manually using another customer's account ID if available.

Backend should return not found/denied.

Frontend must display safe error.

---

# 57. PROFILE E2E

Open:

```text
/profile
```

Edit phone/address.

Save.

Reload page.

Expected updated values persist from SQL Server.

Restore seed value after test if desired.

---

# 58. DASHBOARD TRANSACTIONS E2E

If clean DB has no transactions:

Verify empty state.

If M5 data exists:

Recent transfer displays correctly.

Do not seed fake frontend transactions.

---

# 59. DASHBOARD BILL E2E

Seeded bills should display real backend bills.

Example before payment:

```text
ELEC-2026-0001
Da Nang Electricity
850,000 VND
```

No hardcoded frontend bills.

---

# 60. NOTIFICATION BADGE E2E

If unread notifications exist:

Badge > 0.

If none:

Badge hidden or `0` according to chosen UX.

No error.

---

# 61. BUILD TEST

Run:

```bash
cd frontend
npm run build
```

Must pass.

---

# 62. BACKEND REGRESSION

Run:

```bash
dotnet test backend/LocalLink.sln
```

Expected existing backend suite still passes.

Baseline currently approximately:

```text
64 tests
```

Do not delete backend tests.

---

# 63. DOCKER

Run:

```bash
docker compose up --build -d
```

Verify:

```text
locallink-web         Healthy / Running
locallink-api         Healthy
locallink-sqlserver   Healthy
```

---

# 64. BROWSER RESPONSIVE VERIFICATION

Verify at least:

```text
Desktop ~1440px
Tablet ~768px
Mobile ~375px
```

No horizontal overflow.

Drawer works.

Cards stack correctly.

---

# 65. LOADING VERIFICATION

Throttle/network if convenient.

Confirm:

```text
skeleton appears
layout remains stable
```

No ugly blank page.

---

# 66. FAILURE VERIFICATION

Temporarily stop API if feasible.

Dashboard should show friendly error.

Frontend must not crash.

Restart API afterward.

---

# 67. SCREEN QUALITY

Before completing FE2, visually review:

```text
spacing
typography
currency formatting
card consistency
button hierarchy
status colors
mobile responsiveness
dark theme contrast
```

Fix obvious visual defects.

---

# 68. DOCUMENTATION

Create/update:

```text
Docs/frontend/dashboard.md
Docs/frontend/accounts.md
Docs/frontend/profile.md
```

Document:

```text
Routes
API dependencies
Types
Components
Loading/error states
Role requirement
```

---

# 69. FRONTEND ROADMAP

After success:

```text
M8 FRONTEND

FE1 Authentication + App Shell         ✅
FE2 Dashboard + Accounts + Profile     ✅
FE3 Transfer + Transactions            TODO
FE4 Bills + Payments + Notifications   TODO
FE5 Staff/Admin UI                     TODO
```

---

# 70. DO NOT START FE3

Do NOT implement full:

```text
Transfer form
Transfer confirmation
Transfer receipt
Transaction filter page
Beneficiary management UI
```

Those belong to FE3.

---

# 71. GIT

After verification:

```bash
git status
```

Ensure no secrets.

Commit:

```bash
git add .
git commit -m "feat: implement customer banking dashboard"
```

Push:

```bash
git push -u origin feature/frontend-dashboard
```

Follow repository integration workflow.

Do not automatically merge main.

---

# 72. DEFINITION OF DONE

FE2 complete only if:

```text
[ ] Dashboard loads real customer data

[ ] Customer greeting works

[ ] Total balance calculated from current customer's accounts

[ ] Account cards work

[ ] Account status displayed

[ ] Quick actions work

[ ] Recent transactions consume real API

[ ] Transaction empty state works

[ ] Bill summary consumes real API

[ ] Notification unread badge works

[ ] Accounts page works

[ ] Account detail works

[ ] Account ownership remains enforced

[ ] Profile page works

[ ] Profile update works

[ ] Protected fields cannot be modified

[ ] Currency formatting consistent

[ ] Date formatting consistent

[ ] Loading skeletons implemented

[ ] Friendly errors implemented

[ ] Customer layout responsive

[ ] Mobile layout works

[ ] No hardcoded banking data

[ ] No customer balance mutation API

[ ] FE1 auth regression passes

[ ] npm build passes

[ ] backend tests pass

[ ] Docker stack works

[ ] Documentation updated

[ ] feature branch pushed
```

---

# 73. FINAL REPORT

Return:

```text
LOCALINK M8 — FE2 CUSTOMER DASHBOARD REPORT

Branch:

Dashboard:

Customer API:

Accounts API:

Transactions API:

Bills API:

Notification API:

Total Balance:

Account Cards:

Recent Transactions:

Bill Summary:

Notification Badge:

Accounts Page:

Account Detail:

Profile Page:

Profile Update:

Ownership Handling:

Currency Formatting:

Date Formatting:

Loading UX:

Error UX:

Responsive Desktop:

Responsive Tablet:

Responsive Mobile:

Auth Regression:

Frontend Build:

Backend Tests:

Docker:

Files Created:

Documentation:

Commit:

Push:

Screens / Routes Implemented:

Warnings / Known Limitations:

FE2 STATUS:
COMPLETE / NOT COMPLETE
```

Then STOP.

Do not begin FE3 automatically.