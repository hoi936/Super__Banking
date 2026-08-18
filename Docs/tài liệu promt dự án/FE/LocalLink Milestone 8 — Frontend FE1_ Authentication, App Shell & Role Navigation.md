# LocalLink Milestone 8 — Frontend FE1: Authentication, App Shell & Role Navigation

Project hiện đã hoàn thành:

```text
M1 Foundation & DevOps ✅
M2 Database Foundation ✅
M3 Authentication + JWT + RBAC ✅
M4 Customer + Bank Accounts ✅
M5 Transfer Engine ✅
M6 Bill Payment + Notifications ✅
M7 Staff/Admin + Backend V1 ✅
```

Backend V1 đã sẵn sàng để frontend consume.

Current frontend stack:

```text
Nuxt 4
Vue 3
TypeScript
Quasar
$fetch
Runtime Config
```

Backend:

```text
ASP.NET Core .NET 10
http://localhost:8080
```

Frontend:

```text
http://localhost:3000
```

Không redesign backend.

Không sửa financial business logic.

---

# 1. MỤC TIÊU FE1

Xây frontend authentication foundation hoàn chỉnh:

```text
Login Page
↓
Auth API
↓
Access Token
↓
Refresh Token
↓
Auth State
↓
Current User
↓
Route Guards
↓
Role-aware Navigation
↓
Application Layout
```

Sau FE1 user phải:

```text
Login được
Logout được
Refresh session được
Reload browser vẫn giữ session hợp lý
Không truy cập route trái quyền
Thấy navigation theo role
```

---

# 2. FEATURE BRANCH

Tạo branch:

```bash
git checkout feature/hoi
git pull origin feature/hoi
git checkout -b feature/frontend-auth
```

Nếu repository hiện đã dùng `dev` làm integration branch thì follow workflow thực tế.

Không làm trên main.

---

# 3. BACKEND CONTRACT

Đọc trước:

```text
Docs/api/frontend-contract.md
```

Và auth docs hiện có.

Không đoán response schema.

Inspect actual endpoints:

```http
POST /api/v1/auth/login
POST /api/v1/auth/refresh
POST /api/v1/auth/logout
GET  /api/v1/auth/me
```

Frontend phải follow đúng backend contract hiện tại.

---

# 4. FRONTEND STRUCTURE

Tổ chức frontend rõ ràng:

```text
frontend/
├── assets/
├── components/
├── composables/
├── layouts/
├── middleware/
├── pages/
├── plugins/
├── services/
├── stores/
├── types/
└── utils/
```

Chỉ tạo folder/file thực sự cần.

---

# 5. STATE MANAGEMENT

Nếu project chưa có Pinia:

Ưu tiên dùng:

```text
@pinia/nuxt
```

cho auth state.

Tạo:

```text
stores/auth.ts
```

State tối thiểu:

```text
user
accessToken
refreshToken
isAuthenticated
isLoading
```

Không tạo store khổng lồ.

---

# 6. TOKEN STORAGE STRATEGY

Ưu tiên security hợp lý cho portfolio.

Nếu backend hiện trả refresh token trong JSON và chưa dùng HttpOnly cookie:

Có thể lưu token client-side theo chiến lược hiện tại, nhưng phải document limitation.

Ưu tiên:

```text
Access Token
→ memory / Pinia

Refresh Token
→ secure client persistence theo khả năng hiện tại
```

Nếu cần persistence bằng cookie:

sử dụng Nuxt cookie APIs.

Không tự chuyển backend sang cookie auth trong FE1 nếu backend contract hiện không hỗ trợ.

---

# 7. TOKEN SECURITY

Không log:

```text
access token
refresh token
password
```

ra console.

Không render token trong UI.

Không lưu token vào source code.

---

# 8. AUTH SERVICE

Tạo:

```text
services/auth.ts
```

Functions:

```text
login()
refresh()
logout()
getCurrentUser()
```

Reuse API client hiện tại.

Không duplicate `$fetch` config khắp app.

---

# 9. API CLIENT

Review:

```text
services/api.ts
```

Chuẩn hóa request helper để tự attach:

```http
Authorization: Bearer <accessToken>
```

cho authenticated request.

Không hardcode API URL.

Dùng:

```text
useRuntimeConfig().public.apiBaseUrl
```

---

# 10. AUTOMATIC REFRESH

Khi API trả:

```text
401 Unauthorized
```

và refresh token tồn tại:

```text
Attempt refresh once
↓
Receive new access token
↓
Retry original request once
```

Không tạo infinite refresh loop.

Nếu refresh fail:

```text
clear auth state
redirect /login
```

---

# 11. CONCURRENT 401 HANDLING

Nếu nhiều requests đồng thời trả 401:

Không gửi 5 refresh requests song song nếu có thể tránh.

Implement basic refresh lock / shared refresh promise.

Keep implementation simple.

---

# 12. LOGIN PAGE

Tạo:

```text
/pages/login.vue
```

UI phải đẹp và phù hợp banking app.

Fields:

```text
Email
Password
```

Actions:

```text
Login
```

Có:

```text
loading state
validation error
invalid credentials message
```

---

# 13. LOGIN VALIDATION

Validate frontend:

```text
Email required
Email format
Password required
```

Không over-engineer.

Backend vẫn là source of truth.

---

# 14. DEVELOPMENT DEMO ACCOUNTS

Có thể hiển thị một section nhỏ:

```text
Demo Accounts
```

Development only:

```text
Customer:
customer1@locallink.local

Staff:
staff@locallink.local

Admin:
admin@locallink.local
```

Không hiển thị demo credentials nếu build production mode.

Nếu hiển thị password demo:

Ghi rõ:

```text
Development only
```

---

# 15. LOGIN FLOW

Flow:

```text
User enters credentials
↓
POST /auth/login
↓
Save tokens
↓
GET /auth/me
↓
Store current user
↓
Redirect based on role
```

Role routing:

```text
CUSTOMER → /dashboard

STAFF → /admin

ADMIN → /admin
```

---

# 16. AUTH INITIALIZATION

Khi app load:

```text
Check persisted auth state
↓
If token exists
↓
GET /auth/me
↓
If valid → restore session
↓
If expired → refresh
↓
If refresh fails → logout state
```

Không để app flicker sang protected page trước khi auth check xong.

---

# 17. ROUTE MIDDLEWARE

Tạo:

```text
middleware/auth.ts
middleware/guest.ts
middleware/customer.ts
middleware/admin.ts
```

hoặc cấu trúc tương đương.

Rules:

```text
/login
→ guest only

/dashboard
→ CUSTOMER

/accounts
→ CUSTOMER

/transfers
→ CUSTOMER

/transactions
→ CUSTOMER

/bills
→ CUSTOMER

/notifications
→ CUSTOMER

/admin/*
→ STAFF or ADMIN
```

---

# 18. AUTHORIZATION

Frontend route guard chỉ phục vụ UX.

Không coi frontend guard là security boundary.

Backend RBAC vẫn là authoritative.

Document điều này.

---

# 19. DEFAULT LAYOUT

Tạo:

```text
layouts/default.vue
```

Customer app shell gồm:

```text
Top bar
Sidebar / drawer
Main content
User menu
Notification shortcut
Logout
```

Navigation:

```text
Dashboard
Accounts
Transfer
Transactions
Bills
Notifications
Profile
```

---

# 20. ADMIN LAYOUT

Tạo:

```text
layouts/admin.vue
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

Role behavior:

```text
STAFF
→ hide Users
→ hide Audit Logs

ADMIN
→ show all
```

Frontend navigation phải dựa trên auth roles.

---

# 21. MOBILE RESPONSIVENESS

Sidebar trên desktop.

Drawer/hamburger trên small screen.

Không cần Flutter ở milestone này.

Nuxt web phải responsive tốt.

---

# 22. USER HEADER

Hiển thị:

```text
Full Name
Role
Avatar placeholder
```

Ví dụ:

```text
Nguyen Van An
CUSTOMER
```

Admin:

```text
Administrator
ADMIN
```

---

# 23. LOGOUT

User chọn Logout:

```text
POST /auth/logout
↓
Clear tokens
↓
Clear user
↓
Redirect /login
```

Nếu backend logout fail do token already revoked:

Frontend vẫn nên clear local auth state.

---

# 24. AUTH STORE ACTIONS

Auth store nên có:

```text
login
logout
refreshSession
fetchCurrentUser
initializeAuth
hasRole
```

Không expose quá nhiều mutable state ra ngoài.

---

# 25. TYPE DEFINITIONS

Tạo:

```text
types/auth.ts
```

Có:

```text
UserRole
CurrentUser
LoginRequest
LoginResponse
TokenResponse
```

Follow backend contract chính xác.

---

# 26. API ERROR HANDLING

Tạo helper để parse ProblemDetails.

Ví dụ:

```text
title
detail
status
errorCode
```

UI không hiển thị raw stack trace.

---

# 27. GLOBAL ERROR UX

Có component:

```text
components/common/AppAlert.vue
```

hoặc equivalent.

Dùng cho:

```text
API error
Validation
Forbidden
Network issue
```

---

# 28. LOADING UX

Tạo consistent loading states.

Ví dụ:

```text
QSpinner
QLinearProgress
Skeleton
```

Không để button login có thể double-submit.

---

# 29. 401 HANDLING

401:

```text
refresh if possible
```

Nếu refresh fail:

```text
logout
redirect /login
```

---

# 30. 403 HANDLING

403:

Hiển thị:

```text
Bạn không có quyền truy cập chức năng này.
```

Có thể redirect về dashboard phù hợp role.

---

# 31. 404 PAGE

Tạo basic:

```text
pages/[...slug].vue
```

hoặc Nuxt error page convention.

Không cần elaborate.

---

# 32. DASHBOARD PLACEHOLDER

Tạo:

```text
/pages/dashboard.vue
```

CUSTOMER only.

Hiện tại chỉ cần:

```text
Welcome
Current user
Backend connected
```

Không implement full banking dashboard trong FE1.

---

# 33. ADMIN PLACEHOLDER

Tạo:

```text
/pages/admin/index.vue
```

STAFF/ADMIN.

Chỉ cần:

```text
Admin Dashboard
Current role
Backend status
```

Full Admin dashboard làm FE5.

---

# 34. PROFILE PLACEHOLDER

Tạo:

```text
/pages/profile.vue
```

Có thể hiển thị current user basic info.

Không implement full profile editing nếu muốn giữ scope FE1.

---

# 35. THEME

Giữ visual direction hiện tại:

```text
Dark / modern banking
Premium but clean
```

Không cần quá nhiều animation.

Ưu tiên:

```text
readability
spacing
hierarchy
responsive
```

---

# 36. BRANDING

Project hiện có naming LocalLink / InterLink Banking.

Không tự đổi toàn bộ source code.

Frontend visible branding có thể dùng:

```text
InterLink Banking
```

nếu đây là brand UI hiện tại.

README/source project name vẫn LocalLink nếu chưa quyết định rename.

Ghi warning nếu naming chưa thống nhất.

---

# 37. QUASAR

Reuse Quasar.

Không cài thêm UI framework khác.

Sử dụng:

```text
QLayout
QDrawer
QHeader
QPage
QCard
QInput
QBtn
QIcon
QMenu
```

khi phù hợp.

---

# 38. ACCESSIBILITY

Form fields có label.

Buttons có accessible text.

Keyboard tab navigation phải usable.

Không dùng icon-only controls nếu không có aria-label/tooltip.

---

# 39. FRONTEND TESTS

Nếu project chưa có test setup:

Không cần dựng testing ecosystem quá lớn.

Ít nhất verify bằng build + E2E/manual browser.

Nếu test framework hiện có, add auth store tests.

---

# 40. AUTH TEST CASES

Verify:

```text
Valid customer login
Invalid password
Unknown account
Admin login
Staff login
Logout
Expired access token → refresh
Invalid refresh token → logout
Unauthenticated protected route → login
CUSTOMER → admin route denied
STAFF → admin route allowed
ADMIN → admin route allowed
```

---

# 41. REFRESH TEST

Login.

Force access token expiry if test setup allows.

Call authenticated API.

Expected:

```text
401
↓
refresh
↓
retry
↓
200
```

No duplicate user action.

---

# 42. BROWSER RELOAD TEST

Login.

Reload:

```text
F5
```

Expected:

```text
session restored
user remains authenticated
```

unless current token storage policy explicitly chooses non-persistence.

Document behavior.

---

# 43. LOGOUT TEST

Login.

Logout.

Reload.

Expected:

```text
/login
```

No stale session.

---

# 44. ROLE NAV TEST

Customer:

```text
Customer menu visible
Admin menu hidden
```

Staff:

```text
Admin operational menu visible
Users hidden
Audit logs hidden
```

Admin:

```text
all admin menu visible
```

---

# 45. NETWORK ERROR

If backend unavailable:

Login should display friendly message:

```text
Không thể kết nối đến máy chủ.
```

No crash.

---

# 46. DOCKER

Frontend Docker build must continue working.

Run:

```bash
npm run build
```

Then:

```bash
docker compose up --build -d
```

Verify:

```text
frontend healthy
backend healthy
sqlserver healthy
```

---

# 47. NO BACKEND REGRESSION

Run:

```bash
dotnet test backend/LocalLink.sln
```

Existing backend tests must still pass.

Frontend changes must not break backend.

---

# 48. API BASE URL

Keep:

```text
NUXT_PUBLIC_API_BASE_URL
```

Do not hardcode localhost inside services.

---

# 49. FRONTEND ENV

Review:

```text
.env.example
```

Ensure frontend variable exists:

```text
NUXT_PUBLIC_API_BASE_URL=http://localhost:8080
```

No secret frontend env vars.

JWT secret must NEVER be exposed to Nuxt public runtime config.

---

# 50. SECURITY

Verify:

```text
No JWT secret in frontend
No database password in frontend
No password logging
No token console logging
No backend internal stack traces rendered
```

---

# 51. DOCUMENTATION

Update:

```text
README.md
Docs/README.md
```

Add FE1 status.

Create if useful:

```text
Docs/frontend/auth.md
```

Document:

```text
Auth flow
Token strategy
Refresh strategy
Route guards
Role navigation
Logout behavior
```

---

# 52. FRONTEND STATUS

After FE1:

```text
M8 Frontend

FE1 Auth + App Shell         ✅
FE2 Customer Dashboard       TODO
FE3 Transfer + Transactions  TODO
FE4 Bills + Notifications    TODO
FE5 Admin/Staff UI           TODO
```

---

# 53. GIT

Verify:

```bash
git status
```

No secrets.

Commit:

```bash
git add .
git commit -m "feat: implement frontend authentication shell"
```

Push:

```bash
git push -u origin feature/frontend-auth
```

Follow existing workflow to integrate into `feature/hoi`.

Do not merge main automatically.

---

# 54. DEFINITION OF DONE

FE1 only complete when:

```text
[ ] Login page works
[ ] Customer login works
[ ] Staff login works
[ ] Admin login works

[ ] Access token attached to API requests
[ ] Refresh token flow works
[ ] Refresh retry does not loop infinitely

[ ] Current user loads
[ ] Auth state initializes on app load
[ ] Reload behavior works

[ ] Logout works
[ ] Logout clears local session

[ ] CUSTOMER routes protected
[ ] STAFF/ADMIN routes protected
[ ] Role navigation correct

[ ] Customer layout works
[ ] Admin layout works
[ ] Responsive drawer works

[ ] 401 handled
[ ] 403 handled
[ ] Network error handled

[ ] API URL comes from runtime config
[ ] No frontend secrets

[ ] npm build passes
[ ] Docker build passes
[ ] Backend regression tests pass

[ ] Swagger/backend unaffected
[ ] Documentation updated
[ ] Branch pushed
```

---

# 55. FINAL REPORT

Return:

```text
LOCALINK M8 — FE1 AUTH FRONTEND REPORT

Branch:

Nuxt Version:

State Management:

Token Storage Strategy:

Access Token Strategy:

Refresh Token Strategy:

Automatic Refresh:

Concurrent Refresh Handling:

Auth Store:

Auth Service:

API Client:

Login Page:

Customer Layout:

Admin Layout:

Route Middleware:

Role Navigation:

401 Handling:

403 Handling:

Reload Persistence:

Logout:

Responsive Result:

Build Result:

Docker Result:

Backend Regression:

Files Created:

Documentation:

Commit:

Push:

Warnings / Known Limitations:

FE1 STATUS:
COMPLETE / NOT COMPLETE
```

Then STOP.

Do not implement FE2 automatically.