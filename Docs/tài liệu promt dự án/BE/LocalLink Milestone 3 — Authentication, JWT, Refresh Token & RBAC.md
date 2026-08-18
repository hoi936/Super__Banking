# LocalLink Milestone 3 — Authentication, JWT, Refresh Token & RBAC

Bạn đang làm việc trên project **LocalLink** đã hoàn thành:

```text
Milestone 1
Foundation ✅

Milestone 2
Complete Database Foundation ✅
```

Project hiện sử dụng:

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

Database hiện đã có các bảng Identity:

```text
Users
Roles
UserRoles
RefreshTokens
Customers
```

Không tạo lại database architecture.

Không reset migrations.

Không thay đổi tech stack.

---

# 1. MỤC TIÊU MILESTONE

Triển khai authentication foundation hoàn chỉnh:

```text
Password Hashing
        ↓
Login
        ↓
JWT Access Token
        ↓
Refresh Token
        ↓
Token Rotation / Revocation
        ↓
RBAC
        ↓
Current User
        ↓
Logout
        ↓
Audit Events
```

Sau milestone này API phải có thể xác thực user thật.

---

# 2. KHÔNG LÀM TRONG MILESTONE NÀY

Không implement:

```text
Customer Management API
Bank Account API
Transfer
Transaction History
Payment
Bill
Admin Dashboard
Frontend Dashboard
```

Chỉ làm Authentication & Authorization.

---

# 3. FEATURE BRANCH

Bắt đầu từ:

```text
dev
```

Chạy:

```bash
git checkout dev
git pull origin dev
git checkout -b feature/auth
```

Không làm trực tiếp trên `main`.

---

# 4. AUTH ENDPOINTS

Tạo:

```http
POST /api/v1/auth/login
POST /api/v1/auth/refresh
POST /api/v1/auth/logout
GET  /api/v1/auth/me
```

Có thể thêm:

```http
POST /api/v1/auth/revoke-all
```

nếu implementation gọn và hợp lý.

Không tạo Register public trong milestone này.

User demo đã được seed từ database.

---

# 5. LOGIN REQUEST

Request:

```json
{
  "email": "customer1@locallink.local",
  "password": "..."
}
```

Validate:

```text
Email required
Password required
Valid email format
```

---

# 6. LOGIN FLOW

Flow bắt buộc:

```text
Receive email/password
        ↓
Find User by normalized email
        ↓
Check User exists
        ↓
Check User Status
        ↓
Verify Password Hash
        ↓
Load User Roles
        ↓
Generate JWT Access Token
        ↓
Generate secure Refresh Token
        ↓
Hash Refresh Token
        ↓
Persist RefreshToken
        ↓
Audit successful login
        ↓
Return response
```

Nếu login thất bại:

```text
Invalid credentials
Locked/suspended user
```

ghi AuditLog phù hợp.

Không tiết lộ:

```text
"user does not exist"
"wrong password"
```

trong response public.

Sử dụng message chung:

```text
Invalid email or password.
```

---

# 7. PASSWORD HASHING

Không dùng:

```text
SHA256(password)
MD5
plain text
custom crypto
```

Sử dụng cơ chế password hashing chuẩn của ASP.NET Core.

Ưu tiên:

```text
PasswordHasher<TUser>
```

hoặc abstraction hợp lý dựa trên:

```text
Microsoft.AspNetCore.Identity.IPasswordHasher<TUser>
```

Không bắt buộc sử dụng full ASP.NET Core Identity database schema.

Giữ database hiện tại.

---

# 8. UPDATE SEED PASSWORDS

M2 có user seed:

```text
admin@locallink.local
customer1@locallink.local
customer2@locallink.local
```

Update development seeder để password hash được sinh bằng password hasher thật.

Development demo password có thể là:

```text
LocalLink@123
```

cho cả 3 demo users nếu muốn đơn giản hóa demo.

Password chỉ được document là:

```text
development-only credentials
```

Không dùng cho production.

Không lưu plain password trong database.

---

# 9. JWT ACCESS TOKEN

Implement JWT bearer authentication.

JWT phải chứa tối thiểu:

```text
sub
email
roles
jti
iat
```

Có thể thêm:

```text
customer_id
```

chỉ nếu user có Customer profile và cần thiết.

Không nhét object lớn vào token.

---

# 10. JWT CONFIGURATION

Configuration:

```text
Jwt:
  Issuer
  Audience
  Secret
  AccessTokenMinutes
  RefreshTokenDays
```

Không hardcode secret.

Sử dụng environment variable.

Ví dụ:

```text
JWT_SECRET
JWT_ISSUER
JWT_AUDIENCE
JWT_ACCESS_TOKEN_MINUTES
JWT_REFRESH_TOKEN_DAYS
```

Update:

```text
.env.example
```

Không commit real secret.

---

# 11. JWT SECRET

Local development secret phải đủ dài.

Không sử dụng:

```text
secret
password
123456
locallink
```

`.env.example` dùng placeholder rõ ràng.

Ví dụ:

```text
JWT_SECRET=CHANGE_ME_WITH_A_LONG_RANDOM_SECRET_FOR_LOCAL_DEVELOPMENT
```

---

# 12. ACCESS TOKEN EXPIRATION

Development mặc định:

```text
15 minutes
```

Refresh Token:

```text
7 days
```

Các giá trị phải configurable.

---

# 13. REFRESH TOKEN GENERATION

Refresh Token phải dùng cryptographically secure random generator.

Không dùng:

```text
Guid.NewGuid().ToString()
Random()
timestamp
```

làm refresh token chính.

Ưu tiên:

```text
RandomNumberGenerator
```

Token raw chỉ trả về client.

Database lưu:

```text
TokenHash
```

Không lưu raw token.

---

# 14. REFRESH TOKEN HASH

Sử dụng cryptographic hash phù hợp:

```text
SHA-256
```

cho random refresh token.

Điều này khác với password hashing.

Password:

```text
PasswordHasher
```

Refresh token random secret:

```text
SHA-256 hash
```

là chấp nhận được.

---

# 15. REFRESH FLOW

Endpoint:

```http
POST /api/v1/auth/refresh
```

Input:

```json
{
  "refreshToken": "..."
}
```

Flow:

```text
Hash presented token
        ↓
Find RefreshToken by hash
        ↓
Validate:
- exists
- not expired
- not revoked
        ↓
Load User
        ↓
Check User ACTIVE
        ↓
Revoke old refresh token
        ↓
Generate new Access Token
        ↓
Generate new Refresh Token
        ↓
Save new token
        ↓
Return new token pair
```

Sử dụng refresh token rotation.

Một refresh token đã dùng không được sử dụng lần thứ hai.

---

# 16. REFRESH TOKEN REUSE

Nếu token:

```text
revoked
expired
unknown
```

return:

```text
401 Unauthorized
```

Không trả chi tiết nhạy cảm.

---

# 17. LOGOUT

Endpoint:

```http
POST /api/v1/auth/logout
```

Require authenticated user.

Input có thể nhận:

```json
{
  "refreshToken": "..."
}
```

Flow:

```text
hash token
↓
find token
↓
confirm belongs to current user
↓
set RevokedAtUtc
↓
audit logout
```

Logout phải idempotent nếu hợp lý.

---

# 18. CURRENT USER

Endpoint:

```http
GET /api/v1/auth/me
```

Require:

```text
[Authorize]
```

Response:

```json
{
  "id": "...",
  "email": "customer1@locallink.local",
  "roles": [
    "CUSTOMER"
  ],
  "customer": {
    "id": "...",
    "customerCode": "CUS000001",
    "fullName": "Nguyen Van An"
  }
}
```

Customer có thể null với ADMIN.

Không trả:

```text
PasswordHash
RefreshTokens
internal security fields
```

---

# 19. ROLE BASED AUTHORIZATION

Configure ASP.NET Core authorization.

Roles:

```text
CUSTOMER
STAFF
ADMIN
```

JWT role claims phải map đúng với ASP.NET Core role authorization.

Verify có thể sử dụng:

```csharp
[Authorize(Roles = "ADMIN")]
```

và:

```csharp
[Authorize(Roles = "STAFF,ADMIN")]
```

---

# 20. TEST AUTHORIZATION ENDPOINTS

Có thể tạo diagnostic endpoints chỉ trong Development để verify RBAC:

```http
GET /api/v1/auth/test/customer
GET /api/v1/auth/test/staff
GET /api/v1/auth/test/admin
```

Hoặc sử dụng controller nhỏ tương đương.

Nếu tạo, đánh dấu rõ:

```text
Development verification only
```

Không xây business feature.

---

# 21. APPLICATION STRUCTURE

Ưu tiên structure:

```text
LocalLink.Application/
├── Auth/
│   ├── DTOs/
│   ├── Interfaces/
│   └── Services/
```

hoặc convention phù hợp architecture hiện tại.

Không cần CQRS/MediatR.

---

# 22. AUTH DTOS

Tạo tối thiểu:

```text
LoginRequest
LoginResponse

RefreshTokenRequest
TokenResponse

LogoutRequest

CurrentUserDto
```

Không expose Entity trực tiếp từ Controller.

---

# 23. APPLICATION INTERFACES

Có thể tạo:

```text
IAuthService
ITokenService
ICurrentUserService
```

Chỉ tạo abstraction thực sự dùng.

Không tạo hàng loạt repository interface giả.

---

# 24. INFRASTRUCTURE

Infrastructure chịu trách nhiệm:

```text
JWT generation
Password hashing implementation
Refresh token generation
Database persistence
```

Tách hợp lý khỏi Controller.

Controller không chứa business logic token.

---

# 25. CURRENT USER SERVICE

Implement mechanism đọc:

```text
UserId
Email
Roles
```

từ:

```text
HttpContext.User
```

Không để Application layer phụ thuộc trực tiếp vào HttpContext nếu tránh được.

Có thể define abstraction:

```text
ICurrentUserService
```

và implement trong API/Infrastructure phù hợp architecture.

---

# 26. USER STATUS

Login chỉ cho phép:

```text
UserStatus.ACTIVE
```

Các trạng thái:

```text
SUSPENDED
LOCKED
CLOSED
```

không được login.

Response sử dụng 401/403 phù hợp.

Không tiết lộ internal account security state quá chi tiết cho attacker.

---

# 27. EMAIL NORMALIZATION

Email login phải được normalize.

Ví dụ:

```text
Trim
ToLowerInvariant
```

Database lookup phải consistent với seed data.

Không tạo duplicate email case-sensitive về mặt application.

---

# 28. AUDIT LOG

Ghi AuditLog cho:

```text
LOGIN_SUCCESS
LOGIN_FAILED
TOKEN_REFRESH
LOGOUT
```

Không log:

```text
Password
Raw JWT
Raw Refresh Token
JWT Secret
```

Audit có thể lưu:

```text
UserId
Action
EntityType = "User"
EntityId
Description
IpAddress
CreatedAtUtc
```

Login failed với email không tồn tại có thể UserId = null.

---

# 29. IP ADDRESS

Nếu available:

```text
HttpContext.Connection.RemoteIpAddress
```

được sử dụng cho AuditLog và refresh token metadata.

Không fail auth nếu IP null.

---

# 30. SWAGGER

Configure Swagger:

```text
Bearer JWT
```

Thêm:

```text
Authorize
```

button.

Sau login, developer có thể paste access token và test:

```text
GET /api/v1/auth/me
```

---

# 31. GLOBAL ERROR FORMAT

Giữ ProblemDetails architecture hiện tại.

Ví dụ invalid login:

```http
401
```

Không return stack trace.

Refresh invalid:

```http
401
```

Forbidden role:

```http
403
```

---

# 32. CORS

Không mở rộng CORS không cần thiết.

Giữ frontend origin hiện tại.

Không dùng:

```text
AllowAnyOrigin
+
AllowCredentials
```

---

# 33. DATABASE MIGRATION

Ưu tiên KHÔNG tạo migration nếu schema hiện tại đủ.

Chỉ tạo migration nếu thực sự cần thay đổi:

```text
index
column size
constraint
```

cho authentication.

Không tạo migration rỗng.

Nếu cần migration:

```text
ImproveAuthenticationSchema
```

Không sửa migration cũ đã commit.

---

# 34. REFRESH TOKEN INDEX

Kiểm tra RefreshTokens hiện tại.

Nếu chưa có index phù hợp để lookup:

```text
TokenHash
```

thì thêm UNIQUE index:

```text
UX_RefreshTokens_TokenHash
```

vì refresh endpoint sẽ lookup theo hash.

Nếu cần schema update, tạo migration.

---

# 35. TOKEN CLEANUP

Không cần background job ở milestone này.

Expired/revoked tokens có thể giữ lại.

Không tự thêm Hangfire/Quartz.

---

# 36. AUTHENTICATION CONFIGURATION

Program.cs phải có:

```text
AddAuthentication
AddJwtBearer
AddAuthorization
```

Middleware order đúng:

```text
UseAuthentication
UseAuthorization
```

trước endpoint execution.

---

# 37. DEVELOPMENT CREDENTIALS

Document trong README:

```text
Development Demo Accounts
```

Ví dụ:

```text
Customer 1
customer1@locallink.local

Customer 2
customer2@locallink.local

Admin
admin@locallink.local
```

Development password:

```text
LocalLink@123
```

Ghi rõ:

```text
Development only.
Never use these credentials in production.
```

---

# 38. DOCKER ENV

Update docker-compose để pass:

```text
JWT_SECRET
JWT_ISSUER
JWT_AUDIENCE
JWT_ACCESS_TOKEN_MINUTES
JWT_REFRESH_TOKEN_DAYS
```

Backend container phải đọc được config.

---

# 39. .ENV.EXAMPLE

Update:

```text
.env.example
```

Ví dụ:

```ini
JWT_SECRET=CHANGE_ME_WITH_A_LONG_RANDOM_SECRET
JWT_ISSUER=LocalLink
JWT_AUDIENCE=LocalLink.Client
JWT_ACCESS_TOKEN_MINUTES=15
JWT_REFRESH_TOKEN_DAYS=7
```

Không commit `.env`.

---

# 40. AUTOMATED TESTS

Tạo test project nếu chưa có.

Ưu tiên:

```text
backend/tests/
```

Tối thiểu test:

```text
Login success

Login invalid password

Login unknown email

Login suspended user

Refresh valid token

Refresh expired token

Refresh revoked token

Refresh token rotation

/me without token → 401

/me valid token → 200

Admin endpoint with CUSTOMER → 403

Admin endpoint with ADMIN → 200
```

Không cần test framework phức tạp.

---

# 41. SECURITY TEST

Verify token không chứa:

```text
PasswordHash
RefreshToken
database data không cần thiết
```

Verify logs không in token.

---

# 42. DOCKER CLEAN TEST

Chạy:

```bash
docker compose down -v
docker compose up --build -d
```

Verify:

```text
SQL Server Healthy
Migration applied
Seed users created with real password hashes
API Healthy
Frontend Operational
```

---

# 43. LOGIN VERIFICATION

Test:

```http
POST http://localhost:8080/api/v1/auth/login
```

với customer demo.

Expected:

```text
200 OK
```

Response có:

```text
accessToken
refreshToken
expiresAt
user
roles
```

---

# 44. PASSWORD DATABASE VERIFICATION

Query Users.

PasswordHash phải:

```text
not equal
```

với:

```text
LocalLink@123
```

Không được lưu password plaintext.

---

# 45. REFRESH VERIFICATION

Login lấy token pair.

Call refresh.

Verify:

```text
old refresh token revoked
new refresh token created
new access token created
```

Call lại refresh bằng old token:

```text
401
```

---

# 46. LOGOUT VERIFICATION

Logout bằng refresh token.

Sau logout:

```text
refresh token = revoked
```

Refresh lại:

```text
401
```

---

# 47. RBAC VERIFICATION

Customer JWT:

```text
CUSTOMER endpoint → 200
ADMIN endpoint → 403
```

Admin JWT:

```text
ADMIN endpoint → 200
```

---

# 48. SWAGGER VERIFICATION

Swagger phải:

```text
show Authorize button
```

Login.

Copy Access Token.

Authorize.

Call:

```http
GET /api/v1/auth/me
```

Expected:

```text
200
```

---

# 49. FRONTEND

Không xây Login UI hoàn chỉnh trong milestone này.

Có thể giữ frontend hiện tại.

Không thay đổi banking UI.

Frontend authentication sẽ làm milestone riêng.

---

# 50. GIT

Sau khi hoàn thành:

```bash
git status
```

Đảm bảo `.env` không tracked.

Commit:

```bash
git add .
git commit -m "feat: implement authentication and RBAC"
```

Push:

```bash
git push -u origin feature/auth
```

Không merge trực tiếp vào main.

---

# 51. DEFINITION OF DONE

Milestone hoàn thành khi:

```text
[ ] Password hashing chuẩn

[ ] Seed password hash thật

[ ] Login hoạt động

[ ] JWT access token hoạt động

[ ] Refresh token secure random

[ ] Refresh token hash trong database

[ ] Refresh token rotation hoạt động

[ ] Logout revoke token

[ ] /me hoạt động

[ ] CUSTOMER RBAC hoạt động

[ ] STAFF RBAC hoạt động

[ ] ADMIN RBAC hoạt động

[ ] Suspended/Locked user không login

[ ] Swagger JWT Authorize hoạt động

[ ] Audit login/logout hoạt động

[ ] Không log secrets

[ ] Docker clean rebuild thành công

[ ] Database seed thành công

[ ] Backend build 0 errors

[ ] Tests pass

[ ] feature/auth được push
```

---

# 52. FINAL REPORT

Sau khi hoàn thành trả:

```text
LOCALINK MILESTONE 3 AUTH REPORT

Branch:

Authentication Architecture:

Password Hasher:

JWT Algorithm:

Access Token Lifetime:

Refresh Token Lifetime:

Refresh Token Storage:

Refresh Token Rotation:

Endpoints:

RBAC Roles:

Swagger Auth:

Audit Events:

Database Changes:

Migration:

Tests:

Docker Result:

Login Test:

Refresh Test:

Logout Test:

RBAC Test:

Commit:

Push:

Warnings:
```

Sau đó DỪNG.

Không bắt đầu Customer API hoặc Transfer.