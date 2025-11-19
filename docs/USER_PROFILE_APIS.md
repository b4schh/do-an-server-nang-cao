# User Profile APIs - Cập nhật & Cải tiến

## Tổng quan

Tài liệu này mô tả 3 APIs mới được thêm vào module User để quản lý thông tin cá nhân:
1. **Upload Avatar** - Tải lên ảnh đại diện
2. **Update Profile** - Cập nhật thông tin cá nhân
3. **Change Password** - Đổi mật khẩu

## Các thay đổi đã thực hiện

### 1. UserController (`Controllers/UserController.cs`)

#### ✅ Sửa lỗi & Cải tiến:

**Constructor:**
- ✅ **Thêm** `IStorageService` dependency injection
- ✅ **Fix**: Inject đúng cả 2 services trong constructor

```csharp
public UsersController(IUserService userService, IStorageService storageService)
{
    _userService = userService;
    _storageService = storageService;
}
```

**Authorization:**
- ✅ **Bỏ** `[Authorize(Policy = "RequireCustomerRole")]` (policy không tồn tại)
- ✅ **Thay bằng** `[Authorize]` - cho phép tất cả user đã login

**Error Handling:**
- ✅ Cải thiện error messages rõ ràng hơn
- ✅ Trả về đúng HTTP status codes (400, 403, 404, 500)
- ✅ Sử dụng `ApiResponse<T>` nhất quán

**Helper Methods:**
- ✅ **Thêm** `GetCurrentUserId()` - Lấy userId từ JWT token một cách an toàn
- ✅ **Thêm** `DeleteOldAvatar()` - Xử lý xóa avatar cũ trước khi upload mới

#### API Endpoints:

##### 1. Upload Avatar
```http
POST /api/users/{id}/upload-avatar
Authorization: Bearer {token}
Content-Type: multipart/form-data

file: [image file]
```

**Validations:**
- ✅ File không được rỗng
- ✅ Chỉ chấp nhận: JPEG, JPG, PNG, WEBP
- ✅ Kích thước tối đa: 2MB
- ✅ User chỉ upload avatar cho chính mình

**Logic:**
- ✅ Avatar được lưu trong folder `avatars/` trên MinIO
- ✅ Tên file: `avatar-{userId}-{guid}.{extension}`
- ✅ Tự động xóa avatar cũ nếu có
- ✅ Lưu relative path vào database
- ✅ Trả về full URL cho client

**Response Success (200):**
```json
{
  "success": true,
  "message": "Upload avatar thành công",
  "data": {
    "id": 1,
    "firstName": "Nguyen",
    "lastName": "Van A",
    "fullName": "Nguyen Van A",
    "email": "user@example.com",
    "phone": "0123456789",
    "role": "Customer",
    "avatarUrl": "/football-images/avatars/avatar-1-guid.jpg",
    "createdAt": "2025-11-19T10:00:00",
    "updatedAt": "2025-11-19T14:30:00"
  },
  "statusCode": 200
}
```

##### 2. Delete Avatar
```http
DELETE /api/users/{id}/avatar
Authorization: Bearer {token}
```

**Logic:**
- ✅ Xóa file từ MinIO
- ✅ Set `AvatarUrl = null` trong database
- ✅ Chỉ user chính mới được xóa

##### 3. Update Profile
```http
PATCH /api/users/{id}/profile
Authorization: Bearer {token}
Content-Type: application/json

{
  "firstName": "Nguyen",
  "lastName": "Van B",
  "phone": "0987654321"
}
```

**Validations:**
- ✅ FirstName: Tối đa 100 ký tự
- ✅ LastName: Tối đa 100 ký tự
- ✅ Phone: Định dạng số điện thoại, tối đa 15 ký tự
- ✅ Kiểm tra số điện thoại không bị trùng với user khác

**Logic:**
- ✅ Chỉ cập nhật các field được gửi lên (không null/empty)
- ✅ Trim khoảng trắng thừa
- ✅ User chỉ cập nhật thông tin của chính mình

##### 4. Change Password
```http
POST /api/users/{id}/change-password
Authorization: Bearer {token}
Content-Type: application/json

{
  "currentPassword": "oldpass123",
  "newPassword": "newpass456"
}
```

**Validations:**
- ✅ CurrentPassword: Required
- ✅ NewPassword: Required, tối thiểu 6 ký tự
- ✅ Kiểm tra mật khẩu hiện tại đúng

**Logic:**
- ✅ Verify mật khẩu cũ bằng `IAuthService.VerifyPassword()`
- ✅ Hash mật khẩu mới bằng `IAuthService.HashPassword()`
- ✅ User chỉ đổi mật khẩu của chính mình

### 2. UserService (`Services/Implements/UserService.cs`)

#### ✅ Sửa lỗi & Cải tiến:

**Constructor:**
```csharp
// TRƯỚC (Lỗi)
public UserService(IUserRepository userRepository, IMapper mapper)
{
    _userRepository = userRepository;
    _mapper = mapper;
    // _authService không được inject!
}

// SAU (Đúng)
public UserService(IUserRepository userRepository, IMapper mapper, IAuthService authService)
{
    _userRepository = userRepository;
    _mapper = mapper;
    _authService = authService; // ✅ Inject đúng
}
```

**UpdateUserProfileAsync:**
```csharp
// Cải tiến logic:
- ✅ Kiểm tra user tồn tại và chưa bị xóa
- ✅ Chỉ validate phone nếu thay đổi (tránh validate không cần thiết)
- ✅ Chỉ cập nhật field không null/empty
- ✅ Trim khoảng trắng
- ✅ Sử dụng UTC+7 cho UpdatedAt
- ✅ Error messages tiếng Việt rõ ràng
```

**ChangePasswordAsync:**
```csharp
// Cải tiến:
- ✅ Kiểm tra user tồn tại và chưa bị xóa
- ✅ Sử dụng _authService.VerifyPassword() và HashPassword()
- ✅ Return false nếu mật khẩu cũ sai (không throw exception)
- ✅ Sử dụng UTC+7 cho UpdatedAt
```

### 3. DTOs

#### ChangePasswordDto (`Dtos/User/ChangePasswordDto.cs`)
```csharp
// TRƯỚC: Không có validation
public class ChangePasswordDto
{
    public string CurrentPassword { get; set; }
    public string NewPassword { get; set; }
}

// SAU: Thêm validation attributes
public class ChangePasswordDto
{
    [Required(ErrorMessage = "Mật khẩu hiện tại là bắt buộc")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
    [MinLength(6, ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự")]
    public string NewPassword { get; set; } = string.Empty;
}
```

#### UpdateUserProfileDto (`Dtos/User/UpdateUserProfileDto.cs`)
```csharp
// SAU: Thêm validation
public class UpdateUserProfileDto
{
    [StringLength(100, ErrorMessage = "Tên không được quá 100 ký tự")]
    public string? FirstName { get; set; }

    [StringLength(100, ErrorMessage = "Họ không được quá 100 ký tự")]
    public string? LastName { get; set; }

    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    [StringLength(15, ErrorMessage = "Số điện thoại không được quá 15 ký tự")]
    public string? Phone { get; set; }
}
```

#### UserResponseDto (`Dtos/User/UserResponseDto.cs`)
```csharp
// TRƯỚC: Có trường "Name" không rõ ràng
public class UserResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } // ❌ Không rõ FirstName hay LastName?
    public string Email { get; set; }
    // ...
}

// SAU: Tách rõ FirstName, LastName, thêm FullName
public class UserResponseDto
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string FullName => $"{FirstName} {LastName}".Trim(); // ✅ Computed property
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string Role { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

### 4. AutoMapper Configuration

#### MappingProfile (`Mappings/MappingProfile.cs`)
```csharp
// Thêm mapping cho UserResponseDto
CreateMap<User, UserResponseDto>()
    .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));
```

## MinIO Storage - Avatar Organization

### Folder Structure ✅
```
football-images/           # Bucket name
├── avatars/              # ✅ Avatar folder
│   ├── avatar-1-{guid}.jpg
│   ├── avatar-2-{guid}.png
│   └── avatar-3-{guid}.webp
├── complex-images/       # Complex images (khác)
└── other-folders/
```

### File Naming Convention
- **Pattern**: `avatar-{userId}-{guid}.{extension}`
- **Example**: `avatar-123-a1b2c3d4-e5f6-7890-abcd-ef1234567890.jpg`
- **Benefits**:
  - ✅ Dễ identify user nào
  - ✅ Unique bằng GUID
  - ✅ Tránh conflict tên file

### Storage Path Handling

**Lưu trong Database:**
```
Relative path: /{bucket}/avatars/avatar-1-guid.jpg
```

**Trả về cho Client:**
```
Full URL: http://localhost:9000/football-images/avatars/avatar-1-guid.jpg
```

**Delete Old Avatar Logic:**
```csharp
// Hỗ trợ cả full URL và relative path
private async Task DeleteOldAvatar(string avatarUrl)
{
    // Xử lý full URL (http://...)
    if (avatarUrl.StartsWith("http"))
    {
        var uri = new Uri(avatarUrl);
        objectName = ExtractObjectName(uri.AbsolutePath);
    }
    // Xử lý relative path (/{bucket}/...)
    else
    {
        objectName = ExtractFromRelativePath(avatarUrl);
    }
    
    await _storageService.DeleteAsync(objectName);
}
```

## Security & Authorization

### Authentication Flow
1. User login → Nhận JWT token
2. JWT token chứa `ClaimTypes.NameIdentifier` = userId
3. Mọi request gửi `Authorization: Bearer {token}`
4. Controller extract userId từ token
5. Validate: userId trong token == userId trong URL

### Permission Checks
```csharp
// ✅ Kiểm tra user chỉ thao tác với data của mình
var userId = GetCurrentUserId(); // Từ JWT token
if (userId != id) // id trong URL
{
    return StatusCode(403, "Bạn chỉ có thể... của chính mình");
}
```

### Why không dùng Policy?
- ❌ `[Authorize(Policy = "RequireCustomerRole")]` không được define trong `Program.cs`
- ✅ `[Authorize]` đơn giản, đủ cho các API profile
- ✅ Permission check thủ công trong controller linh hoạt hơn

## Testing Guide

### 1. Upload Avatar
```bash
# Đăng nhập để lấy token
POST /api/auth/login
{
  "email": "user@example.com",
  "password": "password123"
}

# Upload avatar
curl -X POST http://localhost:5000/api/users/1/upload-avatar \
  -H "Authorization: Bearer {token}" \
  -F "file=@avatar.jpg"

# Expected: 200 OK với UserResponseDto
```

### 2. Update Profile
```bash
curl -X PATCH http://localhost:5000/api/users/1/profile \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "Nguyen",
    "lastName": "Van B",
    "phone": "0987654321"
  }'
```

### 3. Change Password
```bash
curl -X POST http://localhost:5000/api/users/1/change-password \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "currentPassword": "oldpass123",
    "newPassword": "newpass456"
  }'

# Test case: Mật khẩu cũ sai
# Expected: 400 Bad Request - "Mật khẩu hiện tại không đúng"
```

## Error Responses

### 400 Bad Request
```json
{
  "success": false,
  "message": "Định dạng file không hợp lệ. Chỉ chấp nhận ảnh (JPEG, PNG, WEBP)",
  "data": null,
  "statusCode": 400
}
```

### 403 Forbidden
```json
{
  "success": false,
  "message": "Bạn chỉ có thể upload avatar cho chính mình",
  "data": null,
  "statusCode": 403
}
```

### 404 Not Found
```json
{
  "success": false,
  "message": "Không tìm thấy người dùng",
  "data": null,
  "statusCode": 404
}
```

### 500 Internal Server Error
```json
{
  "success": false,
  "message": "Lỗi khi upload avatar: {error detail}",
  "data": null,
  "statusCode": 500
}
```

## Best Practices Implemented

### 1. Dependency Injection ✅
- Inject đúng tất cả dependencies vào constructor
- Không khởi tạo services bằng `new`

### 2. Error Handling ✅
- Try-catch blocks cho tất cả operations
- Error messages rõ ràng, tiếng Việt
- Trả về đúng HTTP status codes

### 3. Validation ✅
- Data Annotations trên DTOs
- Business logic validation trong Service
- Permission validation trong Controller

### 4. Security ✅
- JWT token authentication
- User chỉ thao tác với data của mình
- Hash password trước khi lưu

### 5. Code Organization ✅
- Helper methods để tránh code duplicate
- Separation of concerns (Controller → Service → Repository)
- Clean code principles

### 6. Performance ✅
- Chỉ load user một lần
- Xóa avatar cũ asynchronously
- Validate phone chỉ khi thay đổi

## Tổng kết

### ✅ Đã sửa:
1. Constructor dependency injection
2. Authorization policy (bỏ policy không tồn tại)
3. Error handling & status codes
4. DTOs validation
5. UserResponseDto structure
6. Service logic & error messages
7. Helper methods để tránh duplicate code

### ✅ Đã kiểm tra:
1. Avatar được lưu đúng trong folder `avatars/`
2. File naming convention hợp lý
3. Delete old avatar hoạt động đúng
4. Tất cả APIs có authorization check
5. Validation đầy đủ cho inputs
6. Mapping AutoMapper hoàn chỉnh

### 🎯 Kết quả:
Module User Profile APIs đã hoàn thiện với:
- Code đúng kiến trúc
- Logic nghiệp vụ chặt chẽ
- Security đảm bảo
- Error handling tốt
- Storage organization hợp lý
