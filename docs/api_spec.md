### 1. Authentication Module (`/api/auth`)

#### 1.1. Đăng ký tài khoản
- **Endpoint**: `POST /api/auth/register`
- **Auth**: Không yêu cầu
- **Description**: Đăng ký tài khoản người dùng mới

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "Password@123",
  "firstName": "Văn A",
  "lastName": "Nguyễn",
  "phone": "0901234567",
  "role": 2
}
```

**Response Success (201):**
```json
{
  "success": true,
  "message": "Đăng ký thành công",
  "statusCode": 201,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "refresh_token_here",
    "expiresIn": 3600,
    "user": {
      "id": 1,
      "email": "user@example.com",
      "firstName": "Văn A",
      "lastName": "Nguyễn",
      "phone": "0901234567",
      "role": 2,
      "status": 0
    }
  }
}
```

**Response Error (400):**
```json
{
  "success": false,
  "message": "Email hoặc số điện thoại đã tồn tại",
  "statusCode": 400,
  "data": null
}
```

---

#### 1.2. Đăng nhập
- **Endpoint**: `POST /api/auth/login`
- **Auth**: Không yêu cầu
- **Description**: Đăng nhập vào hệ thống

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "Password@123"
}
```

**Response Success (200):**
```json
{
  "success": true,
  "message": "Đăng nhập thành công",
  "statusCode": 200,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "refresh_token_here",
    "expiresIn": 3600,
    "user": {
      "id": 1,
      "email": "user@example.com",
      "firstName": "Văn A",
      "lastName": "Nguyễn",
      "role": 2
    }
  }
}
```

**Response Error (401):**
```json
{
  "success": false,
  "message": "Email hoặc mật khẩu không đúng",
  "statusCode": 401,
  "data": null
}
```

---

#### 1.3. Lấy thông tin người dùng hiện tại
- **Endpoint**: `GET /api/auth/profile`
- **Auth**: Required (Bearer Token)
- **Description**: Lấy thông tin profile của user đang đăng nhập

**Response Success (200):**
```json
{
  "success": true,
  "message": "Lấy thông tin thành công",
  "statusCode": 200,
  "data": {
    "id": 1,
    "email": "user@example.com",
    "firstName": "Văn A",
    "lastName": "Nguyễn",
    "phone": "0901234567",
    "role": 2,
    "status": 0,
    "emailVerified": true,
    "createdAt": "2024-01-01T00:00:00Z"
  }
}
```

---

#### 1.4. Admin Only Endpoint (Demo)
- **Endpoint**: `GET /api/auth/admin-only`
- **Auth**: Required (Role: Admin)
- **Description**: Endpoint demo kiểm tra phân quyền Admin

**Response Success (200):**
```json
{
  "success": true,
  "message": "Access granted",
  "statusCode": 200,
  "data": "Welcome Admin!"
}
```

---

### 2. Users Module (`/api/users`)

#### 2.1. Lấy danh sách người dùng (phân trang)
- **Endpoint**: `GET /api/users?pageIndex=1&pageSize=10`
- **Auth**: Required
- **Description**: Lấy danh sách tất cả người dùng có phân trang

**Query Parameters:**
- `pageIndex` (int, default: 1)
- `pageSize` (int, default: 10)

**Response Success (200):**
```json
{
  "success": true,
  "message": "Lấy danh sách người dùng thành công",
  "statusCode": 200,
  "data": [
    {
      "id": 1,
      "email": "user@example.com",
      "firstName": "Văn A",
      "lastName": "Nguyễn",
      "phone": "0901234567",
      "role": 2,
      "status": 0
    }
  ],
  "pageIndex": 1,
  "pageSize": 10,
  "totalRecords": 50,
  "totalPages": 5
}
```

---

#### 2.2. Lấy thông tin người dùng theo ID
- **Endpoint**: `GET /api/users/{id}`
- **Auth**: Required
- **Description**: Lấy chi tiết thông tin người dùng theo ID

**Response Success (200):**
```json
{
  "success": true,
  "message": "Lấy thông tin người dùng thành công",
  "statusCode": 200,
  "data": {
    "id": 1,
    "email": "user@example.com",
    "firstName": "Văn A",
    "lastName": "Nguyễn",
    "phone": "0901234567",
    "role": 2,
    "status": 0,
    "emailVerified": true
  }
}
```

---

#### 2.3. Tạo người dùng mới
- **Endpoint**: `POST /api/users`
- **Auth**: Required
- **Description**: Tạo người dùng mới

**Request Body:**
```json
{
  "email": "newuser@example.com",
  "password": "Password@123",
  "firstName": "Văn B",
  "lastName": "Trần",
  "phone": "0902222222",
  "role": 2
}
```

**Response Success (201):**
```json
{
  "success": true,
  "message": "Tạo người dùng thành công",
  "statusCode": 201,
  "data": {
    "id": 2,
    "email": "newuser@example.com",
    "firstName": "Văn B",
    "lastName": "Trần"
  }
}
```

---

#### 2.4. Cập nhật thông tin người dùng
- **Endpoint**: `PUT /api/users/{id}`
- **Auth**: Required
- **Description**: Cập nhật thông tin người dùng

**Request Body:**
```json
{
  "firstName": "Văn B Updated",
  "lastName": "Trần",
  "phone": "0902222333",
  "status": 0
}
```

**Response Success (200):**
```json
{
  "success": true,
  "message": "Cập nhật người dùng thành công",
  "statusCode": 200,
  "data": ""
}
```

---

#### 2.5. Xóa người dùng (Soft Delete)
- **Endpoint**: `DELETE /api/users/{id}`
- **Auth**: Required
- **Description**: Xóa mềm người dùng

**Response Success (200):**
```json
{
  "success": true,
  "message": "Xóa người dùng thành công",
  "statusCode": 200,
  "data": ""
}
```
