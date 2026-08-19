# LocalLink M8 --- FE4: Bills, Payments & Notifications

## MỤC TIÊU

Triển khai hoàn chỉnh Customer Frontend cho:

1.  Danh sách hóa đơn
2.  Chi tiết hóa đơn
3.  Thanh toán hóa đơn
4.  Xác nhận trước thanh toán
5.  Idempotent Payment
6.  Biên lai thanh toán
7.  Lịch sử thanh toán
8.  Danh sách thông báo
9.  Unread notification badge
10. Mark as read / Mark all as read

KHÔNG triển khai chức năng Backend mới nếu API hiện tại đã đủ.

KHÔNG dùng Playwright/browser automation để verification.

Testing ưu tiên:

1.  API E2E PowerShell
2.  npm run build
3.  dotnet test
4.  Docker health
5.  Manual UI smoke check tối thiểu nếu cần

------------------------------------------------------------------------

# 1. CURRENT PROJECT STATUS

Backend:

M1 Foundation COMPLETE M2 Database COMPLETE M3 Authentication COMPLETE
M4 Customer + Accounts COMPLETE M5 Transfer Engine COMPLETE M6 Bills +
Payments + Notifications COMPLETE M7 Admin/Staff Backend COMPLETE

Frontend:

FE1 Authentication + App Shell COMPLETE FE2 Dashboard + Accounts +
Profile COMPLETE FE3 Transfer + Transactions COMPLETE FE4 Bills +
Payments + Notifications \<-- IMPLEMENT NOW

Do NOT start FE5.

------------------------------------------------------------------------

# 2. IMPORTANT: INSPECT EXISTING CODE FIRST

Before implementing:

-   Inspect current frontend architecture.
-   Reuse existing:
    -   services/api.ts
    -   Pinia auth store
    -   AppAlert
    -   currency/date utilities
    -   Customer layout
    -   API error handling
    -   PagedResult
    -   ProblemDetails
    -   Bill/Notification types created during FE2 if they already
        exist.

Read actual Backend contract:

Docs/api/bills.md Docs/api/payments.md Docs/api/notifications.md
Docs/api/frontend-contract.md

Also inspect Controllers/DTOs if documentation and implementation
differ.

Backend implementation is the source of truth.

DO NOT guess DTO fields or endpoint parameters.

------------------------------------------------------------------------

# 3. GIT

Create working branch from latest feature/hoi:

git checkout feature/hoi git pull origin feature/hoi git checkout -b
feature/frontend-bill-payment

If current FE3 work has not been integrated yet, preserve it and branch
from the latest correct frontend state.

------------------------------------------------------------------------

# 4. ROUTES

Implement CUSTOMER routes:

/bills /bills/\[id\] /payments /payments/\[id\] /notifications

All routes must use CUSTOMER middleware.

Do not expose them to STAFF/ADMIN customer UI.

------------------------------------------------------------------------

# 5. TYPES

Create/update:

frontend/types/bill.ts frontend/types/payment.ts
frontend/types/notification.ts

Use actual backend DTOs.

Expected concepts include:

BillListItem BillDetail

PaymentReceipt PaymentListItem PaymentDetail

Notification UnreadNotificationCount

Do not duplicate existing interfaces unnecessarily.

Export through existing types structure if project uses barrel exports.

------------------------------------------------------------------------

# 6. SERVICES

Implement/reuse:

frontend/services/billService.ts frontend/services/paymentService.ts
frontend/services/notificationService.ts

Bill functions:

getBills(filters) getBill(id)

Payment functions:

payBill(request, idempotencyKey) getPayments(filters) getPayment(id)

Notification functions:

getNotifications(...) getNotification(id) getUnreadCount()
markAsRead(id) markAllAsRead()

ALL requests must use the existing shared authenticated API client.

Do NOT create a second \$fetch authentication implementation.

------------------------------------------------------------------------

# 7. BILLS PAGE

Implement:

pages/bills/index.vue

Display customer bills.

Information should follow backend DTO, normally:

Provider Bill Number Bill Type Amount Due Date Status

Provide filters only for parameters supported by backend.

Possible filters based on existing backend:

status type fromDueDate toDueDate page pageSize

Verify actual contract first.

------------------------------------------------------------------------

# 8. BILL STATUS UX

Display clear status badges:

UNPAID OVERDUE PAID CANCELLED

Use consistent Quasar badges/chips.

Do not rely only on color.

Display readable Vietnamese labels.

Example:

UNPAID -\> Chưa thanh toán OVERDUE -\> Quá hạn PAID -\> Đã thanh toán
CANCELLED -\> Đã hủy

------------------------------------------------------------------------

# 9. BILL LIST RESPONSIVE

Desktop:

Table/list layout with:

Provider Bill Due Date Amount Status Action

Mobile:

Card/list layout.

Avoid essential horizontal scrolling.

------------------------------------------------------------------------

# 10. BILL DETAIL

Implement:

pages/bills/\[id\].vue

Call:

GET /api/v1/bills/{id}

Show complete available bill information.

Primary CTA:

"Thanh toán ngay"

Only enable payment when backend/business state allows it.

PAID bills:

Do not show active payment button.

Instead show:

"Đã thanh toán"

CANCELLED:

Cannot pay.

OVERDUE:

Allow payment only if Backend allows overdue payment.

Backend remains authoritative.

------------------------------------------------------------------------

# 11. PAYMENT FLOW

Payment should follow:

Bill Detail ↓ Select Source Account ↓ Review Amount ↓ Confirmation ↓
POST /api/v1/payments ↓ Receipt ↓ Refetch Bill + Account ↓ Payment
History

Do not allow user to manually modify Bill.Amount.

Amount MUST come from backend Bill data.

------------------------------------------------------------------------

# 12. SOURCE ACCOUNT

Load:

GET /api/v1/accounts

Only ACTIVE accounts selectable.

Display:

Masked account number Account name/type if available Available balance

Frontend can warn if:

balance \< bill.amount

but Backend is authoritative.

------------------------------------------------------------------------

# 13. PAYMENT CONFIRMATION

Before POST display:

Provider Bill Number Bill Type Source Account Amount Due Date

User must explicitly press:

"Xác nhận thanh toán"

No automatic payment.

------------------------------------------------------------------------

# 14. IDEMPOTENCY --- CRITICAL

Generate one UUID for each logical payment attempt using:

crypto.randomUUID()

Send:

Idempotency-Key: `<uuid>`{=html}

The SAME key MUST be reused if the same payment request is retried
because of:

network interruption timeout unknown response state

Do NOT generate a new key for automatic retry.

A completely new payment attempt receives a new key.

------------------------------------------------------------------------

# 15. DOUBLE SUBMIT

While payment POST is running:

Disable confirmation button.

Show loading state.

Prevent repeated click.

Backend idempotency remains final protection.

------------------------------------------------------------------------

# 16. PAYMENT API

Use actual backend request DTO.

Expected concept:

POST /api/v1/payments

{ "billId": "...", "accountId": "..." }

Do NOT send:

amount balance customerId billStatus paymentStatus reference

unless actual backend contract explicitly requires it.

Backend determines financial values.

------------------------------------------------------------------------

# 17. PAYMENT SUCCESS

Display success receipt.

Information according to PaymentReceiptDto:

Payment Reference Provider Bill Number Account Number Amount Currency
Status Paid Time

Actions:

Về Dashboard Xem lịch sử thanh toán Xem thông báo Thanh toán hóa đơn
khác

Provide:

"Sao chép mã giao dịch"

No PDF receipt needed in FE4.

------------------------------------------------------------------------

# 18. REFRESH AFTER PAYMENT

After successful payment:

Refetch:

Bill Accounts Notifications/unread count

Do NOT manually mutate balance using:

balance -= bill.amount

Backend remains Source of Truth.

------------------------------------------------------------------------

# 19. PAYMENT ERROR MAPPING

Handle common backend business errors.

Examples:

Insufficient funds Bill already paid Bill cancelled Account locked/not
active Ownership failure Idempotency conflict Concurrency conflict

Display Vietnamese user-friendly messages.

Do not expose stack traces.

If exact error codes exist, map those codes.

Otherwise use ProblemDetails safely.

------------------------------------------------------------------------

# 20. AMBIGUOUS NETWORK PAYMENT STATE

Financial safety requirement.

If network fails during POST:

DO NOT immediately tell user:

"Thanh toán thất bại"

because backend may already have processed it.

Instead show something similar to:

"Không thể xác định trạng thái thanh toán. Vui lòng kiểm tra lịch sử
thanh toán trước khi thử lại."

If implementing retry:

reuse SAME Idempotency-Key.

------------------------------------------------------------------------

# 21. PAYMENT HISTORY

Implement:

pages/payments/index.vue

Call:

GET /api/v1/payments

Use backend pagination/filter contract.

Display:

Reference Provider Bill Number Amount Status Paid Time

Newest first according to API.

------------------------------------------------------------------------

# 22. PAYMENT DETAIL

Implement:

pages/payments/\[id\].vue

Call:

GET /api/v1/payments/{id}

Display full receipt/detail.

If resource does not belong to customer:

show generic:

"Không tìm thấy giao dịch thanh toán."

Do not expose ownership information.

------------------------------------------------------------------------

# 23. NOTIFICATIONS PAGE

Implement:

pages/notifications/index.vue

Call:

GET /api/v1/notifications

Display:

Type Title Message Created time Read/unread state

Unread notifications should be visually distinguishable.

------------------------------------------------------------------------

# 24. MARK NOTIFICATION READ

Click/open notification:

PATCH /api/v1/notifications/{id}/read

Update UI after successful backend response.

Avoid repeated unnecessary requests for already-read items.

------------------------------------------------------------------------

# 25. MARK ALL AS READ

Provide button:

"Đánh dấu tất cả đã đọc"

Call:

PATCH /api/v1/notifications/read-all

Then refetch:

notification list unread count

------------------------------------------------------------------------

# 26. UNREAD BADGE

Integrate notification unread count into Customer App Shell.

Use:

GET /api/v1/notifications/unread-count

Display badge beside:

"Thông báo"

Do not continuously poll aggressively.

Refresh count on:

initial authenticated app load opening notifications successful payment
successful transfer if useful mark read mark all read

Avoid unnecessary API traffic.

------------------------------------------------------------------------

# 27. NOTIFICATION DETAIL

If Backend notification detail endpoint is useful, either:

open detail inside page/dialog

or create:

/notifications/\[id\]

Do not create route if it adds no meaningful UX.

Keep FE4 focused.

------------------------------------------------------------------------

# 28. DASHBOARD INTEGRATION

Update Dashboard.

Bills section:

Show a small number of unpaid/overdue bills.

CTA:

"Xem tất cả hóa đơn" → /bills

Notification quick action:

→ /notifications

If dashboard already loads bills from FE2, reuse existing logic.

------------------------------------------------------------------------

# 29. CUSTOMER NAVIGATION

Final customer navigation should now contain:

Tổng quan Tài khoản Chuyển tiền Giao dịch Hóa đơn Thanh toán Thông báo
Hồ sơ

Avoid duplicate menu entries if UX becomes cluttered.

------------------------------------------------------------------------

# 30. UI CONSISTENCY

Maintain current:

InterLink/LocalLink Banking visual identity Dark banking UI Quasar
Responsive layout

Reuse:

QCard QBtn QBadge/QChip QSelect QSkeleton QPagination QDialog where
appropriate

Do not redesign the entire application.

------------------------------------------------------------------------

# 31. LOADING STATES

Every page should have appropriate:

initial loading empty state error state

Payment action:

submitting state

Notifications:

marking state where necessary

------------------------------------------------------------------------

# 32. EMPTY STATES

Bills:

"Bạn chưa có hóa đơn nào."

Payments:

"Bạn chưa có giao dịch thanh toán nào."

Notifications:

"Bạn chưa có thông báo nào."

Filtered results should distinguish no matching result where
appropriate.

------------------------------------------------------------------------

# 33. API E2E SCRIPT --- REQUIRED

Create:

scripts/verify-fe4-api.ps1

DO NOT USE PLAYWRIGHT.

The script should verify the Backend APIs FE4 depends on.

------------------------------------------------------------------------

# 34. API TEST FLOW

Use Customer 1 demo account.

Flow:

1.  Login Customer 1
2.  Capture JWT
3.  GET accounts
4.  Record account balance
5.  GET bills
6.  Find an UNPAID demo bill
7.  GET bill detail
8.  Generate unique Idempotency-Key
9.  POST payment
10. Save PaymentId + Reference
11. GET account again
12. Verify balance decreased exactly by Bill.Amount
13. Replay SAME payment request with SAME key
14. Verify same payment result
15. Verify balance NOT debited twice
16. GET bill again
17. Verify status PAID
18. GET payments
19. Verify payment appears
20. GET payment detail
21. GET transactions
22. Verify PAYMENT ledger entry exists
23. GET notifications
24. Verify PAYMENT notification exists
25. GET unread count
26. Mark notification read
27. Verify unread count decreases
28. Mark all read
29. Verify unread count = 0

Use assertions.

Script must exit non-zero on failure.

------------------------------------------------------------------------

# 35. TEST DATA SAFETY

The API verification script should work predictably.

If demo bill is already PAID because database contains previous test
state:

Do NOT silently fail.

Prefer one of:

A. document that script expects clean seeded database

OR

B. dynamically select an available UNPAID bill

Preferred: B where possible.

Do not reset database automatically unless explicitly required.

------------------------------------------------------------------------

# 36. BALANCE ASSERTION

Do NOT hardcode final balance.

Use:

expectedAfter = beforeBalance - billAmount

Assert:

actualAfter == expectedAfter

After idempotent replay:

balanceAfterReplay == actualAfter

------------------------------------------------------------------------

# 37. IDEMPOTENCY ASSERTION

Same:

Idempotency-Key BillId AccountId

should return same logical payment/receipt.

Assert:

same PaymentId and/or Reference

and:

NO second debit.

------------------------------------------------------------------------

# 38. BILL ALREADY PAID TEST

After payment, optionally send a NEW logical request/key for same PAID
bill.

Expected:

Backend rejection.

Assert expected 4xx according to actual API contract.

Do not change backend merely to satisfy frontend script.

------------------------------------------------------------------------

# 39. FRONTEND BUILD

Run:

cd frontend npm run build

Must PASS.

No TypeScript build errors.

------------------------------------------------------------------------

# 40. BACKEND REGRESSION

Run:

dotnet test backend/LocalLink.sln

All existing tests must PASS.

Do not rely on an exact historical count.

Report actual result.

------------------------------------------------------------------------

# 41. DOCKER

Run:

docker compose up --build -d

Verify:

locallink-web running locallink-api healthy locallink-sqlserver healthy

Do not automatically run `docker compose down -v` unless clean database
is genuinely required.

Preserve developer data where possible.

------------------------------------------------------------------------

# 42. NO PLAYWRIGHT

IMPORTANT:

DO NOT:

install Playwright run Playwright run browser automation generate
browser screenshots create Playwright tests

unless explicitly requested later.

For FE4 automated verification:

API E2E + build + backend tests + Docker health

is sufficient.

------------------------------------------------------------------------

# 43. MANUAL UI CHECK

If UI verification is needed, provide routes for developer to manually
inspect:

http://localhost:3000/bills http://localhost:3000/payments
http://localhost:3000/notifications

Do not automate browser interaction.

------------------------------------------------------------------------

# 44. DOCUMENTATION

Create/update:

Docs/frontend/bills.md Docs/frontend/payments.md
Docs/frontend/notifications.md

Document:

Routes Services Backend endpoints Payment flow Idempotency strategy
Network retry safety Notification unread behavior Responsive behavior

Update frontend roadmap if one exists.

------------------------------------------------------------------------

# 45. SECURITY RULES

Never expose:

JWT Refresh Token Password Password Hash Customer internal security data

in UI/logs/docs.

Do not log Authorization headers.

Do not persist additional financial data unnecessarily.

Continue using existing authentication architecture.

------------------------------------------------------------------------

# 46. DO NOT MODIFY FINANCIAL RULES

Frontend MUST NOT:

calculate authoritative balance change Bill.Amount set Bill.Status set
Payment.Status create transaction ledger entries create audit logs

These remain Backend responsibilities.

------------------------------------------------------------------------

# 47. DEFINITION OF DONE

FE4 is COMPLETE only when:

\[ \] Bills list implemented \[ \] Bill filters implemented according to
actual API \[ \] Bill detail implemented \[ \] Bill status UI
implemented

\[ \] Payment account selection works \[ \] Confirmation works \[ \]
Amount cannot be modified \[ \] Idempotency-Key implemented \[ \] Retry
reuses same key \[ \] Double-submit prevented \[ \] Payment receipt
implemented \[ \] Account refetched after payment \[ \] Bill refetched
after payment \[ \] Financial errors handled safely

\[ \] Payment history implemented \[ \] Payment detail implemented

\[ \] Notifications implemented \[ \] Unread badge implemented \[ \]
Mark read works \[ \] Mark all read works

\[ \] Dashboard integrated \[ \] Navigation integrated

\[ \] Desktop responsive \[ \] Mobile responsive

\[ \] verify-fe4-api.ps1 PASS \[ \] npm run build PASS \[ \] dotnet test
PASS \[ \] Docker services healthy

\[ \] Playwright NOT USED

\[ \] Documentation completed \[ \] Git commit completed \[ \] Branch
pushed

------------------------------------------------------------------------

# 48. GIT COMMIT

After verification:

git status

Ensure no secrets or temporary files.

Commit:

git add . git commit -m "feat: implement bill payment and notification
frontend"

Push:

git push -u origin feature/frontend-bill-payment

Follow existing project integration workflow for feature/hoi.

------------------------------------------------------------------------

# 49. FINAL REPORT

Return exactly a concise report containing:

LOCALINK M8 --- FE4 REPORT

Branch:

Routes Implemented:

Bills UI:

Bill Detail:

Payment Flow:

Source Account Selection:

Payment Confirmation:

Idempotency Strategy:

Retry Safety:

Payment Receipt:

Payment History:

Notifications:

Unread Badge:

Mark Read:

Dashboard Integration:

Responsive:

API Verification Script:

Payment API Test:

Balance Verification:

Idempotency Verification:

Bill Status Verification:

Notification Verification:

Frontend Build:

Backend Tests:

Docker:

Playwright: NOT USED

Documentation:

Commit:

Push:

Warnings / Known Limitations:

FE4 STATUS: COMPLETE / NOT COMPLETE

Then STOP.

DO NOT begin FE5 automatically.
