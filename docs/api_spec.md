# API SPECIFICATION - FOOTBALL FIELD BOOKING SYSTEM

## A. TỔNG QUAN

### 1. Thông tin chung
- **Base URL**: `http://localhost:5000/api` (Development)
- **Version**: 1.0
- **Protocol**: HTTP/HTTPS
- **Framework**: ASP.NET Core 8.0
- **Authentication**: JWT Bearer Token

### 2. Mô tả hệ thống
Hệ thống API quản lý đặt sân bóng đá, hỗ trợ các chức năng:
- Quản lý người dùng (Admin, Owner, Customer)
- Quản lý cụm sân và sân con
- Đặt lịch và quản lý booking
- Đánh giá và review sân
- Thông báo và push notification
- Quản lý khung giờ (time slots)

---

## B. QUY ƯỚC CHUNG

### 1. Format Response

#### Success Response
```json
{
  "success": true,
  "message": "Thông điệp thành công",
  "statusCode": 200,
  "data": { ... },
  "meta": { ... },
  "errors": null
}
```

#### Error Response
```json
{
  "success": false,
  "message": "Thông điệp lỗi",
  "statusCode": 400,
  "data": null,
  "meta": null,
  "errors": ["Chi tiết lỗi 1", "Chi tiết lỗi 2"]
}
```

#### Paged Response
```json
{
  "success": true,
  "message": "Lấy danh sách thành công",
  "statusCode": 200,
  "data": [...],
  "meta": {
    "totalRecords": 100,
    "totalPages": 10,
    "pageIndex": 1,
    "pageSize": 10
  },
  "pageIndex": 1,
  "pageSize": 10,
  "totalRecords": 100,
  "totalPages": 10,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

### 2. HTTP Status Codes
- `200 OK` - Thành công
- `201 Created` - Tạo mới thành công
- `400 Bad Request` - Lỗi validation hoặc dữ liệu không hợp lệ
- `401 Unauthorized` - Chưa đăng nhập hoặc token không hợp lệ
- `403 Forbidden` - Không có quyền truy cập
- `404 Not Found` - Không tìm thấy tài nguyên
- `500 Internal Server Error` - Lỗi server

### 3. Authentication
**Header yêu cầu cho các endpoint cần xác thực:**
```
Authorization: Bearer {JWT_TOKEN}
```

### 4. Roles & Permissions
- **Admin**: Toàn quyền quản trị hệ thống
- **Owner**: Quản lý cụm sân, sân con, booking của mình
- **Customer**: Đặt sân, đánh giá, xem thông tin

### 5. Query Parameters cho Pagination
- `pageIndex` (int, default: 1) - Trang hiện tại
- `pageSize` (int, default: 10) - Số bản ghi mỗi trang

### 6. Common Data Types

#### UserRole Enum
```
Admin = 0
Owner = 1
Customer = 2
```

#### UserStatus Enum
```
Active = 0
Inactive = 1
Suspended = 2
Banned = 3
PendingVerification = 4
```

#### BookingStatus Enum
```
Pending = 0
Confirmed = 1
Cancelled = 2
Completed = 3
NoShow = 4
```

#### PaymentStatus Enum
```
Unpaid = 0
DepositPaid = 1
FullyPaid = 2
Refunded = 3
```

#### ComplexStatus Enum
```
Pending = 0
Approved = 1
Rejected = 2
Suspended = 3
```

#### NotificationType Enum
```
Booking = 0
Payment = 1
Promotion = 2
System = 3
Review = 4
```

---

## C. DANH SÁCH ENDPOINT

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

---

### 3. Complexes Module (`/api/complexes`)

#### 3.1. Lấy danh sách cụm sân (phân trang)
- **Endpoint**: `GET /api/complexes?pageIndex=1&pageSize=10`
- **Auth**: Không yêu cầu
- **Description**: Lấy danh sách tất cả cụm sân

**Response Success (200):**
```json
{
  "success": true,
  "message": "Lấy danh sách sân thành công",
  "statusCode": 200,
  "data": [
    {
      "id": 1,
      "ownerId": 2,
      "name": "Sân Bóng Thành Phố",
      "street": "123 Nguyễn Huệ",
      "ward": "Bến Nghé",
      "province": "Hồ Chí Minh",
      "phone": "0281234567",
      "openingTime": "06:00:00",
      "closingTime": "22:00:00",
      "latitude": 10.7769,
      "longitude": 106.7009,
      "description": "Sân bóng chất lượng cao",
      "status": 1,
      "isActive": true
    }
  ],
  "pageIndex": 1,
  "pageSize": 10,
  "totalRecords": 3
}
```

---

#### 3.2. Lấy thông tin cụm sân theo ID
- **Endpoint**: `GET /api/complexes/{id}`
- **Auth**: Không yêu cầu
- **Description**: Lấy chi tiết cụm sân theo ID

**Response Success (200):**
```json
{
  "success": true,
  "message": "Lấy thông tin sân thành công",
  "statusCode": 200,
  "data": {
    "id": 1,
    "name": "Sân Bóng Thành Phố",
    "street": "123 Nguyễn Huệ",
    "ward": "Bến Nghé",
    "province": "Hồ Chí Minh",
    "phone": "0281234567",
    "openingTime": "06:00:00",
    "closingTime": "22:00:00",
    "status": 1
  }
}
```

---

#### 3.3. Lấy cụm sân với danh sách sân con
- **Endpoint**: `GET /api/complexes/{id}/with-fields`
- **Auth**: Không yêu cầu
- **Description**: Lấy thông tin cụm sân kèm danh sách sân con

**Response Success (200):**
```json
{
  "success": true,
  "message": "Lấy thông tin sân thành công",
  "statusCode": 200,
  "data": {
    "id": 1,
    "name": "Sân Bóng Thành Phố",
    "street": "123 Nguyễn Huệ",
    "fields": [
      {
        "id": 1,
        "name": "Sân 1",
        "surfaceType": "Cỏ nhân tạo",
        "fieldSize": "5v5",
        "isActive": true
      },
      {
        "id": 2,
        "name": "Sân 2",
        "surfaceType": "Cỏ tự nhiên",
        "fieldSize": "7v7",
        "isActive": true
      }
    ]
  }
}
```

---

#### 3.4. Lấy cụm sân theo Owner ID
- **Endpoint**: `GET /api/complexes/owner/{ownerId}`
- **Auth**: Required
- **Description**: Lấy danh sách cụm sân của một owner

**Response Success (200):**
```json
{
  "success": true,
  "message": "Lấy danh sách sân thành công",
  "statusCode": 200,
  "data": [
    {
      "id": 1,
      "ownerId": 2,
      "name": "Sân Bóng Thành Phố"
    }
  ]
}
```

---

#### 3.5. Tạo cụm sân mới
- **Endpoint**: `POST /api/complexes`
- **Auth**: Required (Role: Admin, Owner)
- **Description**: Tạo cụm sân mới

**Request Body:**
```json
{
  "ownerId": 2,
  "name": "Sân Bóng Mới",
  "street": "456 Lê Lợi",
  "ward": "Bến Thành",
  "province": "Hồ Chí Minh",
  "phone": "0281111111",
  "openingTime": "06:00:00",
  "closingTime": "23:00:00",
  "latitude": 10.7769,
  "longitude": 106.7009,
  "description": "Sân bóng hiện đại"
}
```

**Response Success (201):**
```json
{
  "success": true,
  "message": "Tạo sân thành công",
  "statusCode": 201,
  "data": {
    "id": 4,
    "name": "Sân Bóng Mới"
  }
}
```

---

#### 3.6. Cập nhật cụm sân
- **Endpoint**: `PUT /api/complexes/{id}`
- **Auth**: Required (Role: Admin, Owner)
- **Description**: Cập nhật thông tin cụm sân

**Request Body:**
```json
{
  "name": "Sân Bóng Mới Updated",
  "street": "456 Lê Lợi",
  "phone": "0281111222",
  "openingTime": "05:30:00",
  "closingTime": "23:30:00"
}
```

**Response Success (200):**
```json
{
  "success": true,
  "message": "Cập nhật sân thành công",
  "statusCode": 200,
  "data": null
}
```

---

#### 3.7. Xóa cụm sân (Soft Delete)
- **Endpoint**: `DELETE /api/complexes/{id}`
- **Auth**: Required (Role: Admin, Owner)
- **Description**: Xóa mềm cụm sân

**Response Success (200):**
```json
{
  "success": true,
  "message": "Xóa sân thành công",
  "statusCode": 200,
  "data": null
}
```

---

#### 3.8 Lấy ảnh cụm sân
- **Endpoint**: `GET /api/complex-images/{complexid}`
- **Auth**: Required (Role: Admin, Owner)
- **Description**: Lấy ảnh cụm sân
**Response Success (200):**
```Json
{
  "success": true,
  "message": "Lấy danh sách ảnh thành công!",
  "statusCode": 200,
  "data": [],
  "meta": null,
  "errors": null
}
```
---

### 4. Fields Module (`/api/fields`)

#### 4.1. Lấy danh sách sân con (phân trang)
- **Endpoint**: `GET /api/fields?pageIndex=1&pageSize=10`
- **Auth**: Không yêu cầu
- **Description**: Lấy danh sách tất cả sân con

**Response Success (200):**
```json
{
  "success": true,
  "message": "Lấy danh sách sân con thành công",
  "statusCode": 200,
  "data": [
    {
      "id": 1,
      "complexId": 1,
      "name": "Sân 1",
      "surfaceType": "Cỏ nhân tạo",
      "fieldSize": "5v5",
      "isActive": true
    }
  ],
  "pageIndex": 1,
  "pageSize": 10,
  "totalRecords": 12
}
```

---

#### 4.2. Lấy thông tin sân con theo ID
- **Endpoint**: `GET /api/fields/{id}`
- **Auth**: Không yêu cầu
- **Description**: Lấy chi tiết sân con theo ID

**Response Success (200):**
```json
{
  "success": true,
  "message": "Lấy thông tin sân con thành công",
  "statusCode": 200,
  "data": {
    "id": 1,
    "complexId": 1,
    "name": "Sân 1",
    "surfaceType": "Cỏ nhân tạo",
    "fieldSize": "5v5",
    "isActive": true
  }
}
```

---

#### 4.3. Lấy sân con với danh sách khung giờ
- **Endpoint**: `GET /api/fields/{id}/with-timeslots`
- **Auth**: Không yêu cầu
- **Description**: Lấy thông tin sân con kèm danh sách khung giờ

**Response Success (200):**
```json
{
  "success": true,
  "message": "Lấy thông tin sân con thành công",
  "statusCode": 200,
  "data": {
    "id": 1,
    "name": "Sân 1",
    "fieldSize": "5v5",
    "timeSlots": [
      {
        "id": 1,
        "startTime": "06:00:00",
        "endTime": "08:00:00",
        "price": 240000,
        "isActive": true
      },
      {
        "id": 2,
        "startTime": "08:00:00",
        "endTime": "10:00:00",
        "price": 300000,
        "isActive": true
      }
    ]
  }
}
```

---

#### 4.4. Lấy sân con theo Complex ID
- **Endpoint**: `GET /api/fields/complex/{complexId}`
- **Auth**: Không yêu cầu
- **Description**: Lấy danh sách sân con của một cụm sân

**Response Success (200):**
```json
{
  "success": true,
  "message": "Lấy danh sách sân con thành công",
  "statusCode": 200,
  "data": [
    {
      "id": 1,
      "name": "Sân 1",
      "fieldSize": "5v5"
    },
    {
      "id": 2,
      "name": "Sân 2",
      "fieldSize": "7v7"
    }
  ]
}
```

---

#### 4.5. Tạo sân con mới
- **Endpoint**: `POST /api/fields`
- **Auth**: Required (Role: Admin, Owner)
- **Description**: Tạo sân con mới

**Request Body:**
```json
{
  "complexId": 1,
  "name": "Sân 5",
  "surfaceType": "Cỏ nhân tạo",
  "fieldSize": "11v11"
}
```

**Response Success (201):**
```json
{
  "success": true,
  "message": "Tạo sân con thành công",
  "statusCode": 201,
  "data": {
    "id": 13,
    "name": "Sân 5"
  }
}
```

---

#### 4.6. Cập nhật sân con
- **Endpoint**: `PUT /api/fields/{id}`
- **Auth**: Required (Role: Admin, Owner)
- **Description**: Cập nhật thông tin sân con

**Request Body:**
```json
{
  "name": "Sân 5 Updated",
  "surfaceType": "Cỏ tự nhiên",
  "isActive": true
}
```

**Response Success (200):**
```json
{
  "success": true,
  "message": "Cập nhật sân con thành công",
  "statusCode": 200,
  "data": null
}
```

---

#### 4.7. Xóa sân con (Soft Delete)
- **Endpoint**: `DELETE /api/fields/{id}`
- **Auth**: Required (Role: Admin, Owner)
- **Description**: Xóa mềm sân con

**Response Success (200):**
```json
{
  "success": true,
  "message": "Xóa sân con thành công",
  "statusCode": 200,
  "data": null
}
```

---

### 5. TimeSlots Module (`/api/timeslots`)

#### 5.1. Lấy danh sách khung giờ
- **Endpoint**: `GET /api/timeslots`
- **Auth**: Không yêu cầu
- **Description**: Lấy danh sách tất cả khung giờ

**Response Success (200):**
```json
{
  "success": true,
  "message": "Lấy danh sách khung giờ thành công",
  "statusCode": 200,
  "data": [
    {
      "id": 1,
      "fieldId": 1,
      "startTime": "06:00:00",
      "endTime": "08:00:00",
      "price": 240000,
      "isActive": true
    }
  ]
}
```

---

#### 5.2. Lấy khung giờ theo ID
- **Endpoint**: `GET /api/timeslots/{id}`
- **Auth**: Không yêu cầu
- **Description**: Lấy chi tiết khung giờ theo ID

**Response Success (200):**
```json
{
  "success": true,
  "message": "Lấy thông tin khung giờ thành công",
  "statusCode": 200,
  "data": {
    "id": 1,
    "fieldId": 1,
    "startTime": "06:00:00",
    "endTime": "08:00:00",
    "price": 240000,
    "isActive": true
  }
}
```

---

#### 5.3. Lấy khung giờ theo Field ID
- **Endpoint**: `GET /api/timeslots/field/{fieldId}`
- **Auth**: Không yêu cầu
- **Description**: Lấy danh sách khung giờ của một sân con

**Response Success (200):**
```json
{
  "success": true,
  "message": "Lấy danh sách khung giờ thành công",
  "statusCode": 200,
  "data": [
    {
      "id": 1,
      "startTime": "06:00:00",
      "endTime": "08:00:00",
      "price": 240000
    },
    {
      "id": 2,
      "startTime": "08:00:00",
      "endTime": "10:00:00",
      "price": 300000
    }
  ]
}
```

---

#### 5.4. Tạo khung giờ mới
- **Endpoint**: `POST /api/timeslots`
- **Auth**: Required (Role: Admin, Owner)
- **Description**: Tạo khung giờ mới cho sân

**Request Body:**
```json
{
  "fieldId": 1,
  "startTime": "22:00:00",
  "endTime": "00:00:00",
  "price": 350000
}
```

**Response Success (201):**
```json
{
  "success": true,
  "message": "Tạo khung giờ thành công",
  "statusCode": 201,
  "data": {
    "id": 61,
    "startTime": "22:00:00",
    "endTime": "00:00:00",
    "price": 350000
  }
}
```

---

#### 5.5. Cập nhật khung giờ
- **Endpoint**: `PUT /api/timeslots/{id}`
- **Auth**: Required (Role: Admin, Owner)
- **Description**: Cập nhật thông tin khung giờ

**Request Body:**
```json
{
  "startTime": "22:00:00",
  "endTime": "23:30:00",
  "price": 380000,
  "isActive": true
}
```

**Response Success (200):**
```json
{
  "success": true,
  "message": "Cập nhật khung giờ thành công",
  "statusCode": 200,
  "data": null
}
```

---

#### 5.6. Xóa khung giờ
- **Endpoint**: `DELETE /api/timeslots/{id}`
- **Auth**: Required (Role: Admin, Owner)
- **Description**: Xóa khung giờ

**Response Success (200):**
```json
{
  "success": true,
  "message": "Xóa khung giờ thành công",
  "statusCode": 200,
  "data": null
}
```

---

## D. ERRORS & TROUBLESHOOTING

### Common Error Codes
| Status Code | Ý nghĩa | Giải pháp |
|-------------|---------|-----------|
| 400 | Bad Request | Kiểm tra lại request body, query parameters |
| 401 | Unauthorized | Đăng nhập lại hoặc kiểm tra JWT token |
| 403 | Forbidden | Không đủ quyền, kiểm tra role |
| 404 | Not Found | Resource không tồn tại |
| 500 | Internal Server Error | Liên hệ admin, kiểm tra logs |

### Validation Rules

#### Password Requirements
- Tối thiểu 8 ký tự
- Chứa ít nhất 1 chữ hoa
- Chứa ít nhất 1 chữ thường
- Chứa ít nhất 1 số
- Chứa ít nhất 1 ký tự đặc biệt

#### Email
- Định dạng email hợp lệ
- Không được trùng trong hệ thống

#### Phone
- 10-11 số
- Bắt đầu bằng số 0
- Không được trùng trong hệ thống

---

## E. CHANGELOG

### Version 1.0 (2024-01-01)
- Initial API release
- 8 modules chính: Auth, Users, Complexes, Fields, TimeSlots, Bookings, Reviews, Notifications
- JWT Authentication
- Role-based Authorization
- Pagination support

---

## F. NOTES

### Security Notes
- Tất cả password được hash bằng BCrypt
- JWT token có thời gian hết hạn
- Sensitive data không được log
- CORS được cấu hình theo environment

---

