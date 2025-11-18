# Review Module - Tóm tắt thay đổi

## Các file đã sửa/tạo mới

### 1. Entities
- ✅ `Entities/Review.cs` - Thêm `FieldId` và navigation property `Field`
- ✅ `Entities/Field.cs` - Thêm collection `Reviews`

### 2. Database Context
- ✅ `DbContexts/ApplicationDbContext.cs` - Cập nhật configuration cho Review:
  - Thêm mapping `field_id`
  - Thêm relationship với Field
  - Thêm unique index cho `booking_id`
  - Tăng max length của comment lên 1000
  - Thêm index cho `field_id`

### 3. DTOs
- ✅ `Dtos/Review/CreateReviewDto.cs` - Bỏ `FieldId` và `CustomerId` (tự động lấy)
- ✅ `Dtos/Review/ReviewDto.cs` - Thêm `CustomerName`, `FieldName`, `ComplexName`
- ✅ `Dtos/Review/UpdateReviewDto.cs` - Bỏ `IsVisible`, đổi sang `byte` cho Rating

### 4. Repository Layer
- ✅ `Repositories/Interfaces/IReviewRepository.cs` - Thêm methods mới
- ✅ `Repositories/Implements/ReviewRepository.cs` - Implement đầy đủ với include relationships
- ✅ `Repositories/Implements/BookingRepository.cs` - Override `GetByIdAsync` để include Field

### 5. Service Layer
- ✅ `Services/Interfaces/IReviewService.cs` - Cập nhật interface với methods mới
- ✅ `Services/Implements/ReviewService.cs` - Implement đầy đủ business logic:
  - Kiểm tra booking completed
  - Kiểm tra quyền sở hữu
  - Kiểm tra duplicate review
  - Admin operations (delete, toggle visibility)

### 6. Controller
- ✅ `Controllers/ReviewController.cs` - Hoàn toàn mới với:
  - Public endpoints (lấy review, điểm TB)
  - User endpoints (CRUD review của mình)
  - Admin endpoints (quản lý tất cả review)
  - Authentication với JWT token

### 7. Mapping
- ✅ `Mappings/MappingProfile.cs` - Thêm mapping cho Review entities và DTOs

### 8. Documentation
- ✅ `docs/REVIEW_MODULE.md` - Tài liệu đầy đủ về module Review

## Tính năng chính

### User (Customer)
1. ✅ Tạo review sau khi hoàn thành booking (`BookingStatus.Completed`)
2. ✅ Xem tất cả review của mình
3. ✅ Sửa review của mình
4. ✅ Xóa review của mình
5. ✅ Một booking chỉ review được một lần

### Admin
1. ✅ Xem tất cả review (bao gồm review bị ẩn)
2. ✅ Xóa bất kỳ review nào
3. ✅ Ẩn/Hiện review (`IsVisible`)

### Public
1. ✅ Xem review của sân (`/api/reviews/field/{fieldId}`)
2. ✅ Xem review của complex (`/api/reviews/complex/{complexId}`)
3. ✅ Xem điểm trung bình của sân/complex
4. ✅ Chỉ hiển thị review `IsVisible = true`

## Business Rules

1. **Tạo review:**
   - Booking phải thuộc về user (từ JWT token)
   - Booking phải có status = `Completed`
   - Booking chưa được review trước đó
   - `FieldId` và `ComplexId` tự động lấy từ booking

2. **Update/Delete review:**
   - User chỉ được sửa/xóa review của mình
   - Admin có thể xóa/ẩn bất kỳ review nào
   - Soft delete (không xóa vĩnh viễn)

3. **Hiển thị review:**
   - Public endpoints chỉ show review có `IsVisible = true` và `IsDeleted = false`
   - Admin có thể xem tất cả review

## Migration cần chạy

```bash
dotnet ef migrations add UpdateReviewModule
dotnet ef database update
```

## API Endpoints

### Public (không cần login)
- `GET /api/reviews/field/{fieldId}` - Review của sân
- `GET /api/reviews/complex/{complexId}` - Review của complex
- `GET /api/reviews/{id}` - Chi tiết review
- `GET /api/reviews/field/{fieldId}/average-rating` - Điểm TB sân
- `GET /api/reviews/complex/{complexId}/average-rating` - Điểm TB complex

### User (cần login)
- `GET /api/reviews/my-reviews` - Review của mình
- `POST /api/reviews` - Tạo review
- `PUT /api/reviews/{id}` - Sửa review
- `DELETE /api/reviews/{id}` - Xóa review

### Admin
- `GET /api/reviews` - Tất cả review
- `DELETE /api/reviews/admin/{id}` - Xóa review
- `PATCH /api/reviews/admin/{id}/visibility` - Ẩn/Hiện review

## Test nhanh

```bash
# 1. Tạo review (sau khi có booking completed)
POST /api/reviews
Authorization: Bearer {token}
{
  "bookingId": 1,
  "rating": 5,
  "comment": "Sân đẹp!"
}

# 2. Xem review của sân
GET /api/reviews/field/1

# 3. Admin ẩn review
PATCH /api/reviews/admin/1/visibility
Authorization: Bearer {admin_token}
Content-Type: application/json

false
```

## Hoàn thành! ✅

Module Review đã sẵn sàng sử dụng với đầy đủ tính năng theo yêu cầu.
