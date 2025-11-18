# Review Module Documentation

## Tổng quan

Module Review cho phép người dùng đánh giá sân bóng sau khi hoàn thành booking. Module này bao gồm các tính năng quản lý review cho cả người dùng thường và admin.

## Các thay đổi chính

### 1. Entity Review (`Entities/Review.cs`)
- **Thêm mới**: `FieldId` và navigation property `Field`
- **Mục đích**: Liên kết review với sân cụ thể, cho phép lấy thông tin chi tiết về sân được đánh giá

```csharp
public class Review
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public int CustomerId { get; set; }
    public int FieldId { get; set; }        // MỚI
    public int ComplexId { get; set; }
    public byte Rating { get; set; }
    public string? Comment { get; set; }
    public bool IsVisible { get; set; }
    public bool IsDeleted { get; set; }
    
    // Navigation properties
    public Booking Booking { get; set; }
    public User Customer { get; set; }
    public Field Field { get; set; }        // MỚI
    public Complex Complex { get; set; }
}
```

### 2. Database Context (`DbContexts/ApplicationDbContext.cs`)
**Cập nhật configuration cho Review:**
- Thêm mapping cho `field_id` column
- Thêm relationship với `Field` entity
- Thêm unique index cho `booking_id` (một booking chỉ có một review)
- Tăng max length của `comment` từ 255 lên 1000 ký tự
- Thêm index cho `field_id` để tối ưu query

```csharp
entity.Property(e => e.FieldId).HasColumnName("field_id").IsRequired();
entity.HasIndex(e => e.BookingId).IsUnique().HasDatabaseName("IX_Review_BookingId");
entity.HasIndex(e => e.FieldId).HasDatabaseName("IX_Review_FieldId");
entity.Property(e => e.Comment).HasColumnName("comment").HasMaxLength(1000);

entity.HasOne(e => e.Field)
    .WithMany(e => e.Reviews)
    .HasForeignKey(e => e.FieldId)
    .OnDelete(DeleteBehavior.NoAction);
```

### 3. DTOs (`Dtos/Review/`)

#### CreateReviewDto
**Thay đổi:**
- **Bỏ**: `FieldId`, `CustomerId` (tự động lấy từ Booking và JWT token)
- **Giữ lại**: `BookingId`, `Rating`, `Comment`
- **Kiểu dữ liệu**: Đổi `Rating` từ `int` sang `byte` (phù hợp với entity)

```csharp
public class CreateReviewDto
{
    public int BookingId { get; set; }
    public byte Rating { get; set; }        // 1-5
    public string? Comment { get; set; }    // Max 1000 chars
}
```

#### ReviewDto
**Thêm mới:**
- `CustomerName`: Tên đầy đủ của người đánh giá
- `FieldName`: Tên sân
- `ComplexName`: Tên cụm sân

```csharp
public class ReviewDto
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; }    // MỚI
    public int FieldId { get; set; }
    public string? FieldName { get; set; }      // MỚI
    public int ComplexId { get; set; }
    public string? ComplexName { get; set; }    // MỚI
    public byte Rating { get; set; }
    public string? Comment { get; set; }
    public bool IsVisible { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

#### UpdateReviewDto
**Thay đổi:**
- **Bỏ**: `IsVisible` (chỉ admin mới có quyền thay đổi)
- Chỉ cho phép cập nhật `Rating` và `Comment`

### 4. Repository Layer

#### IReviewRepository Interface
**Thêm các phương thức mới:**
```csharp
Task<IEnumerable<Review>> GetByComplexIdAsync(int complexId);
Task<double> GetAverageRatingByComplexIdAsync(int complexId);
Task<Review?> GetByBookingIdAsync(int bookingId);
Task<bool> HasReviewForBookingAsync(int bookingId);
```

#### ReviewRepository Implementation
**Cải tiến:**
- Include đầy đủ navigation properties (`Customer`, `Field`, `Complex`) trong tất cả query
- Override `GetByIdAsync()` và `GetAllAsync()` để luôn include relationships
- Thêm các phương thức tìm kiếm theo ComplexId và BookingId

### 5. Service Layer

#### IReviewService Interface
**Thay đổi chữ ký methods:**
```csharp
// User methods - cần customerId từ JWT
Task<ReviewDto> CreateReviewAsync(int customerId, CreateReviewDto dto);
Task UpdateReviewAsync(int id, int customerId, UpdateReviewDto dto);
Task DeleteReviewAsync(int id, int customerId);

// Admin methods
Task AdminDeleteReviewAsync(int id);
Task AdminToggleVisibilityAsync(int id, bool isVisible);

// New methods
Task<IEnumerable<ReviewDto>> GetReviewsByComplexIdAsync(int complexId);
Task<IEnumerable<ReviewDto>> GetMyReviewsAsync(int customerId);
Task<double> GetAverageRatingByComplexIdAsync(int complexId);
```

#### ReviewService Implementation
**Logic kiểm tra khi tạo review:**
1. ✅ Kiểm tra booking tồn tại
2. ✅ Kiểm tra quyền sở hữu (booking phải thuộc về customer)
3. ✅ Kiểm tra booking đã completed (`BookingStatus.Completed`)
4. ✅ Kiểm tra chưa review trước đó (một booking chỉ được review một lần)
5. ✅ Tự động điền `FieldId` và `ComplexId` từ booking

**Logic kiểm tra khi update/delete:**
- ✅ Kiểm tra review tồn tại và chưa bị xóa
- ✅ Kiểm tra quyền sở hữu (chỉ người tạo mới được sửa/xóa)
- ✅ Admin có thể xóa/ẩn bất kỳ review nào

### 6. Controller Layer (`Controllers/ReviewController.cs`)

#### Endpoints công khai (không cần login):
| Method | Endpoint | Mô tả |
|--------|----------|-------|
| GET | `/api/reviews/field/{fieldId}` | Lấy review của sân (chỉ visible) |
| GET | `/api/reviews/complex/{complexId}` | Lấy review của complex (chỉ visible) |
| GET | `/api/reviews/{id}` | Lấy chi tiết một review |
| GET | `/api/reviews/field/{fieldId}/average-rating` | Điểm TB của sân |
| GET | `/api/reviews/complex/{complexId}/average-rating` | Điểm TB của complex |

#### Endpoints cho User (cần login):
| Method | Endpoint | Mô tả |
|--------|----------|-------|
| GET | `/api/reviews/my-reviews` | Xem tất cả review của mình |
| POST | `/api/reviews` | Tạo review mới |
| PUT | `/api/reviews/{id}` | Sửa review của mình |
| DELETE | `/api/reviews/{id}` | Xóa review của mình |

#### Endpoints cho Admin:
| Method | Endpoint | Mô tả |
|--------|----------|-------|
| GET | `/api/reviews` | Xem tất cả review (bao gồm ẩn) |
| DELETE | `/api/reviews/admin/{id}` | Xóa bất kỳ review nào |
| PATCH | `/api/reviews/admin/{id}/visibility` | Ẩn/Hiện review |

**Authentication:**
- Sử dụng JWT token để lấy `userId` từ `ClaimTypes.NameIdentifier`
- Validate user ID trước khi thực hiện các thao tác

### 7. Mapping Configuration (`Mappings/MappingProfile.cs`)
**Thêm mapping cho Review:**
```csharp
// Entity to DTO - tự động map các trường từ navigation properties
CreateMap<Review, ReviewDto>()
    .ForMember(dest => dest.CustomerName, 
        opt => opt.MapFrom(src => src.Customer.FirstName + " " + src.Customer.LastName))
    .ForMember(dest => dest.FieldName, 
        opt => opt.MapFrom(src => src.Field.Name))
    .ForMember(dest => dest.ComplexName, 
        opt => opt.MapFrom(src => src.Complex.Name));

// DTO to Entity
CreateMap<CreateReviewDto, Review>();

// Update mapping - ignore các field không được phép thay đổi
CreateMap<UpdateReviewDto, Review>()
    .ForMember(dest => dest.Id, opt => opt.Ignore())
    .ForMember(dest => dest.BookingId, opt => opt.Ignore())
    .ForMember(dest => dest.CustomerId, opt => opt.Ignore())
    .ForMember(dest => dest.FieldId, opt => opt.Ignore())
    .ForMember(dest => dest.ComplexId, opt => opt.Ignore())
    .ForMember(dest => dest.IsVisible, opt => opt.Ignore())
    .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
    .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
    .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());
```

## Quy tắc nghiệp vụ (Business Rules)

### 1. Tạo Review
✅ **Điều kiện:**
- User phải login
- Booking phải tồn tại và thuộc về user
- Booking phải có status = `Completed`
- Booking chưa được review trước đó

❌ **Lỗi thường gặp:**
- `"Không tìm thấy booking"` - BookingId không tồn tại
- `"Bạn không có quyền đánh giá booking này"` - Booking không thuộc về user
- `"Chỉ có thể đánh giá sau khi hoàn thành trận đấu"` - Booking chưa completed
- `"Bạn đã đánh giá booking này rồi"` - Đã tồn tại review cho booking

### 2. Update Review
✅ **Điều kiện:**
- User phải login
- Review phải tồn tại và chưa bị xóa
- Review phải thuộc về user

❌ **Lỗi thường gặp:**
- `"Không tìm thấy đánh giá"` - Review không tồn tại
- `"Đánh giá đã bị xóa"` - Review đã soft deleted
- `"Bạn không có quyền chỉnh sửa đánh giá này"` - Review không thuộc về user

### 3. Delete Review (User)
✅ **Điều kiện:** Giống Update Review
- Thực hiện soft delete (set `IsDeleted = true`, `DeletedAt = now`)

### 4. Admin Operations
✅ **Quyền admin:**
- Xem tất cả review (kể cả ẩn)
- Xóa bất kỳ review nào
- Ẩn/Hiện review (thay đổi `IsVisible`)

**Lưu ý:** Admin delete cũng là soft delete, không xóa vĩnh viễn khỏi database

## Workflow điển hình

### User tạo review:
```
1. User hoàn thành booking → BookingStatus = Completed
2. User gọi POST /api/reviews với BookingId
3. Backend validate:
   - Lấy userId từ JWT token
   - Kiểm tra booking thuộc về user
   - Kiểm tra booking status = Completed
   - Kiểm tra chưa review
4. Tự động lấy FieldId, ComplexId từ booking
5. Tạo review mới
6. Trả về ReviewDto với đầy đủ thông tin
```

### Admin quản lý review:
```
1. Admin xem tất cả review: GET /api/reviews
2. Nếu review không phù hợp:
   - Ẩn review: PATCH /api/reviews/admin/{id}/visibility (body: false)
   - Hoặc xóa: DELETE /api/reviews/admin/{id}
3. Review bị ẩn vẫn tồn tại nhưng không hiển thị cho public
```

## Tối ưu hiệu năng

### 1. Database Indexes
- `IX_Review_BookingId` (Unique): Tìm review theo booking, đảm bảo unique
- `IX_Review_FieldId`: Tối ưu query review theo sân
- `IX_Review_ComplexId`: Tối ưu query review theo complex

### 2. Query Optimization
- Luôn include navigation properties khi cần thiết
- Filter `IsDeleted = false` ở repository layer
- Filter `IsVisible = true` cho public endpoints
- Sử dụng `OrderByDescending(CreatedAt)` để hiển thị review mới nhất trước

### 3. Eager Loading
Repository luôn include relationships để tránh N+1 query problem:
```csharp
.Include(r => r.Customer)
.Include(r => r.Field)
.Include(r => r.Complex)
```

## API Response Format

### Success Response:
```json
{
  "success": true,
  "message": "Tạo đánh giá thành công",
  "data": {
    "id": 1,
    "bookingId": 10,
    "customerId": 5,
    "customerName": "Nguyễn Văn A",
    "fieldId": 3,
    "fieldName": "Sân 5",
    "complexId": 1,
    "complexName": "Sân bóng ABC",
    "rating": 5,
    "comment": "Sân đẹp, chất lượng tốt!",
    "isVisible": true,
    "createdAt": "2025-11-18T10:30:00",
    "updatedAt": "2025-11-18T10:30:00"
  },
  "statusCode": 201
}
```

### Error Response:
```json
{
  "success": false,
  "message": "Chỉ có thể đánh giá sau khi hoàn thành trận đấu",
  "data": null,
  "statusCode": 400
}
```

## Migration cần thiết

Để sử dụng module Review đã cập nhật, cần chạy migration mới:

```bash
dotnet ef migrations add UpdateReviewModule
dotnet ef database update
```

**Các thay đổi database:**
1. Thêm column `field_id` vào bảng `REVIEW`
2. Thêm foreign key constraint từ `REVIEW` đến `FIELD`
3. Thêm unique index cho `booking_id`
4. Thêm index cho `field_id`
5. Tăng max length của `comment` lên 1000

## Testing

### Test Cases:

#### 1. Tạo review thành công
```http
POST /api/reviews
Authorization: Bearer {token}
Content-Type: application/json

{
  "bookingId": 10,
  "rating": 5,
  "comment": "Sân đẹp!"
}

Expected: 201 Created, review được tạo
```

#### 2. Không thể review booking chưa completed
```http
POST /api/reviews
Authorization: Bearer {token}

{
  "bookingId": 11,  // Booking có status = Pending
  "rating": 4
}

Expected: 400 Bad Request
Message: "Chỉ có thể đánh giá sau khi hoàn thành trận đấu"
```

#### 3. Không thể review lại booking đã review
```http
POST /api/reviews
Authorization: Bearer {token}

{
  "bookingId": 10,  // Đã review rồi
  "rating": 3
}

Expected: 400 Bad Request
Message: "Bạn đã đánh giá booking này rồi"
```

#### 4. User xóa review của mình
```http
DELETE /api/reviews/1
Authorization: Bearer {user_token}

Expected: 200 OK (nếu review thuộc về user)
Expected: 403 Forbidden (nếu review không thuộc về user)
```

#### 5. Admin ẩn review
```http
PATCH /api/reviews/admin/1/visibility
Authorization: Bearer {admin_token}
Content-Type: application/json

false

Expected: 200 OK
Message: "Ẩn đánh giá thành công"
```

## Tính năng nâng cao (có thể mở rộng)

1. **Review statistics**: Thống kê số lượng review theo rating (1-5 sao)
2. **Review images**: Cho phép upload hình ảnh kèm review
3. **Owner response**: Chủ sân có thể phản hồi review
4. **Review moderation**: Hệ thống tự động phát hiện review spam/toxic
5. **Rating breakdown**: Chia rating thành nhiều tiêu chí (sân, dịch vụ, giá cả)
6. **Helpful votes**: User có thể vote review hữu ích

## Lưu ý khi triển khai

1. **Timezone**: Tất cả DateTime đều sử dụng UTC+7 (Vietnam timezone)
2. **Soft Delete**: Review bị xóa vẫn còn trong database để audit
3. **Validation**: Rating phải từ 1-5, comment tối đa 1000 ký tự
4. **Security**: 
   - Luôn validate user ID từ JWT token
   - Không cho phép user sửa/xóa review của người khác
   - Admin endpoints phải có role check
5. **Performance**: Sử dụng index và eager loading để tối ưu query

## Kết luận

Module Review đã được hoàn thiện với:
- ✅ Entity và Database schema đầy đủ
- ✅ Repository pattern với query tối ưu
- ✅ Service layer với business logic chặt chẽ
- ✅ Controller với phân quyền rõ ràng
- ✅ DTOs và Mapping configuration
- ✅ Error handling và validation
- ✅ Support cả user và admin operations

Module sẵn sàng sử dụng sau khi chạy migration!
