# 📖 Module Beneficiaries (Danh bạ Thụ hưởng) API

Module cung cấp các API giúp khách hàng quản lý danh bạ tài khoản thụ hưởng để chuyển tiền nhanh chóng.

---

## 1. Lấy Danh bạ Người thụ hưởng (Get Beneficiaries)

- **Endpoint**: `GET /api/v1/beneficiaries`
- **Quyền truy cập**: `[Authorize(Roles = "CUSTOMER")]`
- **Mô tả**: Trả về danh sách người thụ hưởng đã lưu của khách hàng đang đăng nhập.

### Headers:
```http
Authorization: Bearer <accessToken>
```

### Response `200 OK`:
```json
[
  {
    "id": "11f8caf0-b00e-402d-bb56-55dd4c500945",
    "accountNumber": "1000000002",
    "accountName": "Tran Thi Binh - Checking",
    "nickname": "Chị Bình",
    "createdAtUtc": "2026-08-18T08:14:50.3520259Z"
  }
]
```

---

## 2. Thêm Người thụ hưởng mới (Add Beneficiary)

- **Endpoint**: `POST /api/v1/beneficiaries`
- **Quyền truy cập**: `[Authorize(Roles = "CUSTOMER")]`
- **Mô tả**: Lưu một tài khoản ngân hàng vào danh bạ thụ hưởng.
- **Ràng buộc nghiệp vụ tự động của Backend**:
  - Số tài khoản thụ hưởng phải tồn tại trong hệ thống.
  - Không được thêm tài khoản đang bị `CLOSED`.
  - **Không được tự thêm tài khoản của chính mình** (`400 Bad Request`).
  - **Không được thêm trùng lặp tài khoản đã có trong danh bạ** (`409 Conflict`).

### Headers:
```http
Authorization: Bearer <accessToken>
Content-Type: application/json
```

### Request Body:
```json
{
  "accountNumber": "1000000002",
  "nickname": "Chị Bình"
}
```

### Response `201 Created`:
```json
{
  "id": "11f8caf0-b00e-402d-bb56-55dd4c500945",
  "accountNumber": "1000000002",
  "accountName": "Tran Thi Binh - Checking",
  "nickname": "Chị Bình",
  "createdAtUtc": "2026-08-18T08:14:50.3520259Z"
}
```

### Error Responses:
- `400 Bad Request`: Thêm tài khoản của chính mình hoặc tài khoản đã đóng.
- `404 Not Found`: Số tài khoản thụ hưởng không tồn tại.
- `409 Conflict`: Tài khoản thụ hưởng này đã có sẵn trong danh bạ của bạn.

---

## 3. Xóa Người thụ hưởng (Delete Beneficiary)

- **Endpoint**: `DELETE /api/v1/beneficiaries/{id}`
- **Quyền truy cập**: `[Authorize(Roles = "CUSTOMER")]`
- **Mô tả**: Xóa một bản ghi người thụ hưởng ra khỏi danh bạ của khách hàng.
- **Bảo mật quyền sở hữu**: Khách hàng chỉ có thể xóa người thụ hưởng do chính mình tạo ra.

### Headers:
```http
Authorization: Bearer <accessToken>
```

### Response `200 OK`:
```json
{
  "message": "Beneficiary deleted successfully."
}
```

### Error Responses:
- `404 Not Found`: Không tìm thấy bản ghi người thụ hưởng này trong danh bạ của bạn.

---

## 4. TypeScript Interfaces cho Frontend

```typescript
export interface Beneficiary {
  id: string;
  accountNumber: string;
  accountName: string;
  nickname?: string;
  createdAtUtc: string;
}

export interface CreateBeneficiaryRequest {
  accountNumber: string;
  nickname?: string;
}
```
