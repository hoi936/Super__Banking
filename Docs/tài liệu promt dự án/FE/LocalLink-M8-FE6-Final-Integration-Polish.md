# LocalLink M8 — FE6: Final Integration, Polish & Release Readiness

## 0. CURRENT PROJECT STATUS

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
FE5 — Staff/Admin Management UI               ✅
FE6 — Final Integration, Polish & QA           🔄 IMPLEMENT NOW
```

FE6 is the final frontend milestone before V1 release verification.

Do NOT add new major business features.
Do NOT redesign backend architecture.
Do NOT start Flutter.
Do NOT start Azure deployment automatically.

---

# 1. OBJECTIVE

Finalize the entire LocalLink / InterLink Banking V1 frontend and integration quality.

Focus on:

```text
Cross-feature integration
Visual consistency
Responsive quality
Loading / Empty / Error UX
Auth/session edge cases
RBAC regression
API contract consistency
Performance cleanup
Accessibility basics
Docker clone-and-run readiness
Documentation completeness
Release readiness
```

This milestone is for polishing and validating what already exists.

---

# 2. TESTING RULE — IMPORTANT

DO NOT use Playwright/browser automation.

DO NOT:

```text
install Playwright
run Playwright
create Playwright test files
generate automated screenshots
```

Verification priority:

```text
1. API E2E PowerShell scripts
2. npm run build
3. dotnet test backend/LocalLink.sln
4. Docker clean/full-stack verification
5. Manual UI smoke check only where needed
```

Existing API verification scripts from FE3/FE4/FE5 should be reused.

---

# 3. FEATURE BRANCH

Inspect current state:

```bash
git status
git branch
```

Create:

```bash
git checkout feature/hoi
git pull origin feature/hoi
git checkout -b feature/frontend-final-polish
```

If latest FE5 code is not integrated into `feature/hoi`, branch from the actual latest FE5 state.

Never discard completed work.

---

# 4. INSPECT THE ENTIRE FRONTEND FIRST

Review:

```text
frontend/app.vue
frontend/layouts/
frontend/pages/
frontend/components/
frontend/composables/
frontend/services/
frontend/stores/
frontend/types/
frontend/utils/
frontend/middleware/
frontend/plugins/
frontend/nuxt.config.ts
```

Look for:

```text
duplicate code
inconsistent naming
inconsistent status labels
hardcoded values
unused imports
console.log
dead components
duplicate types
duplicate API wrappers
broken links
placeholder pages
inconsistent route names
```

Do not perform a risky architecture rewrite.

Prefer targeted cleanup.

---

# 5. BRANDING CONSISTENCY

Visible product brand:

```text
InterLink Banking
```

Repository / technical project name may remain:

```text
LocalLink
```

Apply this convention consistently.

Do not rename backend namespaces/solution solely for branding.

Review:

```text
Page titles
Sidebar header
Login
Dashboard
Admin header
404 page
README screenshots/text
```

Remove accidental mixed visible branding where confusing.

---

# 6. ROUTE AUDIT

Verify all intended routes exist and resolve:

### Public

```text
/
/login
```

### Customer

```text
/dashboard
/accounts
/accounts/[id]
/transfer
/transactions
/transactions/[id]
/bills
/bills/[id]
/payments
/payments/[id]
/notifications
/profile
```

### Staff/Admin

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
```

Do not create missing routes unless they are part of existing V1 scope.

Fix broken navigation.

---

# 7. AUTH REGRESSION

Verify frontend auth foundation still works:

```text
Customer login
Staff login
Admin login
Logout
F5 session restore
Access-token refresh
Invalid refresh token → logout
Guest route redirect
Protected route redirect
```

Do not change token strategy unless a real bug exists.

---

# 8. RBAC REGRESSION

Verify UI-level role behavior:

```text
CUSTOMER:
- cannot access /admin
- sees customer navigation only

STAFF:
- can access admin dashboard/customers/transactions/payments
- cannot access users/audit logs
- cannot see admin-only mutation actions

ADMIN:
- sees full admin navigation
- can perform supported status mutations
```

Backend remains authoritative.

---

# 9. STRING ENUM CONSISTENCY

Backend now uses `JsonStringEnumConverter`.

Audit all frontend enum consumers.

Ensure frontend expects string values consistently:

```text
ACTIVE
SUSPENDED
LOCKED
CLOSED
COMPLETED
PENDING
TRANSFER
PAYMENT
UNPAID
PAID
OVERDUE
```

Remove legacy assumptions that API returns integer enums.

Do NOT duplicate enum conversion logic unnecessarily.

---

# 10. STATUS LABEL CENTRALIZATION

Create/reuse centralized helpers for readable Vietnamese labels.

Examples:

```text
ACTIVE → Hoạt động
SUSPENDED → Tạm khóa
LOCKED → Đã khóa
CLOSED → Đã đóng

COMPLETED → Hoàn tất
PENDING → Đang xử lý
FAILED → Thất bại

UNPAID → Chưa thanh toán
PAID → Đã thanh toán
OVERDUE → Quá hạn
CANCELLED → Đã hủy
```

Prefer reusable utility/component over repeated switch statements.

---

# 11. STATUS VISUAL CONSISTENCY

Review badges/chips across:

```text
Accounts
Customers
Users
Transactions
Transfers
Bills
Payments
Notifications
Admin pages
```

Use consistent semantics.

Do not rely on color alone.

Use readable text + icon where useful.

---

# 12. CURRENCY FORMAT CONSISTENCY

Use one shared currency formatter.

Preferred user-facing format:

```text
25.000.000 ₫
```

or current consistent project style.

Do not mix:

```text
25,000,000 VND
25.000.000 ₫
25000000
```

across different pages unless data input requires raw values.

---

# 13. DATE/TIME FORMAT CONSISTENCY

Use shared date utility.

User-facing:

```text
18/08/2026
18/08/2026 21:30
```

Handle UTC timestamps correctly.

Do not show raw ISO strings unnecessarily.

---

# 14. LOADING UX AUDIT

Every data page should have proper loading UI.

Review:

```text
Dashboard
Accounts
Account Detail
Profile
Transfer
Transactions
Transaction Detail
Bills
Bill Detail
Payments
Payment Detail
Notifications

Admin Dashboard
Customers
Customer Detail
Admin Transactions
Admin Payments
Users
User Detail
Audit Logs
```

Use:

```text
QSkeleton
QInnerLoading
QSpinner
```

appropriately.

Avoid blank screens.

---

# 15. EMPTY STATE AUDIT

Every list should have useful empty states.

Examples:

```text
No accounts
No transactions
No bills
No payments
No notifications
No customers matching filters
No users matching filters
No audit logs
```

Distinguish:

```text
no data
```

from:

```text
no results after filtering
```

where practical.

---

# 16. ERROR UX AUDIT

Ensure API failures are displayed consistently.

Use:

```text
AppAlert
QNotify
ProblemDetails parser
```

Avoid raw JSON dumps.

Common errors:

```text
Network unavailable
401
403
404
409
429
500
```

---

# 17. 401 HANDLING

Verify:

```text
Access token expires
↓
refresh once
↓
retry original request
```

No infinite loops.

If refresh fails:

```text
clear session
redirect /login
```

---

# 18. 403 HANDLING

Customer hitting admin:

```text
redirect /dashboard
```

Staff hitting admin-only:

```text
redirect /admin
```

Show friendly permission message.

---

# 19. 404 HANDLING

Ensure 404 page works.

For resource-specific 404:

Use safe copy such as:

```text
Không tìm thấy dữ liệu yêu cầu.
```

Do not expose ownership/security internals.

---

# 20. 409 HANDLING

Review:

```text
Transfer idempotency conflict
Payment idempotency conflict
Concurrency conflict
Duplicate beneficiary
```

Use specific friendly messages.

Do not tell user to blindly resubmit financial requests.

---

# 21. 429 HANDLING

Backend rate limiting exists.

When receiving:

```text
429 Too Many Requests
```

display:

```text
Bạn đang thao tác quá nhanh. Vui lòng thử lại sau.
```

No automatic aggressive retry.

---

# 22. FINANCIAL SAFETY AUDIT

Re-check Transfer and Payment frontend.

Transfer:

```text
Idempotency-Key generated once per logical transfer
same key reused for retry
double-submit blocked
balance refetched from backend
```

Payment:

```text
Idempotency-Key generated once
same key reused after ambiguous network failure
amount comes from Bill
balance refetched from backend
```

Never mutate authoritative balances client-side.

---

# 23. AMBIGUOUS NETWORK STATE

For financial request where connection drops after submission:

Do not state definite failure.

Display a safe message and direct user to history.

Transfer:

```text
Kiểm tra lịch sử giao dịch trước khi thử lại.
```

Payment:

```text
Kiểm tra lịch sử thanh toán trước khi thử lại.
```

If retry exists, reuse idempotency key.

---

# 24. RESPONSIVE AUDIT — DESKTOP

Review around:

```text
1440px
1920px
```

Verify:

```text
content width
sidebar
tables
cards
spacing
header
dialogs
forms
```

Avoid excessively stretched content.

---

# 25. RESPONSIVE AUDIT — TABLET

Review around:

```text
768px
```

Verify drawer behavior and tables/cards.

No layout overlap.

---

# 26. RESPONSIVE AUDIT — MOBILE

Review around:

```text
375px
```

Customer flows must remain usable:

```text
Login
Dashboard
Accounts
Transfer
Transactions
Bills
Payment
Notifications
Profile
```

Admin mobile is secondary but must not break.

No essential horizontal overflow.

---

# 27. NAVIGATION AUDIT

Verify active route state.

Customer navigation:

```text
Tổng quan
Tài khoản
Chuyển tiền
Giao dịch
Hóa đơn
Thanh toán
Thông báo
Hồ sơ
```

Admin:

```text
Dashboard
Khách hàng
Giao dịch
Thanh toán
Users (ADMIN)
Audit Logs (ADMIN)
```

Remove duplicate/unnecessary links.

---

# 28. DASHBOARD AUDIT

Customer dashboard should prioritize:

```text
Balance
Accounts
Quick actions
Recent transactions
Bills
Notifications
```

Remove/de-emphasize old technical telemetry.

Admin dashboard should prioritize:

```text
Customer/account KPIs
financial volumes
operational shortcuts
```

---

# 29. PROFILE AUDIT

Verify editable fields only:

```text
FullName
DateOfBirth
Gender
PhoneNumber
Address
```

Protected fields should not be editable.

Confirm saved data refetches correctly.

---

# 30. TABLE/FILTER CONSISTENCY

Across:

```text
Customer transactions
Payments
Admin customers
Admin transactions
Admin payments
Admin users
Audit logs
```

Standardize:

```text
filter spacing
search placement
page-size selector
pagination
reset filters
date validation
```

Do not force one abstraction if it complicates code.

---

# 31. URL QUERY STATE

Review FE3/FE5 query-state behavior.

Preserve useful filters on refresh/back navigation where already implemented.

Fix broken query synchronization if present.

Do not add complexity to every page unnecessarily.

---

# 32. FORM CONSISTENCY

Review forms:

```text
Login
Profile
Transfer
Payment confirmation
Admin status actions
```

Standardize:

```text
validation
disabled state
loading state
success feedback
error feedback
```

---

# 33. BUTTON SAFETY

Financial/destructive-ish actions:

```text
Transfer submit
Payment submit
Suspend customer
Lock account
Suspend user
```

must:

```text
disable while request running
show confirmation where appropriate
prevent obvious repeated clicks
```

---

# 34. ACCESSIBILITY BASICS

Review:

```text
input labels
button labels
tooltips
keyboard access
focus visibility
status text
contrast
```

No need for full WCAG audit, but obvious accessibility defects should be fixed.

---

# 35. PERFORMANCE CLEANUP

Look for unnecessary duplicate API requests.

Examples:

```text
dashboard fetching same customer/accounts multiple times
layout + page fetching unread count repeatedly
duplicate auth /me requests
```

Reduce only obvious duplication.

Do not introduce complex caching library.

---

# 36. NOTIFICATION COUNT STRATEGY

Keep unread notification count lightweight.

Refresh on meaningful events:

```text
app/session init
open notifications
mark read
mark all read
successful transfer
successful payment
```

No aggressive polling.

---

# 37. CODE CLEANUP

Remove:

```text
console.log
console.debug
temporary alerts
TODO placeholders that are already resolved
unused imports
unused vars
dead code
commented-out experimental code
```

Keep useful TODOs for actual V1 limitations.

---

# 38. TYPESCRIPT QUALITY

Avoid `any`.

Run strict build.

Fix unsafe casting where practical.

Ensure enum types reflect string API values.

---

# 39. API SERVICE CONSISTENCY

All services should use shared authenticated client.

No hardcoded:

```text
http://localhost:8080
```

inside services.

Use runtime config.

---

# 40. SECURITY AUDIT — FRONTEND

Ensure no:

```text
JWT secret
DB password
PasswordHash
RefreshTokenHash
Authorization header
raw access token
raw refresh token
```

is logged/rendered/documented.

Refresh token current SPA limitation may remain documented.

---

# 41. ENV AUDIT

Check:

```text
.env.example
frontend runtime config
docker-compose.yml
```

Frontend only receives public configuration such as:

```text
NUXT_PUBLIC_API_BASE_URL
```

Never expose:

```text
JWT_SECRET
DB_PASSWORD
```

to Nuxt public config.

---

# 42. API CONTRACT AUDIT

Compare FE types/services with:

```text
Docs/api/frontend-contract.md
Swagger
actual Controllers/DTOs
```

Focus especially after `JsonStringEnumConverter`.

Fix contract mismatches.

Backend implementation remains source of truth.

---

# 43. LINK AUDIT

Check all buttons/links:

```text
Dashboard quick actions
Account → Transfer
Account → Transactions
Bill → Payment
Payment → Detail
Notification shortcuts
Admin quick actions
Customer detail
Transaction detail
Payment detail
User detail
```

No dead routes.

---

# 44. KNOWN NOTIFICATION LIMITATION

Current Notification DTO does not expose referenced Payment/Transfer entity ID.

Do NOT fake deep-linking.

Document limitation:

```text
Notification currently contains text/type only and cannot deep-link to related entity.
```

Optionally propose V1.1 backend extension:

```text
EntityType
EntityId
```

but do not implement unless explicitly required.

---

# 45. API VERIFICATION — REUSE EXISTING SCRIPTS

Run:

```text
scripts/verify-fe3-api.ps1
scripts/verify-fe4-api.ps1
scripts/verify-fe5-api.ps1
```

All should pass against current backend.

Do not use Playwright.

---

# 46. CREATE FINAL FRONTEND API SCRIPT

Create:

```text
scripts/verify-fe6-api.ps1
```

Purpose:

Perform lightweight cross-module regression without mutating unnecessary data.

Suggested flow:

```text
Customer login
GET /me
GET profile
GET accounts
GET transactions
GET bills
GET payments
GET notifications

Staff login
GET admin dashboard
GET customers
GET admin transactions
GET admin payments
verify users/audit forbidden

Admin login
GET admin dashboard
GET users
GET audit logs
```

Do NOT unnecessarily transfer/pay again if FE3/FE4 scripts already verify those flows.

---

# 47. API SCRIPT SECURITY

Do not print:

```text
JWT
Refresh token
password
Authorization header
```

Use clear PASS/FAIL.

Exit non-zero on assertion failure.

---

# 48. FRONTEND BUILD

Run:

```bash
cd frontend
npm run build
```

Must PASS with 0 TypeScript errors.

---

# 49. BACKEND BUILD

Run:

```bash
dotnet build backend/LocalLink.sln
```

Expected:

```text
0 errors
```

Prefer 0 warnings or document unavoidable warnings.

---

# 50. BACKEND TESTS

Run:

```bash
dotnet test backend/LocalLink.sln
```

All tests must pass.

Report actual count.

Do not hardcode historical count.

---

# 51. DOCKER NORMAL REBUILD

Run:

```bash
docker compose up --build -d
```

Verify:

```text
locallink-web running
locallink-api healthy
locallink-sqlserver healthy
```

---

# 52. CLEAN DOCKER CLONE-LIKE VERIFICATION

Because this is final integration, perform one clean-environment test if safe.

Preferred:

```bash
docker compose down -v
docker compose up --build -d
```

ONLY after confirming local development data can be discarded.

If preserving current data is important, document and perform this in a safe temporary clone/environment instead.

Goal:

```text
empty machine-style startup
→ SQL Server
→ migrations
→ seed
→ API
→ frontend
```

---

# 53. SEED VERIFICATION

After clean setup verify development demo accounts exist:

```text
customer1@locallink.local
customer2@locallink.local
staff@locallink.local
admin@locallink.local
```

Verify demo bills exist where expected.

Do not expose passwords in production docs.

---

# 54. CLONE-AND-RUN DOCUMENTATION

Ensure new developer can:

```bash
git clone <repo>
cd LocalLink
copy .env.example .env
docker compose up --build -d
```

Windows PowerShell instructions should be documented.

No manual `.bak` import.

---

# 55. README RELEASE QUALITY

Update root README with:

```text
Project overview
Architecture
Tech stack
Features
Roles
Screens/routes
Security highlights
Financial integrity
Docker startup
Demo accounts
API docs
Development roadmap
Known limitations
```

Avoid exaggerated claims like "production banking ready".

Describe as:

```text
banking simulation / portfolio / educational platform
```

---

# 56. SRS / DOC CONSISTENCY

Review SRS and docs against implemented V1.

Update only where current implementation differs.

Do not silently invent features that were never implemented.

---

# 57. FRONTEND DOC INDEX

Ensure:

```text
Docs/frontend/
```

has a clear index or README linking:

```text
auth
dashboard
accounts
profile
transfer
transactions
bills
payments
notifications
admin dashboard
admin customers
admin transactions
admin payments
admin users
admin audit
```

---

# 58. API DOC INDEX

Ensure:

```text
Docs/api/README.md
```

links all relevant customer/admin API docs.

---

# 59. ROADMAP FINALIZATION

Set:

```text
M8 FRONTEND

FE1 ✅
FE2 ✅
FE3 ✅
FE4 ✅
FE5 ✅
FE6 ✅
```

And:

```text
Backend V1 COMPLETE
Frontend V1 COMPLETE
```

Do not mark deployment/mobile complete.

---

# 60. V1 KNOWN LIMITATIONS

Document honestly.

Examples:

```text
Simulation only
No real bank integration
No OTP/2FA
No external transfers
No card processing
No loan/credit products
Refresh token stored client-side due SPA backend contract
Notifications cannot deep-link to related entity
No realtime WebSocket notification
No payment reversal
No transaction reversal
No complex IAM/custom permissions
```

Only include limitations that are actually true.

---

# 61. NO NEW FEATURES

Do NOT implement:

```text
Flutter
Azure
Terraform
CI/CD
Prometheus
Grafana
Kafka
Redis
External banking
OTP
Loans
Cards
FX
Crypto
```

in FE6.

Those are later work.

---

# 62. MANUAL UI SMOKE CHECK

No Playwright.

Manually verify essential screens only:

### Customer

```text
/login
/dashboard
/accounts
/transfer
/transactions
/bills
/payments
/notifications
/profile
```

### Staff/Admin

```text
/admin
/admin/customers
/admin/transactions
/admin/payments
/admin/users
/admin/audit-logs
```

Focus on rendering, navigation, obvious visual defects.

---

# 63. FINAL VISUAL REVIEW

Inspect:

```text
spacing
typography
dark-theme contrast
button hierarchy
cards
tables
status chips
dialog sizes
mobile layout
empty states
loading states
```

Fix obvious portfolio-visible defects.

---

# 64. GIT STATUS

Before commit:

```bash
git status
```

Ensure no:

```text
.env
tokens
logs containing credentials
temporary API output
screenshots not intended for repo
build artifacts
```

---

# 65. COMMIT

Commit:

```bash
git add .
git commit -m "chore: finalize frontend v1 integration and polish"
```

Push:

```bash
git push -u origin feature/frontend-final-polish
```

Follow repository workflow to integrate into:

```text
feature/hoi
```

Do not automatically merge `main`.

---

# 66. DEFINITION OF DONE

FE6 is COMPLETE only when:

```text
[ ] All V1 routes audited
[ ] Broken links fixed

[ ] Auth regression passes
[ ] Customer RBAC passes
[ ] Staff RBAC passes
[ ] Admin RBAC passes

[ ] String enum handling consistent
[ ] Status labels consistent
[ ] Currency formatting consistent
[ ] Date formatting consistent

[ ] Loading states reviewed
[ ] Empty states reviewed
[ ] Error states reviewed
[ ] 401 handling verified
[ ] 403 handling verified
[ ] 404 handling verified
[ ] 409 handling verified
[ ] 429 handling verified

[ ] Transfer safety reviewed
[ ] Payment safety reviewed
[ ] Idempotency retry behavior preserved

[ ] Desktop UI reviewed
[ ] Tablet UI reviewed
[ ] Mobile UI reviewed

[ ] No major horizontal overflow
[ ] Navigation consistent
[ ] Dashboard hierarchy correct

[ ] Code cleanup complete
[ ] No debug console logs
[ ] No duplicate obvious API clients
[ ] No hardcoded API URLs
[ ] No frontend secrets

[ ] FE3 API verification PASS
[ ] FE4 API verification PASS
[ ] FE5 API verification PASS
[ ] FE6 API verification PASS

[ ] npm run build PASS
[ ] dotnet build PASS
[ ] dotnet test PASS
[ ] Docker stack healthy

[ ] Clean startup verification completed or safely documented
[ ] Seed data verified
[ ] Clone-and-run documentation verified

[ ] README finalized
[ ] Frontend docs finalized
[ ] API docs indexed
[ ] Known limitations documented
[ ] Roadmap updated

[ ] Playwright NOT USED

[ ] Git commit complete
[ ] Branch pushed

[ ] FRONTEND V1 COMPLETE
```

---

# 67. FINAL REPORT

Return:

```text
LOCALINK M8 — FE6 FINAL INTEGRATION REPORT

Branch:

Frontend V1 Status:

Routes Audited:

Broken Links Fixed:

Branding Consistency:

Enum Consistency:

Status UI Consistency:

Currency Formatting:

Date Formatting:

Loading UX:

Empty States:

Error UX:

401 Handling:

403 Handling:

404 Handling:

409 Handling:

429 Handling:

Transfer Safety Regression:

Payment Safety Regression:

Idempotency Regression:

Customer RBAC:

Staff RBAC:

Admin RBAC:

Responsive Desktop:

Responsive Tablet:

Responsive Mobile:

Accessibility Review:

Performance Cleanup:

Code Cleanup:

Security Review:

FE3 API Script:

FE4 API Script:

FE5 API Script:

FE6 API Script:

Frontend Build:

Backend Build:

Backend Tests:

Docker Normal Rebuild:

Docker Clean Startup:

Seed Verification:

Clone-and-Run Verification:

README:

Frontend Documentation:

API Documentation:

Known Limitations:

Playwright:
NOT USED

Commit:

Push:

Warnings / Remaining Issues:

FRONTEND V1:
COMPLETE / NOT COMPLETE
```

If any critical regression fails:

```text
FRONTEND V1 = NOT COMPLETE
```

Do not hide failures.

Then STOP.

Do NOT begin deployment or Flutter automatically.
