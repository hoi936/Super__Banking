# LocalLink M8 — FE3: Transfer & Transaction UI

## 0. CURRENT STATUS

Project đã hoàn thành:

```text
BACKEND V1 ✅

M1 Foundation
M2 Database
M3 Authentication
M4 Customer + Accounts
M5 Transfer Engine
M6 Bills + Payments + Notifications
M7 Staff/Admin + Backend Finalization

FRONTEND

FE1 Authentication + App Shell ✅
FE2 Dashboard + Accounts + Profile ✅
FE3 Transfer + Transactions ← BẮT ĐẦU
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

Backend transfer engine hiện đã hỗ trợ:

```text
POST /api/v1/transfers
GET  /api/v1/transfers
GET  /api/v1/transfers/{id}

GET  /api/v1/transactions
GET  /api/v1/transactions/{id}

GET  /api/v1/accounts
GET  /api/v1/accounts/lookup/{accountNumber}

GET    /api/v1/beneficiaries
POST   /api/v1/beneficiaries
DELETE /api/v1/beneficiaries/{id}
```

Không implement External Bank Transfer.

Chỉ làm transfer nội bộ LocalLink/InterLink.

---

# 1. TESTING RULE — IMPORTANT

KHÔNG sử dụng Playwright/browser automation cho verification mặc định.

Ưu tiên:

```text
1. npm run build
2. Backend unit tests
3. Docker build
4. API E2E bằng PowerShell/curl
5. Manual UI smoke check tối thiểu nếu cần
```

Không generate hàng loạt screenshots.

Không chạy browser automation tốn token nếu API verification đã chứng minh được chức năng.

---

# 2. FEATURE BRANCH

```bash
git checkout feature/hoi
git pull origin feature/hoi
git checkout -b feature/frontend-transfer
```

Nếu FE2 chưa merge vào feature/hoi thì checkout branch FE2 mới nhất.

---

# 3. READ CONTRACT FIRST

Đọc:

```text
Docs/api/frontend-contract.md
Docs/api/transfers.md
Docs/api/transactions.md
```

và current Swagger.

Không đoán DTO.

Không đổi backend contract chỉ để frontend dễ code.

---

# 4. FE3 OBJECTIVE

Xây flow:

```text
Source Account
↓
Destination Account
↓
Beneficiary
↓
Amount
↓
Description
↓
Validation
↓
Confirmation
↓
POST Transfer
↓
Receipt
↓
Transaction History
```

---

# 5. ROUTES

Implement:

```text
/transfer
/transactions
/transactions/{id}
```

Optional:

```text
/transfers/{id}
```

nếu receipt/detail route cần.

CUSTOMER only.

---

# 6. TYPES

Create/reuse:

```text
types/transfer.ts
```

Include:

```text
CreateTransferRequest
TransferReceipt
TransferListItem
TransferDetail
```

Reuse existing:

```text
Transaction types
Account types
Beneficiary types
PagedResult<T>
ProblemDetails
```

---

# 7. TRANSFER SERVICE

Create:

```text
services/transferService.ts
```

Functions:

```text
createTransfer(...)
getTransfers(...)
getTransfer(id)
```

Use shared `services/api.ts`.

No direct raw `$fetch` duplicated inside pages.

---

# 8. BENEFICIARY SERVICE

Reuse existing beneficiary APIs.

If no service exists, create:

```text
services/beneficiaryService.ts
```

Functions:

```text
getBeneficiaries()
addBeneficiary()
deleteBeneficiary()
```

Do not implement new backend API.

---

# 9. TRANSFER PAGE

Implement:

```text
pages/transfer.vue
```

CUSTOMER middleware.

Use Customer Layout.

---

# 10. SOURCE ACCOUNT

Load:

```http
GET /api/v1/accounts
```

Show only accounts returned by backend.

Source selector displays:

```text
Account Name
Masked Account Number
Balance
Status
```

Only ACTIVE accounts should be selectable.

If backend returns locked account, disable it in UI.

---

# 11. SOURCE FROM QUERY STRING

Support:

```text
/transfer?sourceAccount=<accountId>
```

from account detail quick action.

If valid and owned:

auto-select.

If invalid:

ignore safely.

---

# 12. DESTINATION MODE

Allow 2 ways:

```text
1. Nhập số tài khoản
2. Chọn người thụ hưởng
```

No external bank selection.

---

# 13. ACCOUNT LOOKUP

When user enters destination account number:

Call:

```http
GET /api/v1/accounts/lookup/{accountNumber}
```

Do not call on every keystroke.

Use:

```text
explicit "Kiểm tra tài khoản" button
```

or debounced call.

Preferred:

explicit button to reduce unnecessary requests.

---

# 14. DESTINATION RESULT

After successful lookup display:

```text
Số tài khoản:
1000000002

Chủ tài khoản:
Tran Thi Binh
```

Do NOT display:

```text
Balance
CustomerId
Internal Account Id
```

---

# 15. INVALID DESTINATION

If account not found:

Display:

```text
Không tìm thấy tài khoản người nhận.
```

Disable Continue.

---

# 16. SAME ACCOUNT UI CHECK

Frontend should prevent:

```text
source account number == destination account number
```

Show:

```text
Không thể chuyển tiền đến chính tài khoản nguồn.
```

Backend remains authoritative.

---

# 17. BENEFICIARIES

Load:

```http
GET /api/v1/beneficiaries
```

Show:

```text
Nickname
Account Name
Account Number
```

Click beneficiary:

fills destination account.

Optionally performs lookup again to verify current account state.

---

# 18. ADD BENEFICIARY

Do not make beneficiary management the primary FE3 feature.

After successful transfer:

Optional checkbox:

```text
Lưu người nhận vào danh bạ
```

If checked and beneficiary not already saved:

Call:

```http
POST /api/v1/beneficiaries
```

Failure to save beneficiary must NOT turn successful transfer into failed transfer.

---

# 19. AMOUNT INPUT

Use numeric input.

Rules frontend:

```text
required
> 0
<= displayed source balance
```

Do not trust this for actual security.

Backend still validates balance.

---

# 20. VND UX

Display formatted preview:

```text
500000
→ 500.000 ₫
```

Reuse currency utility.

Do not convert to float unnecessarily.

---

# 21. DESCRIPTION

Optional.

Reasonable max length based on backend contract.

Example:

```text
Tiền ăn tháng 8
```

---

# 22. TRANSFER FORM STEPS

Preferred UX:

```text
STEP 1
Thông tin chuyển tiền

STEP 2
Xác nhận

STEP 3
Kết quả
```

Can implement with:

```text
QStepper
```

or clean state-driven layout.

Do not over-animate.

---

# 23. CONFIRMATION SCREEN

Before POST show:

```text
Tài khoản nguồn
Tài khoản nhận
Tên người nhận
Số tiền
Nội dung
```

Important:

User must explicitly click:

```text
Xác nhận chuyển tiền
```

---

# 24. IDEMPOTENCY KEY

CRITICAL.

Frontend must generate a unique:

```text
Idempotency-Key
```

for each new logical transfer attempt.

Use:

```ts
crypto.randomUUID()
```

where available.

Generate when user enters confirmation/starts transfer submission.

---

# 25. IDEMPOTENCY RETRY

If request is retried due to temporary network issue:

Reuse the SAME idempotency key.

Do NOT generate a new key for automatic retry.

This prevents double debit.

---

# 26. NEW TRANSFER

If user finishes/cancels and starts a completely new transfer:

Generate a NEW Idempotency-Key.

---

# 27. DOUBLE SUBMIT PROTECTION

While POST transfer is in progress:

Disable:

```text
Xác nhận chuyển tiền
```

Show loading.

Prevent double-click.

Idempotency is still backend protection, but UI should avoid duplicate submissions.

---

# 28. CREATE TRANSFER

Call:

```http
POST /api/v1/transfers
Idempotency-Key: <generated-key>
```

Body according to backend:

```json
{
  "sourceAccountId": "...",
  "destinationAccountNumber": "1000000002",
  "amount": 500000,
  "description": "..."
}
```

Do not send:

```text
balance
customerId
destinationCustomerId
status
reference
```

---

# 29. SUCCESS RECEIPT

After success show:

```text
Chuyển tiền thành công
```

Receipt:

```text
Reference
Source Account
Destination Account
Destination Name
Amount
Description
Status
Time
```

Add actions:

```text
Về Dashboard
Xem lịch sử giao dịch
Chuyển khoản mới
```

---

# 30. RECEIPT COPY

Provide:

```text
Sao chép mã giao dịch
```

using clipboard API.

Optional:

```text
Sao chép thông tin biên lai
```

No PDF generation needed.

---

# 31. REFRESH BALANCE

After successful transfer:

Do not calculate balance manually and mutate frontend store blindly.

Refetch:

```http
GET /api/v1/accounts
```

or invalidate relevant account data.

Backend is source of truth.

---

# 32. BUSINESS ERROR MESSAGES

Map ProblemDetails/error codes to Vietnamese UX.

Examples:

```text
INSUFFICIENT_FUNDS
→ Số dư không đủ để thực hiện giao dịch.

ACCOUNT_NOT_ACTIVE
→ Tài khoản nguồn hiện không thể giao dịch.

DESTINATION_NOT_ACTIVE
→ Tài khoản người nhận hiện không thể nhận tiền.

SAME_ACCOUNT_TRANSFER
→ Không thể chuyển tiền đến cùng tài khoản.

IDEMPOTENCY_CONFLICT
→ Yêu cầu giao dịch bị xung đột. Vui lòng tạo giao dịch mới.

CONCURRENCY_CONFLICT
→ Số dư tài khoản vừa thay đổi. Vui lòng kiểm tra lại.
```

Fallback:

```text
Không thể thực hiện giao dịch. Vui lòng thử lại.
```

---

# 33. NETWORK ERROR

If POST fails because connection temporarily drops:

Do not immediately assume transfer failed.

Because request may have reached backend.

Show:

```text
Không thể xác định trạng thái giao dịch.
Vui lòng thử lại với cùng yêu cầu hoặc kiểm tra lịch sử giao dịch.
```

If retry offered:

Reuse same Idempotency-Key.

---

# 34. TRANSACTION HISTORY PAGE

Implement:

```text
pages/transactions/index.vue
```

Use:

```http
GET /api/v1/transactions
```

CUSTOMER only.

---

# 35. TRANSACTION LIST

Display:

```text
Reference
Type
Counterparty
Amount
Date
Status
```

Newest first.

---

# 36. TRANSACTION FILTERS

Support backend-supported filters:

```text
accountId
type
fromDate
toDate
page
pageSize
```

Use actual API contract.

Do not invent filter params.

---

# 37. TRANSACTION TYPE FILTER

Options according to backend enum:

```text
TRANSFER
PAYMENT
DEPOSIT
WITHDRAWAL
```

Only include values backend supports.

---

# 38. DATE FILTER

Use:

```text
From Date
To Date
```

Validate:

```text
fromDate <= toDate
```

before request.

---

# 39. ACCOUNT FILTER

Load customer's accounts.

Allow:

```text
Tất cả tài khoản
1000000001
...
```

Only current customer accounts.

---

# 40. PAGINATION

Use shared backend model:

```text
items
page
pageSize
totalItems
totalPages
```

Do not fake pagination client-side.

---

# 41. PAGE SIZE

Use sensible options:

```text
10
20
50
```

Do not request >100.

---

# 42. TRANSACTION EMPTY STATE

No results:

```text
Không tìm thấy giao dịch phù hợp.
```

---

# 43. TRANSACTION DETAIL

Implement:

```text
pages/transactions/[id].vue
```

Call:

```http
GET /api/v1/transactions/{id}
```

Display:

```text
Reference
Type
Amount
Source
Destination
Description
Status
CreatedAt
CompletedAt
```

according to actual DTO.

---

# 44. OWNERSHIP ERROR

If transaction does not belong to customer:

Backend should return safe error.

Frontend:

```text
Không tìm thấy giao dịch.
```

Do not expose ownership details.

---

# 45. TRANSFER HISTORY OPTIONAL

If backend `/transfers` contains richer transfer-specific details, optionally add:

```text
pages/transfers/index.vue
```

But avoid duplicate UI if transaction history already satisfies customer need.

Preferred FE3:

```text
Transactions = primary history
```

Transfer detail/receipt can use transfer endpoint internally.

---

# 46. DASHBOARD INTEGRATION

Update FE2 Dashboard:

Quick action:

```text
Chuyển tiền
→ /transfer
```

Recent Transactions:

```text
Xem tất cả
→ /transactions
```

---

# 47. ACCOUNT DETAIL INTEGRATION

Account Detail buttons:

```text
Chuyển tiền
→ /transfer?sourceAccount=<id>

Lịch sử giao dịch
→ /transactions?accountId=<id>
```

---

# 48. NAVIGATION

Customer sidebar should have:

```text
Tổng quan
Tài khoản
Chuyển tiền
Giao dịch
Hóa đơn
Thông báo
Hồ sơ
```

Highlight current route.

---

# 49. LOADING UX

Transfer:

```text
lookup loading
submit loading
```

Transactions:

```text
table skeleton/loading
```

Use Quasar existing components.

---

# 50. ERROR UX

Reuse:

```text
AppAlert
QNotify
```

Do not create another unrelated error framework.

---

# 51. RESPONSIVE TRANSFER UI

Desktop:

```text
Form + summary
```

Mobile:

Stack vertically.

Confirmation remains readable.

No horizontal overflow.

---

# 52. RESPONSIVE TRANSACTION HISTORY

Desktop:

Can use table.

Mobile:

Prefer list/cards instead of wide table if necessary.

Do not force horizontal scrolling for essential information.

---

# 53. DESIGN

Keep:

```text
InterLink Banking
Premium dark banking UI
```

Transfer page should visually emphasize:

```text
Amount
Recipient
Confirmation
```

Avoid clutter.

---

# 54. NO EXTERNAL TRANSFER

Do NOT create:

```text
Bank selector
SWIFT
Other bank
NAPAS
External bank fee
External account
```

Backend does not support it.

UI must honestly represent:

```text
Chuyển khoản nội bộ InterLink
```

---

# 55. NO OTP

Backend V1 does not support OTP.

Do NOT fake:

```text
OTP verification
SMS code
2FA
```

Could document as future enhancement.

---

# 56. NO TRANSFER FEE

Unless backend contract provides fee:

Display:

```text
Phí: 0 ₫
```

only if explicitly treated as internal transfer rule.

Otherwise omit fee entirely.

Do not invent fee calculation.

---

# 57. API VERIFICATION SCRIPT — REQUIRED

Create or update a lightweight PowerShell script, for example:

```text
scripts/verify-fe3-api.ps1
```

Purpose:

Verify backend APIs that FE3 depends on WITHOUT Playwright.

---

# 58. API TEST FLOW

PowerShell/curl flow:

```text
1. Login Customer 1
2. Capture access token
3. GET accounts
4. GET destination lookup
5. GET beneficiaries
6. Record source/destination balances
7. POST transfer with unique Idempotency-Key
8. GET accounts again
9. Verify debit/credit
10. Replay same request/key
11. Verify no second debit
12. GET transactions
13. GET transaction detail
14. Verify transfer appears
```

---

# 59. API TEST DATA

If database is clean:

Expected example:

```text
Source:
1000000001

Destination:
1000000002

Transfer:
100,000 VND
```

Do NOT hardcode final balance unless clean reset was explicitly performed.

Better:

```text
beforeSource - amount = afterSource
beforeDestination + amount = afterDestination
```

---

# 60. API ASSERTION — MONEY

Verify:

```text
Source difference = -Amount
Destination difference = +Amount
```

and:

```text
SourceBefore + DestinationBefore
=
SourceAfter + DestinationAfter
```

for internal transfer.

---

# 61. API ASSERTION — IDEMPOTENCY

Replay same:

```text
Idempotency-Key
```

Expected:

```text
same TransferId/Reference
no balance change
```

---

# 62. API ASSERTION — ERROR

Optional safe tests:

```text
same source/destination → 400
insufficient funds → 400
```

Do not modify unnecessary data.

---

# 63. FRONTEND BUILD TEST

Run:

```bash
cd frontend
npm run build
```

Must pass.

---

# 64. BACKEND REGRESSION

Run:

```bash
dotnet test backend/LocalLink.sln
```

All existing backend tests must pass.

Baseline:

```text
64
```

or actual latest count.

Do not assert exact number if suite changed.

---

# 65. DOCKER TEST

Run:

```bash
docker compose up --build -d
```

Verify:

```text
web running
api healthy
sqlserver healthy
```

---

# 66. UI VERIFICATION

Do NOT use Playwright.

Perform only minimal manual smoke check if needed:

```text
/login
/dashboard
/transfer
/transactions
```

Verify pages render.

No automated browser screenshots required.

---

# 67. API TEST IS PRIMARY E2E

M3–M7 backend behavior is already tested.

For FE3 completion, primary automated verification is:

```text
API E2E script PASS
+
frontend build PASS
+
backend tests PASS
+
Docker PASS
```

Not Playwright.

---

# 68. DOCUMENTATION

Create/update:

```text
Docs/frontend/transfer.md
Docs/frontend/transactions.md
```

Document:

```text
Routes
API contract
Idempotency handling
Retry behavior
Error mapping
Filters
Responsive behavior
```

---

# 69. FRONTEND ROADMAP

After success:

```text
FE1 Auth + App Shell                   ✅
FE2 Dashboard + Accounts + Profile     ✅
FE3 Transfer + Transactions            ✅
FE4 Bills + Payments + Notifications   TODO
FE5 Staff/Admin UI                     TODO
```

---

# 70. GIT

After verification:

```bash
git status
```

No secrets.

Commit:

```bash
git add .
git commit -m "feat: implement transfer and transaction frontend"
```

Push:

```bash
git push -u origin feature/frontend-transfer
```

Follow current integration workflow.

---

# 71. DEFINITION OF DONE

FE3 COMPLETE only when:

```text
[ ] Transfer page works

[ ] Source account selector works
[ ] Source query parameter works

[ ] Destination account lookup works
[ ] Invalid account handled
[ ] Same account handled

[ ] Beneficiary selection works

[ ] Amount validation works
[ ] Description works

[ ] Confirmation step works

[ ] Idempotency-Key generated
[ ] Same key reused on retry
[ ] Double-click prevented

[ ] Transfer success receipt works

[ ] Account balance refetched after success

[ ] Transfer business errors mapped

[ ] Network ambiguous state handled safely

[ ] Transactions page works
[ ] Filters work
[ ] Pagination works
[ ] Account filter works
[ ] Transaction detail works
[ ] Ownership errors handled safely

[ ] Dashboard quick links updated
[ ] Account quick links updated

[ ] Desktop responsive
[ ] Mobile responsive

[ ] No External Transfer fake UI
[ ] No OTP fake UI

[ ] API verification script passes
[ ] Frontend build passes
[ ] Backend tests pass
[ ] Docker passes

[ ] No Playwright required
[ ] Docs updated
[ ] Branch pushed
```

---

# 72. FINAL REPORT

Return:

```text
LOCALINK M8 — FE3 TRANSFER & TRANSACTION REPORT

Branch:

Transfer Route:

Transaction Routes:

Transfer Service:

Transaction Service:

Beneficiary Integration:

Source Account Selection:

Destination Lookup:

Amount Validation:

Confirmation Flow:

Idempotency-Key Strategy:

Retry Strategy:

Double Submit Protection:

Receipt:

Balance Refresh:

Error Mapping:

Transaction Filters:

Pagination:

Transaction Detail:

Responsive Desktop:

Responsive Mobile:

API Verification Script:

API Transfer Verification:

API Balance Verification:

API Idempotency Verification:

Frontend Build:

Backend Regression:

Docker:

Playwright:
NOT USED

Documentation:

Commit:

Push:

Warnings / Known Limitations:

FE3 STATUS:
COMPLETE / NOT COMPLETE
```

Then STOP.

Do not begin FE4 automatically.