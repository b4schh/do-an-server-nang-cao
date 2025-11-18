# API SPECIFICATION - FOOTBALL FIELD BOOKING SYSTEM

## A. TỔNG QUAN

### 1. Thông tin chung

- **Base URL**: `http://localhost:5000/api` (Development)
- **Version**: 1.0
- **Protocol**: HTTP/HTTPS
- **Framework**: ASP.NET Core 8.0
- **Authentication**: JWT Bearer Token
- **Timezone**: Asia/Ho_Chi_Minh (SE Asia Standard Time)
- **Storage**: MinIO Object Storage

### 2. Mô tả hệ thống

Hệ thống API quản lý đặt sân bóng đá, hỗ trợ các chức năng:

- Quản lý người dùng (Admin, Owner, Customer)
- Quản lý cụm sân và sân con
- Đặt lịch và quản lý booking với luồng thanh toán cọc 3 bước
- Quản lý ảnh cụm sân (MinIO Storage)
- Quản lý khung giờ (time slots) với validation chồng lấp
- Background job tự động hủy booking hết hạn (5 phút)

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
  "meta": null,
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
  "meta": { ... },
  "errors": { ... }
}
```

#### Paged Response

```json
{
  "success": true,
  "message": "Lấy danh sách thành công",
  "statusCode": 200,
  "data": [...],
  "pageIndex": 1,
  "pageSize": 10,
  "totalRecords": 100,
  "totalPages": 10,
  "hasPreviousPage": false,
  "hasNextPage": false,
  "meta": null,
  "errors": null
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

- **Admin** (2): Toàn quyền quản trị hệ thống
- **Owner** (1): Quản lý cụm sân, sân con, booking của mình
- **Customer** (0): Đặt sân, xem thông tin

### 5. Query Parameters cho Pagination

- `pageIndex` (int, default: 1) - Trang hiện tại
- `pageSize` (int, default: 10) - Số bản ghi mỗi trang

### 6. Common Data Types

#### UserRole Enum

```csharp
Customer = 0
Owner = 1
Admin = 2
```

#### UserStatus Enum

```csharp
Inactive = 0
Active = 1
Banned = 2
```

#### BookingStatus Enum

```csharp
Pending = 0                // Khách vừa tạo booking, chưa upload bill (5 phút)
WaitingForApproval = 1     // Khách đã upload bill, chờ chủ sân duyệt
Confirmed = 2              // Chủ sân đã duyệt cọc, giữ sân thành công
Rejected = 3               // Chủ sân từ chối bill
Cancelled = 4              // Khách hoặc chủ sân hủy booking
Completed = 5              // Trận đấu đã diễn ra, thanh toán đầy đủ
Expired = 6                // Hết thời gian giữ chỗ, khách không upload bill (auto by background job)
NoShow = 7                 // Khách không đến sân
```

#### ComplexStatus Enum

```csharp
Pending = 0
Approved = 1
Rejected = 2
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
  "role": 0
}
```

**Response Success (200):**

```json
{
  "success": true,
  "message": "Đăng ký thành công",
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
      "phone": "0901234567",
      "role": 0,
      "status": 1
    }
  }
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
      "role": 0
    }
  }
}
```

---

#### 1.3. Lấy thông tin người dùng hiện tại

- **Endpoint**: `GET /api/auth/profile`
- **Auth**: Required
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
    "role": 0,
    "status": 1,
    "createdAt": "2024-01-01T00:00:00Z"
  }
}
```

---

#### 1.4. Admin Only Endpoint (Demo)

- **Endpoint**: `GET /api/auth/admin-only`
- **Auth**: Required (Role: Admin)
- **Description**: Endpoint demo kiểm tra phân quyền Admin

---

### 2. Users Module (`/api/users`)

#### 2.1. Lấy danh sách người dùng (phân trang)

- **Endpoint**: `GET /api/users?pageIndex=1&pageSize=10`
- **Auth**: Required
- **Description**: Lấy danh sách tất cả người dùng có phân trang

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
      "role": 0,
      "status": 1
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
  "role": 0
}
```

---

#### 2.4. Cập nhật thông tin người dùng

- **Endpoint**: `PUT /api/users/{id}`
- **Auth**: Required
- **Description**: Cập nhật thông tin người dùng

---

#### 2.5. Cập nhật role người dùng

- **Endpoint**: `PATCH /api/users/{id}/role`
- **Auth**: Required (Role: Admin)
- **Description**: Cập nhật role của người dùng (chỉ Admin)

**Request Body:**

```json
{
  "role": 1
}
```

---

#### 2.6. Xóa người dùng

- **Endpoint**: `DELETE /api/users/{id}`
- **Auth**: Required
- **Description**: Xóa mềm người dùng

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
      "description": "Sân bóng chất lượng cao",
      "status": 1,
      "isActive": true
    }
  ],
  "pageIndex": 1,
  "pageSize": 10,
  "totalPages": 1,
  "totalRecords": 3
}
```

---

#### 3.2. Lấy thông tin cụm sân theo ID

- **Endpoint**: `GET /api/complexes/{id}`
- **Auth**: Không yêu cầu
- **Description**: Lấy chi tiết cụm sân theo ID

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
      }
    ]
  }
}
```

---

#### 3.4. Lấy cụm sân với thông tin đầy đủ (fields + timeslots + availability)

- **Endpoint**: `GET /api/complexes/{id}/full-details?date=2024-01-10`
- **Auth**: Không yêu cầu
- **Description**: Lấy thông tin cụm sân kèm fields, timeslots và trạng thái available/booked cho ngày cụ thể

**Query Parameters:**

- `date` (DateTime, optional) - Ngày kiểm tra trạng thái booking (default: today)

**Response Success (200):**

```json
{
  "success": true,
  "message": "Lấy thông tin sân đầy đủ thành công",
  "statusCode": 200,
  "data": {
    "id": 1,
    "name": "Sân Bóng Thành Phố",
    "fields": [
      {
        "id": 1,
        "name": "Sân 1",
        "timeSlots": [
          {
            "id": 1,
            "startTime": "06:00:00",
            "endTime": "08:00:00",
            "price": 240000,
            "isActive": true,
            "isAvailable": true
          }
        ]
      }
    ]
  }
}
```

---

#### 3.5. Tìm kiếm cụm sân

- **Endpoint**: `GET /api/complexes/search`
- **Auth**: Không yêu cầu
- **Description**: Tìm kiếm cụm sân theo nhiều tiêu chí

**Query Parameters:**

- `name` (string, optional)
- `street` (string, optional)
- `ward` (string, optional)
- `province` (string, optional)
- `minPrice` (decimal, optional)
- `maxPrice` (decimal, optional)
- `minRating` (double, optional)
- `maxRating` (double, optional)
- `pageIndex` (int, default: 1)
- `pageSize` (int, default: 10)

---

#### 3.6. Lấy danh sách cụm sân của Owner hiện tại

- **Endpoint**: `GET /api/complexes/owner/my-complexes`
- **Auth**: Required (Role: Owner)
- **Description**: Owner lấy danh sách cụm sân của mình

---

#### 3.7. Lấy danh sách cụm sân theo Owner ID (Admin only)

- **Endpoint**: `GET /api/complexes/admin/owner/{ownerId}`
- **Auth**: Required (Role: Admin)
- **Description**: Admin lấy danh sách cụm sân của một Owner cụ thể

---

#### 3.8. Tạo cụm sân mới (Owner)

- **Endpoint**: `POST /api/complexes/owner`
- **Auth**: Required (Role: Owner)
- **Description**: Owner tạo cụm sân mới (tự động lấy ownerId từ JWT)

**Request Body:**

```json
{
  "name": "Sân Bóng Mới",
  "street": "456 Lê Lợi",
  "ward": "Bến Thành",
  "province": "Hồ Chí Minh",
  "phone": "0281111111",
  "openingTime": "06:00:00",
  "closingTime": "23:00:00",
  "description": "Sân bóng hiện đại"
}
```

---

#### 3.9. Tạo cụm sân mới (Admin)

- **Endpoint**: `POST /api/complexes/admin`
- **Auth**: Required (Role: Admin)
- **Description**: Admin tạo cụm sân mới cho Owner bất kỳ

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
  "description": "Sân bóng hiện đại"
}
```

---

#### 3.10. Cập nhật cụm sân

- **Endpoint**: `PUT /api/complexes/{id}`
- **Auth**: Required (Role: Admin, Owner)
- **Description**: Cập nhật thông tin cụm sân

---

#### 3.11. Xóa cụm sân

- **Endpoint**: `DELETE /api/complexes/{id}`
- **Auth**: Required (Role: Admin, Owner)
- **Description**: Xóa mềm cụm sân

---

### 4. Complex Images Module (`/api/complex-images`)

#### 4.1. Upload ảnh cụm sân

- **Endpoint**: `POST /api/complex-images/{complexId}/upload`
- **Auth**: Required (Role: Admin, Owner)
- **Description**: Upload ảnh cho cụm sân lên MinIO storage
- **Content-Type**: `multipart/form-data`

**Request Form Data:**

- `file` (IFormFile, required) - File ảnh (JPEG, PNG, WEBP, max 5MB)
- `description` (string, optional) - Mô tả ảnh

**Response Success (200):**

```json
{
  "success": true,
  "message": "Upload ảnh thành công!",
  "statusCode": 200,
  "data": {
    "id": 1,
    "complexId": 1,
    "imageUrl": "http://localhost:9000/football-field-images/complexes/complex-1-xxx.webp",
    "description": "Ảnh sân chính"
  }
}
```

---

#### 4.2. Lấy danh sách ảnh cụm sân

- **Endpoint**: `GET /api/complex-images/{complexId}`
- **Auth**: Không yêu cầu
- **Description**: Lấy danh sách tất cả ảnh của cụm sân

**Response Success (200):**

```json
{
  "success": true,
  "message": "Lấy danh sách ảnh thành công!",
  "statusCode": 200,
  "data": [
    {
      "id": 1,
      "complexId": 1,
      "imageUrl": "http://localhost:9000/football-field-images/complexes/complex-1-xxx.webp",
      "description": "Ảnh sân chính"
    }
  ]
}
```

---

#### 4.3. Xóa ảnh cụm sân

- **Endpoint**: `DELETE /api/complex-images/{imageId}`
- **Auth**: Required (Role: Admin, Owner)
- **Description**: Xóa ảnh khỏi cụm sân và MinIO storage

---

### 5. Fields Module (`/api/fields`)

#### 5.1. Lấy danh sách sân con (phân trang)

- **Endpoint**: `GET /api/fields?pageIndex=1&pageSize=10`
- **Auth**: Không yêu cầu
- **Description**: Lấy danh sách tất cả sân con

---

#### 5.2. Lấy thông tin sân con theo ID

- **Endpoint**: `GET /api/fields/{id}`
- **Auth**: Không yêu cầu
- **Description**: Lấy chi tiết sân con theo ID

---

#### 5.3. Lấy sân con với danh sách khung giờ

- **Endpoint**: `GET /api/fields/{id}/with-timeslots`
- **Auth**: Không yêu cầu
- **Description**: Lấy thông tin sân con kèm danh sách khung giờ

---

#### 5.4. Lấy sân con theo Complex ID

- **Endpoint**: `GET /api/fields/complex/{complexId}`
- **Auth**: Không yêu cầu
- **Description**: Lấy danh sách sân con của một cụm sân

---

#### 5.5. Tạo sân con mới

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

---

#### 5.6. Cập nhật sân con

- **Endpoint**: `PUT /api/fields/{id}`
- **Auth**: Required (Role: Admin, Owner)
- **Description**: Cập nhật thông tin sân con

---

#### 5.7. Xóa sân con

- **Endpoint**: `DELETE /api/fields/{id}`
- **Auth**: Required (Role: Admin, Owner)
- **Description**: Xóa mềm sân con

---

### 6. TimeSlots Module (`/api/timeslots`)

#### 6.1. Lấy danh sách khung giờ

- **Endpoint**: `GET /api/timeslots`
- **Auth**: Không yêu cầu
- **Description**: Lấy danh sách tất cả khung giờ

---

#### 6.2. Lấy khung giờ theo ID

- **Endpoint**: `GET /api/timeslots/{id}`
- **Auth**: Không yêu cầu
- **Description**: Lấy chi tiết khung giờ theo ID

---

#### 6.3. Lấy khung giờ theo Field ID

- **Endpoint**: `GET /api/timeslots/field/{fieldId}`
- **Auth**: Không yêu cầu
- **Description**: Lấy danh sách khung giờ của một sân con

---

#### 6.4. Tạo khung giờ mới

- **Endpoint**: `POST /api/timeslots`
- **Auth**: Required (Role: Admin, Owner)
- **Description**: Tạo khung giờ mới cho sân (có validation chồng lấp thời gian)

**Request Body:**

```json
{
  "fieldId": 1,
  "startTime": "22:00:00",
  "endTime": "00:00:00",
  "price": 350000
}
```

**Response Error (400) - Nếu trùng giờ:**

```json
{
  "success": false,
  "message": "Khung giờ bị trùng với khung giờ hiện tại từ 20:00:00 đến 22:00:00",
  "statusCode": 400,
  "data": null
}
```

---

#### 6.5. Cập nhật khung giờ

- **Endpoint**: `PUT /api/timeslots/{id}`
- **Auth**: Required (Role: Admin, Owner)
- **Description**: Cập nhật thông tin khung giờ (có validation chồng lấp)

---

#### 6.6. Xóa khung giờ

- **Endpoint**: `DELETE /api/timeslots/{id}`
- **Auth**: Required (Role: Admin, Owner)
- **Description**: Xóa khung giờ

---

### 7. Bookings Module (`/api/bookings`)

> **QUAN TRỌNG**: Luồng booking gồm 3 bước:
>
> 1. Customer tạo booking (status: Pending) - có 5 phút để upload bill
> 2. Customer upload bill thanh toán (status: WaitingForApproval)
> 3. Owner duyệt hoặc từ chối (status: Confirmed hoặc Rejected)

#### 7.1. Tạo booking mới (Bước 1)

- **Endpoint**: `POST /api/bookings`
- **Auth**: Required (Role: Customer)
- **Description**: Khách hàng tạo booking mới, có 5 phút để upload bill

**Request Body:**

```json
{
  "fieldId": 1,
  "timeSlotId": 4,
  "bookingDate": "2024-01-15",
  "note": "Ghi chú đặt sân"
}
```

**Response Success (201):**

```json
{
  "success": true,
  "message": "Tạo booking thành công. Vui lòng upload bill thanh toán trong 5 phút.",
  "statusCode": 201,
  "data": {
    "id": 1,
    "fieldId": 1,
    "fieldName": "Sân 1",
    "complexId": 1,
    "complexName": "Sân Bóng Thành Phố",
    "customerId": 5,
    "customerName": "Nguyễn Văn A",
    "timeSlotId": 4,
    "startTime": "08:00:00",
    "endTime": "10:00:00",
    "bookingDate": "2024-01-15T00:00:00Z",
    "holdExpiresAt": "2024-01-15T10:05:00Z",
    "totalAmount": 260000,
    "depositAmount": 78000,
    "bookingStatus": 0,
    "bookingStatusText": "Pending"
  }
}
```

**Response Error (400) - Khung giờ đã được đặt:**

```json
{
  "success": false,
  "message": "Khung giờ này đã được đặt cho ngày đã chọn",
  "statusCode": 400,
  "data": null
}
```

---

#### 7.2. Upload bill thanh toán (Bước 2)

- **Endpoint**: `POST /api/bookings/{id}/upload-payment`
- **Auth**: Required (Role: Customer)
- **Description**: Khách hàng upload bill thanh toán cọc
- **Content-Type**: `multipart/form-data`

**Request Form Data:**

- `file` (IFormFile, required) - File ảnh bill (JPEG, PNG, WEBP, max 5MB)

**Response Success (200):**

```json
{
  "success": true,
  "message": "Upload bill thanh toán thành công. Đang chờ chủ sân duyệt.",
  "statusCode": 200,
  "data": {
    "id": 1,
    "bookingStatus": 1,
    "bookingStatusText": "WaitingForApproval",
    "paymentProofUrl": "http://localhost:9000/football-field-images/bookings/booking-1-xxx.webp"
  }
}
```

---

#### 7.3. Duyệt booking (Bước 3)

- **Endpoint**: `POST /api/bookings/{id}/approve`
- **Auth**: Required (Role: Owner)
- **Description**: Chủ sân duyệt booking

**Response Success (200):**

```json
{
  "success": true,
  "message": "Duyệt booking thành công",
  "statusCode": 200,
  "data": {
    "id": 1,
    "bookingStatus": 2,
    "bookingStatusText": "Confirmed",
    "approvedBy": 2,
    "approvedByName": "Nguyễn Văn Chủ",
    "approvedAt": "2024-01-15T10:30:00Z"
  }
}
```

---

#### 7.4. Từ chối booking

- **Endpoint**: `POST /api/bookings/{id}/reject`
- **Auth**: Required (Role: Owner)
- **Description**: Chủ sân từ chối booking

**Request Body:**

```json
{
  "reason": "Bill thanh toán không hợp lệ"
}
```

---

#### 7.5. Hủy booking

- **Endpoint**: `POST /api/bookings/{id}/cancel`
- **Auth**: Required (Customer hoặc Owner)
- **Description**: Hủy booking (Customer hoặc Owner đều có thể hủy)

---

#### 7.6. Đánh dấu hoàn thành

- **Endpoint**: `POST /api/bookings/{id}/complete`
- **Auth**: Required (Role: Owner)
- **Description**: Chủ sân đánh dấu booking đã hoàn thành

---

#### 7.7. Đánh dấu không đến

- **Endpoint**: `POST /api/bookings/{id}/no-show`
- **Auth**: Required (Role: Owner)
- **Description**: Chủ sân đánh dấu khách không đến sân

---

#### 7.8. Lấy danh sách booking của Customer

- **Endpoint**: `GET /api/bookings/my-bookings?status=2`
- **Auth**: Required (Role: Customer)
- **Description**: Customer xem danh sách booking của mình

**Query Parameters:**

- `status` (BookingStatus, optional) - Lọc theo trạng thái

**Response Success (200):**

```json
{
  "success": true,
  "message": "Lấy danh sách booking thành công",
  "statusCode": 200,
  "data": [
    {
      "id": 1,
      "fieldName": "Sân 1",
      "complexName": "Sân Bóng Thành Phố",
      "bookingDate": "2024-01-15T00:00:00Z",
      "startTime": "08:00:00",
      "endTime": "10:00:00",
      "totalAmount": 260000,
      "bookingStatus": 2,
      "bookingStatusText": "Confirmed"
    }
  ]
}
```

---

#### 7.9. Lấy danh sách booking của Owner

- **Endpoint**: `GET /api/bookings/owner-bookings?status=1`
- **Auth**: Required (Role: Owner)
- **Description**: Owner xem danh sách booking của các sân mình

**Query Parameters:**

- `status` (BookingStatus, optional) - Lọc theo trạng thái

---

#### 7.10. Lấy chi tiết booking

- **Endpoint**: `GET /api/bookings/{id}`
- **Auth**: Required
- **Description**: Xem chi tiết một booking

---

## D. LƯU Ý QUAN TRỌNG

### 1. Luồng Booking

#### Bước 1: Tạo Booking

- Customer POST `/api/bookings`
- Status: `Pending` (0)
- Có 5 phút để upload bill
- Background job sẽ tự động chuyển sang `Expired` (6) nếu hết 5 phút

#### Bước 2: Upload Bill

- Customer POST `/api/bookings/{id}/upload-payment`
- Status: `WaitingForApproval` (1)
- Chờ Owner duyệt

#### Bước 3: Duyệt/Từ chối

- Owner POST `/api/bookings/{id}/approve` → Status: `Confirmed` (2)
- Owner POST `/api/bookings/{id}/reject` → Status: `Rejected` (3)

#### Các trạng thái khác:

- `Cancelled` (4): Customer hoặc Owner hủy
- `Completed` (5): Owner đánh dấu đã hoàn thành
- `Expired` (6): Tự động bởi background job
- `NoShow` (7): Owner đánh dấu khách không đến

### 2. Storage (MinIO)

- Endpoint: `http://localhost:9000`
- Bucket: `football-field-images`
- Folders:
  - `complexes/` - Ảnh cụm sân
  - `bookings/` - Bill thanh toán
- File types: JPEG, PNG, WEBP
- Max size: 5MB

### 3. Background Jobs

- **BookingExpirationBackgroundService**: Chạy mỗi 1 phút, tự động hủy các booking Pending quá 5 phút

### 4. Timezone

- Tất cả DateTime đều sử dụng timezone: **Asia/Ho_Chi_Minh** (UTC+7)

### 5. Validation Rules

#### Password Requirements

- Tối thiểu 8 ký tự
- Chứa ít nhất 1 chữ hoa
- Chứa ít nhất 1 chữ thường
- Chứa ít nhất 1 số
- Chứa ít nhất 1 ký tự đặc biệt

#### TimeSlot Validation

- Không được trùng lặp trong cùng Field
- `startTime` < `endTime`
- Nằm trong giờ mở cửa của Complex

---

## E. LỖI VÀ KHẮC PHỤC SỰ CỐ

### Common Error Codes

| Status Code | Ý nghĩa               | Giải pháp                                   |
| ----------- | --------------------- | ------------------------------------------- |
| 400         | Bad Request           | Kiểm tra lại request body, query parameters |
| 401         | Unauthorized          | Đăng nhập lại hoặc kiểm tra JWT token       |
| 403         | Forbidden             | Không đủ quyền, kiểm tra role               |
| 404         | Not Found             | Resource không tồn tại                      |
| 500         | Internal Server Error | Liên hệ admin, kiểm tra logs                |

---

## F. LỊCH SỬ THAY ĐỔI

### Version 1.0 (2024-11-16)

- Initial API release
- 7 modules chính: Auth, Users, Complexes, ComplexImages, Fields, TimeSlots, Bookings
- JWT Authentication
- Role-based Authorization
- Pagination support
- MinIO Storage integration
- Background job cho booking expiration
- Timezone support (Asia/Ho_Chi_Minh)
