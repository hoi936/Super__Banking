# Frontend API Contract (V1)

## Base URL
Development: `http://localhost:8080` (Docker) or `http://localhost:5000` (.NET Local)

## Authentication
- Endpoints under `/api/v1/auth/` do not require authentication (except `/me`).
- All other endpoints require a Bearer token: `Authorization: Bearer <token>`
- Token rotation via `POST /api/v1/auth/refresh` using `refreshToken` and `accessToken`.

## Authorization Roles
- `CUSTOMER`: End users using the bank. Can access `/me`, `/accounts`, `/transfers`, `/bills`, `/payments`, `/notifications`.
- `STAFF`: Bank officers. Can access `GET /api/v1/admin/dashboard`, `GET /admin/customers`, `GET /admin/transactions`, `GET /admin/payments`.
- `ADMIN`: System admins. Full access including `PATCH /admin/users/{id}/status`, `GET /admin/audit-logs`.

## Common Models

### Pagination
```typescript
interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number; // Derived as Math.ceil(totalItems / pageSize)
}
```

### ProblemDetails (Errors)
```typescript
interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  errors?: Record<string, string[]>;
}
```

## Customer Endpoints
- `POST /api/v1/auth/login` -> `{ accessToken, refreshToken, expiresIn }`
- `GET /api/v1/auth/me` -> `{ id, email, status, roles, customerCode, fullName }`

## Account Endpoints
- `GET /api/v1/accounts` -> List of accounts with balances.

## Transfer Endpoints
- `POST /api/v1/transfers` 
  - Required Headers: `Idempotency-Key: <uuid>`
  - Body: `{ sourceAccountId, destinationAccountNumber, amount, description }`

## Transaction Endpoints
- `GET /api/v1/transactions` -> Paginated transaction history.

## Bill & Payment Endpoints
- `GET /api/v1/bills` -> List of bills.
- `POST /api/v1/payments`
  - Required Headers: `Idempotency-Key: <uuid>`
  - Body: `{ billId, accountId }` (Note: Amount is determined by the server based on the bill).

## Notification Endpoints
- `GET /api/v1/notifications` -> Paginated notifications.
- `GET /api/v1/notifications/unread-count` -> `{ count }`
- `PATCH /api/v1/notifications/{id}/read` -> Mark as read.

## Admin Endpoints
- `GET /api/v1/admin/dashboard` -> Aggregated stats.
- `GET /api/v1/admin/customers` -> Paginated customers.
- `GET /api/v1/admin/transactions` -> Paginated all transactions.
- `GET /api/v1/admin/payments` -> Paginated all payments.
- `GET /api/v1/admin/users` -> Paginated all users (Admin only).
- `PATCH /api/v1/admin/users/{id}/status` -> `{ status: number, reason: string }` (Admin only).
- `GET /api/v1/admin/audit-logs` -> Paginated audit logs (Admin only).
