# LocalLink / InterLink Banking V1 — FINAL RELEASE VERIFICATION

## Mục tiêu

Đây là bước cuối cùng của LocalLink / InterLink Banking Web V1.

KHÔNG phát triển thêm major feature.
KHÔNG dùng Playwright.
KHÔNG bắt đầu Flutter, Azure, Terraform, CI/CD hoặc monitoring.

Nhiệm vụ là:

1. Audit toàn bộ Backend + Frontend + Database + Docker.
2. Hoàn thành các checkbox FE6 còn thiếu.
3. Sửa các regression/bug thực sự phát hiện được.
4. Chạy toàn bộ verification.
5. Chỉ đánh dấu V1 `RELEASE READY` khi tất cả critical checks PASS.

---

# 1. Git Safety

Kiểm tra trước:

```bash
git status
git branch
git log -5 --oneline
```

Làm việc từ code mới nhất của:

```text
feature/hoi
```

Tạo branch:

```bash
git checkout feature/hoi
git pull origin feature/hoi
git checkout -b release/v1-final-verification
```

Không xóa hoặc overwrite code đã hoàn thành.

Không merge vào `main` tự động.

---

# 2. Source Audit

Audit:

```text
backend/
frontend/
Docs/
scripts/
docker-compose.yml
.env.example
README.md
.gitignore
```

Frontend đặc biệt kiểm tra:

```text
layouts/
pages/
components/
composables/
services/
stores/
types/
utils/
middleware/
plugins/
```

Tìm và xử lý:

```text
console.log/debug
dead code
unused imports
placeholder text
hardcoded localhost API URL
duplicate API clients
unsafe any
broken imports
temporary TODO đã giải quyết
test credentials/token bị ghi vào source
```

Không refactor kiến trúc lớn nếu không cần thiết.

---

# 3. Branding

UI public phải thống nhất:

```text
InterLink Banking
```

Technical/repository name có thể giữ:

```text
LocalLink
```

Không đổi namespace .NET hoặc database chỉ vì branding.

---

# 4. Route Audit

Xác minh:

## Public

```text
/
/login
```

## CUSTOMER

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

## STAFF / ADMIN

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

Kiểm tra toàn bộ sidebar, header, quick actions và detail links.

Không để dead route.

---

# 5. Authentication Regression

Xác minh:

```text
CUSTOMER login
STAFF login
ADMIN login
logout
session restore sau F5
access token refresh
invalid/expired refresh token
guest middleware
auth middleware
```

Yêu cầu:

```text
refresh thành công → retry request tối đa 1 lần
refresh thất bại → clear session → /login
không refresh loop
```

---

# 6. RBAC Matrix

## CUSTOMER

Phải:

```text
truy cập customer UI
không truy cập /admin/*
```

## STAFF

Được:

```text
/admin
/admin/customers
/admin/transactions
/admin/payments
```

Không được:

```text
/admin/users
/admin/audit-logs
ADMIN-only mutations
```

## ADMIN

Được toàn bộ chức năng admin đã implement.

Backend authorization là source of truth.

Frontend middleware chỉ là lớp UX/security bổ sung.

---

# 7. HTTP Error Audit

Xác minh UX cho:

```text
400
401
403
404
409
429
500/network error
```

### 401

Refresh session đúng logic.

### 403

Hiển thị/redirect thân thiện.

### 404

Không leak thông tin ownership.

### 409

Đặc biệt:

```text
transfer idempotency
payment idempotency
concurrency conflict
duplicate beneficiary
```

### 429

Hiển thị:

```text
Bạn đang thao tác quá nhanh. Vui lòng thử lại sau.
```

Không aggressive auto retry.

---

# 8. Enum Contract

Backend dùng:

```text
JsonStringEnumConverter
```

Frontend phải xử lý string enum nhất quán.

Audit các giá trị:

```text
ACTIVE
SUSPENDED
LOCKED
CLOSED
PENDING
COMPLETED
FAILED
TRANSFER
PAYMENT
UNPAID
PAID
OVERDUE
CANCELLED
```

Không còn logic dựa trên integer enum cũ.

---

# 9. Status Utility

Kiểm tra `frontend/utils/status.ts`.

Các page/component phải ưu tiên dùng helper chung thay vì tự tạo lại:

```text
getStatusLabel
getStatusColor
getStatusIcon
```

hoặc API tương đương hiện có.

Không ép refactor nếu component có semantic đặc biệt hợp lý.

---

# 10. Currency Standardization

Audit toàn FE.

Dùng formatter chung.

Ví dụ:

```text
25.000.000 ₫
850.000 ₫
```

Không hiển thị raw:

```text
25000000
```

trong user-facing UI trừ trường hợp kỹ thuật cần thiết.

---

# 11. Date Standardization

Dùng utility chung.

Ưu tiên:

```text
18/08/2026
18/08/2026 23:15
```

Không hiển thị raw UTC ISO nếu không cần.

---

# 12. Loading States

Audit tất cả data page.

Không để blank UI trong lúc loading.

Sử dụng Quasar component phù hợp:

```text
QSkeleton
QSpinner
QInnerLoading
```

---

# 13. Empty States

Danh sách phải có empty state:

```text
Accounts
Transactions
Bills
Payments
Notifications
Beneficiaries
Admin Customers
Admin Transactions
Admin Payments
Admin Users
Audit Logs
```

Nếu có filter, phân biệt hợp lý:

```text
Không có dữ liệu
```

và

```text
Không tìm thấy kết quả phù hợp
```

---

# 14. Responsive Verification

KHÔNG dùng Playwright.

Manual inspection/safe dev tooling nếu cần.

Kiểm tra ít nhất:

```text
Desktop ~1440px
Tablet ~768px
Mobile ~375px
```

Customer flows ưu tiên cao nhất.

Kiểm tra:

```text
sidebar/drawer
header
cards
forms
tables
pagination
dialogs
stepper
transaction/payment receipt
```

Không có essential horizontal overflow trên mobile.

---

# 15. Accessibility Basics

Kiểm tra:

```text
input labels
button labels
icon-only tooltip/aria label khi phù hợp
keyboard usability
focus visibility
readable contrast
status không phụ thuộc duy nhất vào màu
```

Không yêu cầu full WCAG certification.

---

# 16. Transfer Safety Regression

Xác minh frontend transfer:

```text
amount > 0
source account ACTIVE
destination lookup
own-account prevention
insufficient balance UX
confirmation before submit
loading/disable while submit
```

Idempotency:

```text
1 logical transfer = 1 UUID
network retry = reuse SAME UUID
new transfer = new UUID
```

Sau thành công:

```text
refetch balance
refetch transaction/history where applicable
```

Không tự tính authoritative balance client-side.

---

# 17. Payment Safety Regression

Xác minh:

```text
Bill amount không do client tự sửa
ACTIVE account only
insufficient balance handling
already-paid handling
double-submit blocked
```

Idempotency:

```text
1 logical payment = 1 UUID
ambiguous retry = SAME UUID
new payment = new UUID
```

Sau thành công:

```text
refetch bill
refetch account/balance
refetch payment/history
refresh notification count
```

---

# 18. Ambiguous Financial Network Failure

Nếu POST transfer/payment mất kết nối sau khi gửi:

KHÔNG hiển thị chắc chắn:

```text
Giao dịch thất bại
```

nếu không biết server đã commit hay chưa.

Thông báo nên hướng user kiểm tra history.

Retry phải reuse idempotency key.

---

# 19. Notification Regression

Xác minh:

```text
list
unread filter
unread count
mark read
mark all read
shared badge
```

Không aggressive polling.

Current limitation:

```text
Notification chưa có EntityId để deep-link chính xác
```

Không fake deep link.

---

# 20. Admin Mutation Safety

Kiểm tra:

```text
Suspend/Activate Customer
Lock/Unlock Account
Suspend/Activate User
Self-suspension protection
```

Buttons phải:

```text
confirmation nếu cần
loading
disable khi request đang chạy
friendly success/error feedback
```

STAFF không thấy/không dùng ADMIN-only action.

---

# 21. API Service Audit

Tất cả service phải dùng shared API client.

Không hardcode:

```text
http://localhost:8080
```

trong từng service.

Dùng:

```text
runtimeConfig.public.apiBaseUrl
```

hoặc abstraction hiện tại.

---

# 22. Frontend Security Audit

Không được log/render:

```text
access token
refresh token
Authorization header
PasswordHash
RefreshTokenHash
DB credentials
JWT signing secret
```

Kiểm tra browser-facing runtime config không chứa server secret.

---

# 23. Environment Audit

`.env.example` phải đủ để clone-and-run.

Kiểm tra Docker env.

Public frontend env chỉ chứa config an toàn như:

```text
NUXT_PUBLIC_API_BASE_URL
```

Không đưa:

```text
DB_PASSWORD
JWT_SECRET
```

vào public runtime config.

---

# 24. Existing Regression Scripts

Chạy:

```powershell
.\scripts\verify-fe3-api.ps1
.\scripts\verify-fe4-api.ps1
.\scripts\verify-fe5-api.ps1
.\scripts\verify-fe6-api.ps1
```

Nếu script phụ thuộc clean seed state, xử lý/document rõ.

Không sửa assertion chỉ để làm test xanh nếu behavior thực tế sai.

---

# 25. Backend Tests

Chạy:

```bash
dotnet test backend/LocalLink.sln
```

Yêu cầu:

```text
Failed: 0
```

Report actual test count.

Không assume luôn là 64 nếu code thay đổi.

---

# 26. Backend Build

Chạy:

```bash
dotnet build backend/LocalLink.sln
```

Yêu cầu:

```text
0 errors
```

Document warning nếu có.

---

# 27. Frontend Build

Chạy:

```bash
cd frontend
npm run build
```

Yêu cầu:

```text
PASS
0 TypeScript build errors
```

---

# 28. Docker Full Stack

Từ project root:

```bash
docker compose up --build -d
docker compose ps
```

Xác minh:

```text
SQL Server healthy
API healthy
Frontend running
```

Health endpoint phải PASS.

---

# 29. Clean Startup Test

Đây là release verification nên cần test gần giống máy mới.

Nếu local DB chỉ là demo/dev data và an toàn xóa:

```bash
docker compose down -v
docker compose up --build -d
```

Nếu có dữ liệu cần giữ:

KHÔNG xóa volume.

Thay vào đó tạo temporary clean clone/environment và document.

Mục tiêu:

```text
empty DB
→ SQL Server start
→ EF migrations
→ seed
→ API healthy
→ frontend running
```

---

# 30. Seed Verification

Sau clean setup, xác minh development seed hoạt động.

Expected demo identities:

```text
customer1@locallink.local
customer2@locallink.local
staff@locallink.local
admin@locallink.local
```

Verify demo bills/accounts required by E2E scripts.

Không đưa demo credentials vào production config.

---

# 31. Clone-and-Run Test

README phải cho developer mới chạy được bằng flow gần như:

```bash
git clone <repo>
cd <repo>
copy .env.example .env
docker compose up --build -d
```

Nếu Windows:

```powershell
Copy-Item .env.example .env
docker compose up --build -d
```

Không yêu cầu import `.bak`.

Database phải được tạo qua:

```text
EF Core migrations + seed
```

---

# 32. Documentation Audit

Review:

```text
README.md
Docs/README.md
Docs/api/
Docs/frontend/
Docs/database.md
SRS nếu tồn tại
```

Docs phải phản ánh implementation thực tế.

Không mô tả feature chưa có như đã hoàn thành.

---

# 33. Product Positioning

README không được tuyên bố đây là real production bank.

Dùng wording:

```text
Banking simulation platform
Portfolio project
Educational / demonstration banking system
```

Có thể nói architecture hướng enterprise, nhưng không tuyên bố regulatory/production certification.

---

# 34. Known Limitations

Document chính xác các limitation còn tồn tại.

Ví dụ nếu đúng với code:

```text
No real banking network integration
No OTP / 2FA
No external interbank transfer
No card processing
No loans
No transaction reversal
No payment reversal
No realtime WebSocket notification
Notification cannot deep-link to related entity
SPA refresh token storage limitation
```

Không thêm limitation không đúng.

---

# 35. Final Git Security Check

Chạy:

```bash
git status
git diff --check
```

Kiểm tra không commit:

```text
.env
real credentials
JWTs
refresh tokens
DB data
logs chứa token
node_modules
bin/
obj/
.nuxt/
.output/
```

---

# 36. Final Commit

Nếu có fix trong Final Verification:

```bash
git add .
git commit -m "chore: finalize v1 release verification"
```

Push:

```bash
git push -u origin release/v1-final-verification
```

Sau khi verify, tích hợp theo workflow hiện tại về:

```text
feature/hoi
```

Không tự merge `main` nếu chưa được yêu cầu.

---

# 37. RELEASE GATE

Chỉ được kết luận:

```text
LOCALINK / INTERLINK BANKING V1 = RELEASE READY
```

khi tất cả critical gate sau PASS:

```text
[ ] Frontend build PASS
[ ] Backend build PASS
[ ] Backend tests PASS

[ ] FE3 API regression PASS
[ ] FE4 API regression PASS
[ ] FE5 API regression PASS
[ ] FE6 API regression PASS

[ ] Docker API healthy
[ ] Docker SQL Server healthy
[ ] Docker frontend running

[ ] Clean startup PASS
[ ] EF migrations PASS
[ ] Seed PASS

[ ] Customer authentication PASS
[ ] Staff authentication PASS
[ ] Admin authentication PASS

[ ] CUSTOMER RBAC PASS
[ ] STAFF RBAC PASS
[ ] ADMIN RBAC PASS

[ ] Transfer idempotency regression PASS
[ ] Payment idempotency regression PASS

[ ] Double-submit protection PASS
[ ] 401 behavior PASS
[ ] 403 behavior PASS
[ ] 404 behavior PASS
[ ] 409 behavior PASS
[ ] 429 behavior PASS

[ ] Loading UX reviewed
[ ] Empty UX reviewed
[ ] Error UX reviewed

[ ] Desktop responsive reviewed
[ ] Tablet responsive reviewed
[ ] Mobile responsive reviewed

[ ] Currency formatting consistent
[ ] Date formatting consistent
[ ] Status formatting consistent

[ ] No critical dead routes
[ ] No critical broken navigation

[ ] No frontend secrets
[ ] No secrets committed to Git

[ ] README accurate
[ ] Docs accurate
[ ] Known limitations documented

[ ] Playwright NOT USED
```

Nếu critical item fail:

```text
V1 STATUS = NOT RELEASE READY
```

Fix lỗi rồi chạy lại relevant verification.

Không che giấu lỗi.

---

# 38. FINAL REPORT FORMAT

Khi hoàn tất, trả về đúng format:

```text
LOCALINK / INTERLINK BANKING
V1 FINAL RELEASE VERIFICATION REPORT

Branch:

Commit:

Push:

========================================
1. SOURCE QUALITY
========================================

Frontend Audit:
Backend Audit:
Dead Code:
Console Logs:
Hardcoded URLs:
TypeScript Quality:
Branding:

========================================
2. AUTH & RBAC
========================================

Customer Login:
Staff Login:
Admin Login:
Session Restore:
Token Refresh:
Logout:

CUSTOMER RBAC:
STAFF RBAC:
ADMIN RBAC:

========================================
3. ERROR HANDLING
========================================

400:
401:
403:
404:
409:
429:
Network/500:

========================================
4. FINANCIAL SAFETY
========================================

Transfer Validation:
Transfer Idempotency:
Transfer Double Submit:
Transfer Balance Refresh:

Payment Validation:
Payment Idempotency:
Payment Double Submit:
Payment Balance Refresh:

Ambiguous Network Handling:

========================================
5. FRONTEND UX
========================================

Loading States:
Empty States:
Error States:

Status Formatting:
Currency Formatting:
Date Formatting:

Desktop:
Tablet:
Mobile:

Accessibility Basics:

========================================
6. API REGRESSION
========================================

FE3 Script:
FE4 Script:
FE5 Script:
FE6 Script:

========================================
7. BUILD & TEST
========================================

Frontend Build:

Backend Build:

Backend Tests:
Passed:
Failed:
Skipped:

========================================
8. DOCKER & DATABASE
========================================

SQL Server:
API:
Frontend:

Health Check:

Clean Startup:

EF Migrations:

Seeder:

Volume Persistence:

========================================
9. CLONE-AND-RUN
========================================

.env.example:
Docker Instructions:
Fresh Database Creation:
Demo Seed:
Result:

========================================
10. DOCUMENTATION
========================================

README:
Docs:
API Docs:
Frontend Docs:
Database Docs:
Known Limitations:

========================================
11. SECURITY
========================================

Frontend Secrets:
Git Secrets:
Token Logging:
Public Runtime Config:
Security Result:

========================================
12. KNOWN LIMITATIONS
========================================

- ...

========================================
FINAL RELEASE GATE
========================================

Backend V1:
COMPLETE / NOT COMPLETE

Frontend V1:
COMPLETE / NOT COMPLETE

Database:
READY / NOT READY

Docker:
READY / NOT READY

API Regression:
PASS / FAIL

Unit Tests:
PASS / FAIL

Financial Safety:
PASS / FAIL

RBAC:
PASS / FAIL

Documentation:
PASS / FAIL

Playwright:
NOT USED

----------------------------------------

LOCALINK / INTERLINK BANKING V1:

RELEASE READY
```

Nếu chưa đạt, thay dòng cuối bằng:

```text
NOT RELEASE READY
```

và liệt kê blocker cụ thể.

---

# 39. STOP CONDITION

Sau khi tạo Final Report:

STOP.

Không tự động:

```text
deploy Azure
merge main
create GitHub Release
create tag
start Flutter
start CI/CD
add new banking features
```

Các việc đó thuộc phase sau V1 và cần yêu cầu riêng từ user.
