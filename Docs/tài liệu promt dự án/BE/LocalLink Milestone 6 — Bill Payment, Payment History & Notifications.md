# LocalLink Milestone 6 — Bill Payment, Payment History & Notifications

Project đã hoàn thành:

```text
M1 Foundation & DevOps ✅
M2 Database Foundation ✅
M3 Authentication + JWT + Refresh Token + RBAC ✅
M4 Customer & Bank Account Management ✅
M5 Transfer Engine + Transactions + Financial Integrity ✅
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

Database hiện đã có:

```text
Bills
Payments
BankAccounts
Transactions
Notifications
AuditLogs
Customers
Users
```

M5 hiện đã có pattern:

```text
Atomic SQL Transaction
Idempotency
RowVersion
AuditLog
Notification
Transaction History
ProblemDetails
CurrentUserService
Ownership Authorization
```

Hãy tái sử dụng các pattern hiện có.

Không redesign architecture.

Không implement frontend Bill UI trong milestone này.

---

# 1. MỤC TIÊU M6

Triển khai hoàn chỉnh:

```text
Bill Management (Customer Read)
        ↓
Bill Payment
        ↓
Atomic Financial Transaction
        ↓
Transaction Ledger
        ↓
Payment Record
        ↓
Bill Status Update
        ↓
Audit Log
        ↓
Notification
        ↓
Payment History
```

Customer phải có thể:

```text
Xem hóa đơn
Xem chi tiết hóa đơn
Thanh toán hóa đơn
Xem lịch sử thanh toán
Xem notification
Đánh dấu notification đã đọc
```

---

# 2. FEATURE BRANCH

Bắt đầu từ latest dev branch.

```bash
git checkout dev
git pull origin dev
git checkout -b feature/bill-payment
```

Không làm trực tiếp trên main.

---

# 3. BILL LIST

Implement:

```http
GET /api/v1/bills
```

Require:

```text
CUSTOMER
```

Chỉ trả bills thuộc current customer.

Support:

```text
page
pageSize
status
type
fromDueDate
toDueDate
```

SQL-level pagination.

Không load toàn bộ bills vào memory.

---

# 4. BILL DETAIL

Implement:

```http
GET /api/v1/bills/{id}
```

Require:

```text
CUSTOMER
```

Ownership bắt buộc:

```text
Bill.CustomerId == CurrentCustomer.Id
```

Nếu bill thuộc customer khác:

```text
404
```

để tránh data leakage.

---

# 5. BILL DTO

Response có thể gồm:

```json
{
  "id": "...",
  "billNumber": "ELEC-2026-0001",
  "providerName": "Da Nang Electricity",
  "billType": "ELECTRICITY",
  "amount": 850000,
  "dueDate": "2026-08-30",
  "status": "UNPAID",
  "createdAtUtc": "..."
}
```

Không expose internal EF fields.

---

# 6. PAYMENT ENDPOINT

Implement:

```http
POST /api/v1/payments
```

Require:

```text
CUSTOMER
```

Request:

```json
{
  "billId": "...",
  "accountId": "..."
}
```

Do NOT accept:

```text
amount
customerId
transactionId
paymentStatus
billStatus
```

from client.

Amount phải lấy server-side từ Bill.

---

# 7. PAYMENT FLOW

Flow:

```text
Current User
↓
Current Customer
↓
Load Bill
↓
Verify Bill Ownership
↓
Verify Bill = UNPAID
↓
Load Source Account
↓
Verify Account Ownership
↓
Verify Account ACTIVE
↓
Verify Balance >= Bill.Amount
↓
Begin SQL Transaction
↓
Debit Account
↓
Create Transaction(type = PAYMENT)
↓
Create Payment
↓
Bill.Status = PAID
↓
Create AuditLog
↓
Create Notification
↓
Commit
```

Nếu bất kỳ bước nào fail:

```text
ROLLBACK
```

---

# 8. BILL STATUS VALIDATION

Chỉ bill:

```text
UNPAID
```

mới được thanh toán.

Không cho thanh toán:

```text
PAID
CANCELLED
```

Nếu `OVERDUE` vẫn được phép thanh toán theo business rule hiện tại, cho phép và document rõ.

Nếu muốn đơn giản:

```text
UNPAID
OVERDUE
```

đều có thể thanh toán.

---

# 9. ACCOUNT VALIDATION

Account phải:

```text
exist
belong to current customer
status = ACTIVE
currency = VND
```

Không được dùng account customer khác.

---

# 10. AMOUNT

Payment amount:

```text
Bill.Amount
```

Không tin amount từ frontend.

Sử dụng decimal.

Không float/double.

---

# 11. BALANCE VALIDATION

Before debit:

```text
Account.Balance >= Bill.Amount
```

Nếu không đủ:

```text
INSUFFICIENT_FUNDS
```

Không thay đổi:

```text
Balance
Bill Status
Payment
Transaction
```

---

# 12. ATOMIC SQL TRANSACTION

Tái sử dụng pattern từ M5.

Các thao tác sau phải cùng một transaction:

```text
Debit Account
Create Transaction
Create Payment
Update Bill
Create AuditLog
Create Notification
```

Nếu lỗi:

```text
Rollback everything
```

---

# 13. ROWVERSION

BankAccount đã có RowVersion.

Payment phải tiếp tục sử dụng optimistic concurrency.

Nếu `DbUpdateConcurrencyException`:

```text
rollback
409 Conflict
```

Không bỏ RowVersion.

---

# 14. PAYMENT IDEMPOTENCY

POST payment cũng nên hỗ trợ:

```http
Idempotency-Key
```

Reason:

Frontend/mobile có thể retry request.

Same payment request + same key:

```text
return original receipt
do not debit twice
```

Same key + different bill/account:

```text
409 Conflict
```

---

# 15. IDEMPOTENCY DATABASE

Review Payment entity/schema.

Nếu chưa có:

```text
IdempotencyKey
```

thì thêm:

```text
Payments.IdempotencyKey
```

nullable.

Unique filtered index:

```text
IX_Payments_IdempotencyKey
```

Nếu cần schema change tạo migration:

```text
ImprovePaymentIntegrity
```

Không sửa old migration.

---

# 16. PAYMENT RECORD

Payment sử dụng existing entity.

Expected fields conceptually:

```text
Id
BillId
AccountId
TransactionId
Amount
Status
PaidAtUtc
CreatedAtUtc
IdempotencyKey
```

Không redesign nếu schema hiện tại tương đương.

---

# 17. PAYMENT STATUS

Use existing PaymentStatus.

Flow thành công:

```text
PENDING
↓
COMPLETED
```

Nếu transaction rollback, không để lại misleading COMPLETED payment.

---

# 18. TRANSACTION LEDGER

Payment phải tạo Transaction record:

```text
TransactionType = PAYMENT
SourceAccountId = paying account
DestinationAccountId = null
Amount = Bill.Amount
Currency = VND
Status = COMPLETED
ReferenceNumber = generated server-side
```

Reuse transaction convention từ M5.

---

# 19. PAYMENT REFERENCE

Generate unique reference.

Example:

```text
PAY202608181620001234
```

Server generated.

Unique at database level thông qua existing transaction reference constraint.

---

# 20. BILL UPDATE

Successful payment:

```text
Bill.Status = PAID
```

Update:

```text
UpdatedAtUtc
```

Payment:

```text
PaidAtUtc
```

---

# 21. AUDIT LOG

Successful payment:

```text
PAYMENT_COMPLETED
```

Audit:

```text
UserId
EntityType = Payment
EntityId
Description
IpAddress
CreatedAtUtc
```

Không log:

```text
JWT
RefreshToken
Password
Secrets
```

---

# 22. PAYMENT NOTIFICATION

Create notification for customer:

Title:

```text
Thanh toán thành công
```

Message example:

```text
Bạn đã thanh toán hóa đơn điện 850,000 VND thành công.
```

Type:

```text
PAYMENT
```

Notification creation nên nằm trong transaction nếu architecture hiện tại đã làm tương tự M5.

---

# 23. PAYMENT RECEIPT

Response:

```json
{
  "paymentId": "...",
  "reference": "PAY...",
  "billNumber": "ELEC-2026-0001",
  "providerName": "Da Nang Electricity",
  "accountNumber": "1000000001",
  "amount": 850000,
  "currency": "VND",
  "status": "COMPLETED",
  "paidAtUtc": "..."
}
```

---

# 24. PAYMENT HISTORY

Implement:

```http
GET /api/v1/payments
```

Require CUSTOMER.

Chỉ payment của current customer.

Support:

```text
page
pageSize
status
fromDate
toDate
billType
```

SQL-level pagination.

---

# 25. PAYMENT DETAIL

Implement:

```http
GET /api/v1/payments/{id}
```

Ownership required.

Customer chỉ được xem payment liên quan bill/account của mình.

---

# 26. NOTIFICATION LIST

Implement:

```http
GET /api/v1/notifications
```

Require authenticated user.

Only current user notifications.

Support:

```text
page
pageSize
isRead
type
```

Default:

```text
newest first
```

---

# 27. NOTIFICATION DETAIL

Optional:

```http
GET /api/v1/notifications/{id}
```

Nếu implement:

Ownership required.

---

# 28. MARK AS READ

Implement:

```http
PATCH /api/v1/notifications/{id}/read
```

Only owner.

Set:

```text
IsRead = true
ReadAtUtc = DateTime.UtcNow
```

Idempotent:

Calling twice should still succeed safely.

---

# 29. MARK ALL READ

Implement:

```http
PATCH /api/v1/notifications/read-all
```

Mark all unread notifications of current user.

Do not update notifications of other users.

---

# 30. UNREAD COUNT

Implement if simple:

```http
GET /api/v1/notifications/unread-count
```

Response:

```json
{
  "count": 3
}
```

Useful later for web/mobile badge.

---

# 31. DTO STRUCTURE

Add only needed DTOs:

```text
BillListItemDto
BillDetailDto

CreatePaymentRequest
PaymentReceiptDto
PaymentListItemDto
PaymentDetailDto

NotificationDto
UnreadNotificationCountDto
```

Do not expose entities directly.

---

# 32. APPLICATION STRUCTURE

Follow current conventions:

```text
Application/
├── Bills/
├── Payments/
└── Notifications/
```

Each can contain:

```text
DTOs
Interfaces
Services
```

No MediatR/CQRS.

---

# 33. BILL SERVICE

Bill service handles:

```text
GetCurrentCustomerBills
GetBillDetail
Ownership
Filters
Pagination
```

No financial mutation here.

---

# 34. PAYMENT SERVICE

Payment service handles:

```text
Validation
Ownership
Balance
Concurrency
Idempotency
SQL Transaction
Ledger creation
Payment creation
Bill update
Audit
Notification
```

Controller must not contain financial logic.

---

# 35. NOTIFICATION SERVICE

Handles:

```text
List current user's notifications
Mark one read
Mark all read
Unread count
```

Do not introduce WebSocket/SignalR yet.

Realtime notification can be a future extension.

---

# 36. DATABASE REVIEW

Inspect existing:

```text
Bills
Payments
Notifications
Transactions
BankAccounts
```

before changing schema.

Do not make unnecessary migration.

---

# 37. BILL INDEXES

Review existing indexes.

Useful:

```text
Bills(CustomerId, Status)
Bills(DueDate)
Bills(BillNumber) UNIQUE
```

Do not duplicate existing indexes.

---

# 38. PAYMENT INDEXES

Potential useful indexes:

```text
Payments(AccountId)
Payments(BillId) UNIQUE
Payments(TransactionId) UNIQUE
Payments(PaidAtUtc)
Payments(IdempotencyKey) UNIQUE FILTERED
```

Only add if missing.

---

# 39. NOTIFICATION INDEX

Existing expected index:

```text
UserId + IsRead + CreatedAtUtc
```

Reuse.

---

# 40. SEED DEMO BILLS

Add development seed bills.

For Customer 1:

```text
Bill 1
Provider: Da Nang Electricity
Type: ELECTRICITY
Bill Number: ELEC-2026-0001
Amount: 850,000 VND
Status: UNPAID
```

```text
Bill 2
Provider: Da Nang Water
Type: WATER
Bill Number: WATER-2026-0001
Amount: 220,000 VND
Status: UNPAID
```

For Customer 2:

```text
Bill 3
Provider: VNPT Internet
Type: INTERNET
Bill Number: NET-2026-0002
Amount: 350,000 VND
Status: UNPAID
```

Seeder must remain idempotent.

---

# 41. SEED IDEMPOTENCY

Restart must not duplicate bills.

Use:

```text
BillNumber
```

to identify existing demo bill.

---

# 42. BUSINESS ERRORS

Use existing ProblemDetails.

Recommended errors:

```text
BILL_NOT_FOUND
BILL_ALREADY_PAID
BILL_NOT_PAYABLE
ACCOUNT_NOT_FOUND
ACCOUNT_NOT_ACTIVE
INSUFFICIENT_FUNDS
PAYMENT_IDEMPOTENCY_CONFLICT
CONCURRENCY_CONFLICT
```

---

# 43. SECURITY

Verify:

```text
Customer cannot view other customer's bill
Customer cannot pay other customer's bill
Customer cannot use other customer's account
Customer cannot provide custom amount
Customer cannot mark other user's notifications read
Customer cannot read other user's notification
```

---

# 44. TESTS — BILL LIST

Test:

```text
Customer sees only own bills
Customer cannot fetch another customer's bill
Pagination works
Status filter works
```

---

# 45. TEST — PAYMENT SUCCESS

Initial:

```text
Account balance = 24,500,000
Bill amount = 850,000
```

After payment:

```text
Balance = 23,650,000
Bill = PAID
Payment = COMPLETED
Transaction = PAYMENT / COMPLETED
AuditLog exists
Notification exists
```

---

# 46. MONEY CONSERVATION NOTE

Payment is an outflow from simulated customer banking system.

Unlike internal transfer:

```text
total customer balances may decrease
```

This is expected.

Do NOT incorrectly assert internal money conservation across payment.

Document distinction from Transfer.

---

# 47. TEST — INSUFFICIENT FUNDS

Bill amount > balance.

Expected:

```text
Payment rejected
Balance unchanged
Bill unchanged
No completed Payment
No completed Transaction
```

---

# 48. TEST — PAY SAME BILL TWICE

Pay bill once.

Second payment attempt:

```text
reject
```

Bill already:

```text
PAID
```

No second debit.

---

# 49. TEST — PAYMENT IDEMPOTENCY

Same:

```text
Idempotency-Key
bill
account
```

sent twice.

Expected:

```text
one debit
one payment
original receipt returned
```

---

# 50. TEST — PAYMENT IDEMPOTENCY CONFLICT

Same key:

First:

```text
Bill A
```

Second:

```text
Bill B
```

Expected:

```text
409
```

No second debit.

---

# 51. TEST — LOCKED ACCOUNT

Pay from LOCKED account.

Expected:

```text
rejected
balance unchanged
bill unchanged
```

---

# 52. TEST — CONCURRENCY

Use RowVersion.

Concurrent payment attempts using same account must not overspend.

If accurate concurrency test requires SQL Server integration, use SQL Server integration test where feasible.

Do not pretend InMemory proves RowVersion correctness.

---

# 53. TEST — NOTIFICATION OWNERSHIP

User A cannot:

```text
read
mark as read
```

User B notification.

---

# 54. TEST — MARK READ

Notification:

```text
IsRead = false
```

Call mark read.

Expected:

```text
IsRead = true
ReadAtUtc != null
```

Second call remains safe.

---

# 55. TEST — READ ALL

Only current user's unread notifications should update.

Other users remain unchanged.

---

# 56. REGRESSION

All existing:

```text
45 tests
```

must continue to pass.

Add meaningful M6 tests.

Target quality, not arbitrary count.

---

# 57. INTEGRATION TESTS

If current test setup supports:

Test live:

```text
GET /bills
POST /payments
GET /payments
GET /notifications
PATCH /notifications/{id}/read
```

---

# 58. DOCKER VERIFICATION

Run:

```bash
dotnet test backend/LocalLink.sln
```

Expected:

```text
0 failed
```

Then:

```bash
docker compose up --build -d
```

If migration added:

```bash
docker compose down -v
docker compose up --build -d
```

Verify:

```text
SQL Server Healthy
API Healthy
Frontend Operational
```

---

# 59. LIVE PAYMENT TEST

Login:

```text
customer1@locallink.local
```

Before:

```text
Account:
1000000001

Record balance
```

Get bills:

```http
GET /api/v1/bills
```

Select:

```text
ELEC-2026-0001
850,000 VND
```

Pay:

```http
POST /api/v1/payments
Idempotency-Key: LIVE-M6-PAY-001
```

Verify:

```text
balance decreases exactly 850,000
bill becomes PAID
payment exists
transaction exists
audit exists
notification exists
```

---

# 60. LIVE DUPLICATE TEST

Replay:

```text
LIVE-M6-PAY-001
```

Expected:

```text
same receipt
no second debit
```

---

# 61. LIVE SECOND PAYMENT TEST

Attempt payment of same bill using new Idempotency-Key.

Expected:

```text
BILL_ALREADY_PAID
```

No debit.

---

# 62. PAYMENT HISTORY TEST

Verify:

```http
GET /api/v1/payments
GET /api/v1/payments/{id}
```

contains payment.

---

# 63. TRANSACTION HISTORY

Existing:

```http
GET /api/v1/transactions
```

must now display:

```text
TRANSFER
PAYMENT
```

correctly.

No regression.

---

# 64. NOTIFICATION LIVE TEST

After payment:

```http
GET /api/v1/notifications
```

Payment notification visible.

Call:

```http
PATCH /api/v1/notifications/{id}/read
```

Verify read state.

Then:

```http
GET /api/v1/notifications/unread-count
```

Verify count.

---

# 65. SWAGGER

Add groups:

```text
Bills
Payments
Notifications
```

JWT Authorize must continue working.

Document Idempotency-Key on payment endpoint.

---

# 66. README

Update:

```text
README.md
Docs/README.md
```

Document:

```text
Bill Payment Flow
Payment Integrity
Payment Idempotency
Notification APIs
Error Codes
Authorization
```

---

# 67. API DOCS

Create:

```text
Docs/api/bills.md
Docs/api/payments.md
Docs/api/notifications.md
```

Include:

```text
Request
Response
TypeScript interface
Errors
Authorization
Examples
```

Useful for future Nuxt and Flutter clients.

---

# 68. FRONTEND ROADMAP

Only update status:

```text
M1 COMPLETED
M2 COMPLETED
M3 COMPLETED
M4 COMPLETED
M5 COMPLETED
M6 COMPLETED
```

Do not build full UI.

---

# 69. GIT

After successful verification:

```bash
git status
```

No secrets.

Commit:

```bash
git add .
git commit -m "feat: implement bill payment and notifications"
```

Push:

```bash
git push -u origin feature/bill-payment
```

Do not merge directly into main.

---

# 70. DEFINITION OF DONE

M6 complete only when:

```text
[ ] Bill list works
[ ] Bill detail ownership enforced
[ ] Payment endpoint works
[ ] Bill ownership enforced
[ ] Account ownership enforced
[ ] Account status enforced
[ ] Bill status enforced
[ ] Amount server controlled
[ ] Balance validation works
[ ] Atomic SQL transaction works
[ ] RowVersion concurrency retained
[ ] Payment idempotency works
[ ] Duplicate payment prevented
[ ] Payment record created
[ ] Transaction PAYMENT created
[ ] Bill becomes PAID
[ ] AuditLog created
[ ] Payment Notification created
[ ] Payment history works
[ ] Payment detail ownership works
[ ] Notification list works
[ ] Mark read works
[ ] Mark all read works
[ ] Unread count works
[ ] Seed demo bills idempotent
[ ] Existing 45 tests pass
[ ] M6 tests pass
[ ] Docker healthy
[ ] Live payment verified
[ ] Duplicate payment retry verified
[ ] Same bill cannot be paid twice
[ ] Swagger documented
[ ] Feature branch pushed
```

---

# 71. FINAL REPORT

Return:

```text
LOCALINK MILESTONE 6 REPORT

Branch:

Bill Endpoints:

Payment Endpoints:

Notification Endpoints:

Bill Ownership:

Account Ownership:

Payment Transaction Strategy:

Payment Amount Strategy:

Idempotency Strategy:

Concurrency Strategy:

Ledger Strategy:

Bill Status Strategy:

Audit Events:

Notification Strategy:

Database Changes:

Migration:

Seed Bills:

Tests Before:

Tests Added:

Tests Total:

Test Result:

Docker Result:

Live Payment:

Balance Before:

Balance After:

Duplicate Retry:

Second Payment Attempt:

Payment History:

Notification Verification:

Swagger:

Commit:

Push:

Warnings / Known Limitations:
```

Then STOP.

Do not implement Staff/Admin analytics.

Do not implement frontend banking UI.