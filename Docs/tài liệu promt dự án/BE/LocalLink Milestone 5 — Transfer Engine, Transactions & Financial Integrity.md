# LocalLink Milestone 5 — Transfer Engine, Transactions & Financial Integrity

Project đã hoàn thành:

```text
M1 Foundation & DevOps ✅
M2 Database Foundation ✅
M3 Authentication + JWT + Refresh Token + RBAC ✅
M4 Customer & Bank Account Management ✅
```

Tech stack:

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

Existing domain/database includes:

```text
Users
Customers
BankAccounts
Beneficiaries
Transactions
Transfers
AuditLogs
Notifications
```

BankAccount already has RowVersion optimistic concurrency support.

Authentication and CurrentUserService already exist.

DO NOT redesign the existing architecture.

DO NOT replace existing migrations.

DO NOT implement frontend transfer UI.

---

# 1. MILESTONE OBJECTIVE

Implement a safe internal bank transfer engine.

The system must support:

```text
CUSTOMER
   ↓
Select own source account
   ↓
Enter destination account number
   ↓
Enter amount
   ↓
Optional description
   ↓
Submit transfer
   ↓
Validate
   ↓
Atomic database transaction
   ↓
Debit source
   ↓
Credit destination
   ↓
Create financial records
   ↓
Audit
   ↓
Return receipt
```

Financial correctness is more important than architecture complexity.

---

# 2. FEATURE BRANCH

Start from latest development branch.

```bash
git checkout dev
git pull origin dev
git checkout -b feature/transfer-engine
```

If the project currently uses another established feature-branch convention, follow the existing repository convention instead.

Never work directly on main.

---

# 3. TRANSFER ENDPOINT

Implement:

```http
POST /api/v1/transfers
```

Require:

```text
CUSTOMER
```

Example request:

```json
{
  "sourceAccountId": "...",
  "destinationAccountNumber": "1000000002",
  "amount": 500000,
  "description": "Chuyen tien"
}
```

Do NOT accept:

```text
sourceCustomerId
destinationCustomerId
sourceBalance
destinationBalance
transactionStatus
transferStatus
```

from the client.

---

# 4. OWNERSHIP

Resolve current customer from JWT.

Then:

```text
Current User
↓
Current Customer
↓
Source Account
↓
Verify SourceAccount.CustomerId == CurrentCustomer.Id
```

A customer must NEVER transfer money from another customer's account.

If source account is not owned by current customer, return a safe 404/403 according to the project's existing security convention.

---

# 5. VALIDATE SOURCE ACCOUNT

Before transfer:

```text
Source account exists
Source account belongs to current customer
Source account status == ACTIVE
Source account currency supported
```

LOCKED or CLOSED source accounts cannot transfer.

---

# 6. VALIDATE DESTINATION

Resolve destination using:

```text
destinationAccountNumber
```

Validate:

```text
destination exists
destination status == ACTIVE
destination != source
currency compatible
```

Do not trust destination information supplied by client.

Load destination account from database.

---

# 7. AMOUNT VALIDATION

Amount must be:

```text
> 0
```

For current simulation, support:

```text
VND
```

Do not use:

```text
float
double
```

for financial amounts.

Continue using:

```text
decimal
```

and existing SQL decimal precision.

---

# 8. BALANCE VALIDATION

Before debit:

```text
Source.Balance >= Amount
```

If not:

Return business error:

```text
INSUFFICIENT_FUNDS
```

HTTP status may be:

```text
400
```

or existing ProblemDetails convention.

Do not expose unnecessary internal information.

---

# 9. ATOMIC DATABASE TRANSACTION

THIS IS CRITICAL.

The following operations must happen inside ONE database transaction:

```text
Debit source
Credit destination
Create Transfer
Create Transaction record(s)
Create AuditLog
```

Use EF Core transaction support, e.g.:

```text
BeginTransactionAsync
CommitAsync
RollbackAsync
```

or an equivalent correct EF Core transactional strategy.

If ANY operation fails:

```text
ROLLBACK EVERYTHING
```

Never allow:

```text
source debited
destination not credited
```

or the reverse.

---

# 10. MONEY MOVEMENT

Inside transaction:

```text
source.Balance -= amount

destination.Balance += amount
```

No controller may mutate balance directly.

Money mutation belongs in the financial application/service layer.

---

# 11. TRANSACTION LEDGER

Review the existing Transaction entity/schema before implementation.

Use the existing model whenever possible.

Each transfer should produce appropriate ledger records.

Preferred model if compatible with current schema:

```text
Source account
TransactionType = DEBIT
Amount = amount

Destination account
TransactionType = CREDIT
Amount = amount
```

Both records should reference/correlate to the same transfer if the current schema supports it.

If the existing schema was designed differently, adapt to it rather than unnecessarily redesigning the database.

---

# 12. TRANSFER RECORD

Create one Transfer representing the business operation.

It should contain the fields supported by the existing entity, such as:

```text
Id
Reference
SourceAccountId
DestinationAccountId
Amount
Description
Status
CreatedAtUtc
CompletedAtUtc
```

Use existing names/schema where they differ.

---

# 13. TRANSFER REFERENCE

Every transfer needs a human-readable unique reference.

Example:

```text
TRF202608181530001234
```

Requirements:

```text
unique
server-generated
not supplied by client
```

Do not rely only on a timestamp if collisions are possible.

A random/counter component can be added.

If schema supports a unique index, enforce uniqueness at DB level.

---

# 14. TRANSFER STATUS

Use existing TransferStatus enum.

Expected successful flow:

```text
PENDING
↓
PROCESSING
↓
COMPLETED
```

For the synchronous simulation, it is acceptable to create and complete the transfer within the same request.

If transaction fails before commit, do not leave misleading COMPLETED data.

Do not over-engineer asynchronous processing.

---

# 15. IDEMPOTENCY — IMPORTANT

Banking transfer requests must be protected against accidental duplicate submissions.

Support an idempotency key.

Preferred header:

```http
Idempotency-Key: <unique-client-generated-value>
```

Example:

```text
80f13a76-...
```

Behavior:

First request:

```text
Idempotency-Key = ABC
↓
Transfer executes
↓
500,000 transferred
```

Same request retried with:

```text
Idempotency-Key = ABC
```

must NOT transfer another 500,000.

Return the previously created transfer result if appropriate.

---

# 16. IDEMPOTENCY STORAGE

Review existing schema first.

If there is no suitable field/storage for idempotency, add the minimum necessary schema.

Preferred option:

```text
Transfers.IdempotencyKey
```

with a UNIQUE constraint/index scoped appropriately.

For stronger ownership isolation, uniqueness can be logically associated with initiating user/customer if architecture requires it.

Do not introduce Redis just for this milestone.

Use SQL Server.

If schema changes:

Create migration:

```text
ImproveTransferIntegrity
```

Never edit old migrations.

---

# 17. IDEMPOTENCY PAYLOAD SAFETY

If an existing idempotency key is reused with a materially different request:

Example:

```text
First:
ABC → 500,000 → account B

Second:
ABC → 5,000,000 → account C
```

Do NOT silently return success.

Return:

```text
409 Conflict
```

or equivalent ProblemDetails.

Store/compare sufficient request identity if necessary.

Keep implementation simple but correct.

---

# 18. CONCURRENCY CONTROL

BankAccount already has RowVersion.

Use optimistic concurrency correctly.

Problem scenario:

```text
Balance = 1,000,000

Request A → transfer 800,000
Request B → transfer 800,000
```

Both requests must NOT successfully spend 1,600,000.

Handle:

```text
DbUpdateConcurrencyException
```

appropriately.

A concurrency conflict should not leave partial money movement.

Rollback transaction.

Return a safe conflict response such as:

```text
409 Conflict
```

Client may retry by reloading latest balance.

---

# 19. DO NOT DISABLE ROWVERSION

Do not remove:

```text
RowVersion
```

Do not bypass optimistic concurrency.

Do not use unsafe last-write-wins behavior for balances.

---

# 20. TRANSFER RECEIPT

Successful response should contain a DTO such as:

```json
{
  "transferId": "...",
  "reference": "TRF...",
  "sourceAccountNumber": "1000000001",
  "destinationAccountNumber": "1000000002",
  "destinationAccountName": "Tran Thi Binh",
  "amount": 500000,
  "currency": "VND",
  "description": "Chuyen tien",
  "status": "COMPLETED",
  "createdAtUtc": "...",
  "completedAtUtc": "..."
}
```

Do not return EF entities directly.

---

# 21. GET TRANSFER DETAIL

Implement:

```http
GET /api/v1/transfers/{id}
```

CUSTOMER may only retrieve transfers involving an account they own according to the intended customer-facing policy.

At minimum, the initiating customer must be able to retrieve their transfer receipt.

Do not leak transfers belonging entirely to other customers.

---

# 22. TRANSFER HISTORY

Implement:

```http
GET /api/v1/transfers
```

for current customer.

Support:

```text
page
pageSize
status
fromDate
toDate
```

Optional:

```text
accountId
```

If accountId is provided:

Verify ownership.

Pagination must happen at SQL level.

---

# 23. TRANSACTION HISTORY

Implement:

```http
GET /api/v1/transactions
```

CUSTOMER only sees transactions belonging to their accounts.

Support:

```text
page
pageSize
accountId
type
fromDate
toDate
```

Never allow customer to query another customer's account history.

---

# 24. TRANSACTION DETAIL

Implement if compatible with existing domain:

```http
GET /api/v1/transactions/{id}
```

Ownership required.

---

# 25. PAGINATION

Reuse the M4 pagination model.

Example:

```json
{
  "items": [],
  "page": 1,
  "pageSize": 20,
  "totalItems": 50,
  "totalPages": 3
}
```

Perform pagination at SQL level.

Use:

```text
AsNoTracking
Select
Skip
Take
```

where appropriate.

---

# 26. AUDIT LOG

Successful transfer must create:

```text
TRANSFER_COMPLETED
```

Audit information:

```text
UserId
Action
EntityType = Transfer
EntityId
Description
IpAddress
CreatedAtUtc
```

Do NOT log:

```text
JWT
Refresh Token
password
secrets
```

Avoid dumping unnecessary sensitive financial payloads.

---

# 27. FAILED TRANSFER AUDIT

Consider recording meaningful failed attempts such as:

```text
TRANSFER_FAILED
```

for:

```text
insufficient funds
locked source account
invalid destination
```

But do not compromise atomicity.

If AuditLog is inside a transaction that rolls back, failed-attempt audit may need to be persisted separately after rollback.

Keep this implementation simple and reliable.

Successful financial state is the higher priority.

---

# 28. NOTIFICATION

If Notification entity already supports this cleanly, create notifications after successful transfer.

Sender example:

```text
Bạn đã chuyển 500,000 VND đến Tran Thi Binh.
```

Receiver example:

```text
Bạn đã nhận 500,000 VND.
```

However:

Notification must NOT be allowed to break the financial transaction unnecessarily.

If the current architecture makes notifications awkward, defer notification creation to the later Notification milestone and document it.

Do NOT introduce a message broker.

---

# 29. BUSINESS ERROR MODEL

Use existing ProblemDetails architecture.

Recommended errors:

```text
INVALID_AMOUNT
ACCOUNT_NOT_FOUND
ACCOUNT_NOT_ACTIVE
DESTINATION_NOT_ACTIVE
SAME_ACCOUNT_TRANSFER
INSUFFICIENT_FUNDS
IDEMPOTENCY_CONFLICT
CONCURRENCY_CONFLICT
```

Keep messages safe for clients.

---

# 30. CONTROLLER RESPONSIBILITY

Controller should only:

```text
receive request
validate basic HTTP input
get current context
call service
return response
```

Do not place:

```text
balance mutation
transaction handling
ledger logic
```

inside controller.

---

# 31. APPLICATION STRUCTURE

Recommended:

```text
LocalLink.Application/
└── Transfers/
    ├── DTOs/
    ├── Interfaces/
    └── Services/

LocalLink.Application/
└── Transactions/
    ├── DTOs/
    ├── Interfaces/
    └── Services/
```

Follow existing conventions if different.

Do not introduce CQRS/MediatR just for this milestone.

---

# 32. DTOs

Create only necessary DTOs such as:

```text
CreateTransferRequest
TransferReceiptDto
TransferListItemDto
TransactionListItemDto
TransactionDetailDto
```

Reuse:

```text
PagedResult<T>
```

from M4.

---

# 33. DATABASE REVIEW

Before changing schema, inspect:

```text
BankAccounts
Transactions
Transfers
```

and their EF configurations.

Do not assume missing columns.

Reuse existing schema wherever possible.

---

# 34. DATABASE INDEX REVIEW

Review indexes for common financial queries.

Potential indexes:

```text
Transfers.Reference UNIQUE
Transfers.SourceAccountId
Transfers.DestinationAccountId
Transfers.CreatedAtUtc

Transactions.AccountId
Transactions.CreatedAtUtc
```

For idempotency:

```text
Transfers.IdempotencyKey
```

unique according to selected strategy.

Only add indexes that are missing and useful.

---

# 35. DATABASE MIGRATION

If schema changes are required:

```bash
dotnet dotnet-ef migrations add ImproveTransferIntegrity \
  --project src/LocalLink.Infrastructure \
  --startup-project src/LocalLink.API \
  --output-dir Persistence/Migrations
```

Do not create an empty migration.

Do not modify old migrations.

---

# 36. FINANCIAL PRECISION

Verify SQL columns for money use appropriate decimal precision.

Do not silently change precision unless necessary.

No float/double.

Tests must include values with realistic VND amounts.

---

# 37. TEST — SUCCESSFUL TRANSFER

Seed state:

```text
Account A = 25,000,000
Account B = 15,000,000
```

Transfer:

```text
A → B
500,000
```

Expected:

```text
A = 24,500,000
B = 15,500,000

Transfer = COMPLETED

Debit transaction exists
Credit transaction exists

AuditLog exists
```

---

# 38. TEST — INSUFFICIENT FUNDS

Example:

```text
A balance = 1,000,000
Transfer = 2,000,000
```

Expected:

```text
rejected
A remains 1,000,000
B unchanged
no completed transfer
no financial ledger mutation
```

---

# 39. TEST — OWNERSHIP

Customer A attempts:

```text
sourceAccountId = Customer B account
```

Expected:

```text
rejected
balances unchanged
```

---

# 40. TEST — LOCKED ACCOUNT

Source:

```text
LOCKED
```

Transfer must fail.

Destination:

```text
LOCKED
```

Transfer must fail according to business rule.

Balances unchanged.

---

# 41. TEST — SAME ACCOUNT

```text
SourceAccount == DestinationAccount
```

Expected:

```text
400
```

No balance mutation.

---

# 42. TEST — IDEMPOTENCY

Request:

```text
Idempotency-Key = TEST-001
Amount = 500,000
```

Send twice.

Expected final balances:

```text
A decreased exactly 500,000
B increased exactly 500,000
```

NOT:

```text
1,000,000
```

Only one logical transfer exists.

---

# 43. TEST — IDEMPOTENCY CONFLICT

Use:

```text
TEST-002
```

First request:

```text
500,000 → Account B
```

Second request with same key:

```text
1,000,000 → Account B
```

Expected:

```text
409
```

Second transfer does not execute.

---

# 44. TEST — CONCURRENCY

Create a concurrency test simulating two transfer attempts against the same source account.

Example:

```text
Balance = 1,000,000

Transfer A = 800,000
Transfer B = 800,000
```

Expected invariant:

```text
Balance never becomes negative
Total successful outgoing <= original available balance
```

At most one should succeed in this scenario.

If SQLite/InMemory cannot accurately model SQL Server RowVersion semantics, do not pretend the test proves concurrency safety.

Prefer an integration test against SQL Server where feasible, or clearly document the limitation.

---

# 45. MONEY CONSERVATION INVARIANT

For internal transfer:

Before:

```text
Source + Destination = X
```

After:

```text
Source + Destination = X
```

Example:

```text
25M + 15M = 40M

24.5M + 15.5M = 40M
```

Add a test asserting this invariant.

This is critical.

---

# 46. ROLLBACK TEST

Force/simulate failure during financial persistence if test architecture allows.

Verify:

```text
Source balance unchanged
Destination balance unchanged
No misleading completed ledger
```

---

# 47. EXISTING TEST REGRESSION

All existing:

```text
34 tests
```

must continue to pass.

Do not break:

```text
Auth
Customer
Accounts
Beneficiaries
Admin
```

---

# 48. EXPECTED TEST SUITE

Add tests for:

```text
successful transfer
insufficient balance
invalid amount
source ownership
source locked
destination locked
same-account transfer
destination not found
idempotency retry
idempotency conflict
concurrency
money conservation
transfer history ownership
transaction history ownership
pagination
```

Aim for meaningful coverage, not arbitrary test count.

---

# 49. SWAGGER

Swagger must expose:

```text
Transfers
Transactions
```

Bearer JWT authorization must continue to work.

Document:

```text
Idempotency-Key
```

header for POST transfer.

---

# 50. DOCKER BUILD

Run:

```bash
dotnet test backend/LocalLink.sln
```

Then:

```bash
docker compose up --build -d
```

Verify:

```text
SQL Server Healthy
API Healthy
Frontend Operational
```

If migration changed schema, also perform clean database verification:

```bash
docker compose down -v
docker compose up --build -d
```

---

# 51. LIVE TRANSFER TEST

Login:

```text
customer1@locallink.local
```

Get JWT.

Before transfer record balances for:

```text
1000000001
1000000002
```

Execute:

```http
POST /api/v1/transfers
Idempotency-Key: LIVE-M5-001
```

Amount:

```text
500000
```

Expected:

```text
1000000001
25,000,000 → 24,500,000

1000000002
15,000,000 → 15,500,000
```

Verify using API and database.

---

# 52. LIVE DUPLICATE TEST

Send exact same request again with:

```text
LIVE-M5-001
```

Expected:

```text
NO additional money movement
```

Balances remain:

```text
24,500,000
15,500,000
```

---

# 53. LIVE HISTORY TEST

Verify:

```http
GET /api/v1/transfers

GET /api/v1/transactions
```

Transfer should appear in history.

Source transaction should reflect debit.

Destination customer should see appropriate credit transaction in their transaction history.

---

# 54. FRONTEND

Do not build Transfer UI yet.

Only update roadmap after M5 succeeds:

```text
M1 COMPLETED
M2 COMPLETED
M3 COMPLETED
M4 COMPLETED
M5 COMPLETED
```

Frontend banking UI comes later.

---

# 55. DOCUMENTATION

Update:

```text
README.md
Docs/README.md
```

Document:

```text
Transfer flow
Financial invariants
Atomic transaction strategy
RowVersion concurrency strategy
Idempotency strategy
Transaction ledger
Transfer endpoints
Transaction endpoints
Error codes
```

Include a Mermaid flow if appropriate.

---

# 56. SECURITY REVIEW

Verify:

```text
Customer cannot debit another customer's account
Customer cannot directly modify balance
Customer cannot forge destination customer data
Client cannot specify transaction status
Client cannot specify transfer status
Client cannot bypass account status
Duplicate request cannot double-transfer
Concurrent requests cannot overspend
```

---

# 57. GIT

After successful verification:

```bash
git status
```

Ensure secrets are not tracked.

Commit:

```bash
git add .
git commit -m "feat: implement secure transfer engine"
```

Push:

```bash
git push -u origin feature/transfer-engine
```

Do not merge directly into main.

---

# 58. DEFINITION OF DONE

M5 is complete only when:

```text
[ ] POST transfer works
[ ] Source ownership enforced
[ ] Source/destination status validated
[ ] Amount validated
[ ] Insufficient funds handled
[ ] Balance uses decimal
[ ] Atomic SQL transaction implemented
[ ] Source debit works
[ ] Destination credit works
[ ] Transfer record created
[ ] Ledger transactions created
[ ] Unique transfer reference generated
[ ] Idempotency implemented
[ ] Duplicate request cannot double-transfer
[ ] Idempotency payload conflict handled
[ ] RowVersion concurrency used
[ ] Concurrency conflict handled safely
[ ] Money conservation invariant tested
[ ] Rollback safety verified
[ ] Transfer history works
[ ] Transaction history works
[ ] Ownership enforced on history
[ ] Audit log works
[ ] Existing 34 tests still pass
[ ] New financial tests pass
[ ] Swagger works
[ ] Docker healthy
[ ] Clean migration verification passes if schema changed
[ ] Live 500,000 VND transfer verified
[ ] Duplicate live request verified
[ ] feature branch pushed
```

---

# 59. FINAL REPORT

After completing implementation return:

```text
LOCALINK MILESTONE 5 TRANSFER ENGINE REPORT

Branch:

Transfer Endpoint:

History Endpoints:

Transaction Endpoints:

Ownership Strategy:

Financial Transaction Strategy:

Debit/Credit Strategy:

Ledger Strategy:

Transfer Reference Strategy:

Idempotency Strategy:

Concurrency Strategy:

RowVersion Handling:

Money Conservation Test:

Rollback Test:

Audit Events:

Notification Strategy:

Database Changes:

Migration:

Indexes:

Tests Before:

Tests Added:

Tests Total:

Test Result:

Docker Result:

Live Transfer Result:

Balances Before:

Balances After:

Duplicate Request Result:

History Verification:

Swagger:

Commit:

Push:

Warnings / Known Limitations:
```

Then STOP.

Do not implement Bill Payment.

Do not implement frontend Transfer UI.