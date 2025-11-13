# Booking Feature Implementation

## Tổng quan
Đã implement đầy đủ tính năng Booking theo đúng nghiệp vụ yêu cầu trong prompt.md

## ⚠️ Giải pháp Filtered Unique Index

### Vấn đề
Ban đầu database có UNIQUE constraint trên `(field_id, booking_date, time_slot_id)` cho **tất cả** các booking. Điều này gây ra vấn đề:
- Khi booking bị Rejected/Cancelled/Expired, dòng dữ liệu vẫn còn trong DB
- UNIQUE constraint chặn không cho người khác đặt cùng slot đó
- Slot không được "trả lại" cho hệ thống

### Giải pháp: Filtered Unique Index
Sử dụng **Filtered Unique Index** - chỉ áp dụng unique constraint cho những trạng thái **thực sự chiếm slot**:

```sql
CREATE UNIQUE INDEX IX_Booking_UniqueActiveSlot 
ON BOOKING (field_id, booking_date, time_slot_id)
WHERE booking_status IN (0, 1, 2); -- Pending, WaitingForApproval, Confirmed
```

### Phân loại trạng thái

#### Trạng thái CHIẾM SLOT (áp dụng unique):
- **Pending (0)**: Khách vừa tạo, đang giữ chỗ trong 5 phút
- **WaitingForApproval (1)**: Đã upload bill, chờ chủ sân duyệt
- **Confirmed (2)**: Chủ sân đã duyệt, sân đã được đặt chắc chắn

#### Trạng thái KHÔNG CHIẾM SLOT (không áp dụng unique):
- **Rejected (3)**: Chủ sân từ chối bill → slot được trả lại
- **Cancelled (4)**: Booking bị hủy → slot được trả lại
- **Completed (5)**: Đã hoàn thành → slot được trả lại
- **Expired (6)**: Hết thời gian giữ chỗ → slot được trả lại
- **NoShow (7)**: Khách không đến → slot được trả lại

### Lợi ích
1. ✅ **Slot được tự động release**: Khi booking chuyển sang trạng thái không chiếm slot, người khác có thể đặt ngay
2. ✅ **Giữ lịch sử**: Vẫn lưu trữ toàn bộ lịch sử booking trong DB
3. ✅ **Performance tốt**: Index được tối ưu, chỉ check trên subset nhỏ của data
4. ✅ **Tránh race condition**: Database đảm bảo không có 2 booking active cùng lúc cho 1 slot

### Ví dụ thực tế

**Scenario 1: Booking bị reject**
```
1. User A tạo booking → Status = Pending (chiếm slot)
2. User A upload bill → Status = WaitingForApproval (vẫn chiếm slot)
3. Owner reject → Status = Rejected (KHÔNG chiếm slot nữa)
4. User B có thể tạo booking mới cho cùng slot ✅
```

**Scenario 2: Booking bị expired**
```
1. User A tạo booking → Status = Pending (chiếm slot)
2. Sau 5 phút không upload → Status = Expired (KHÔNG chiếm slot nữa)
3. User B có thể tạo booking mới cho cùng slot ✅
```

**Scenario 3: Booking bị cancel**
```
1. User A tạo booking → Status = Confirmed (chiếm slot)
2. User A cancel → Status = Cancelled (KHÔNG chiếm slot nữa)
3. User B có thể tạo booking mới cho cùng slot ✅
```

## Các file đã tạo/cập nhật

### 1. Entities
- **Booking.cs**: Cập nhật entity với đầy đủ thuộc tính và BookingStatus enum mới
  - 8 trạng thái: Pending, WaitingForApproval, Confirmed, Rejected, Cancelled, Completed, Expired, NoShow
  - Thêm các trường: HoldExpiresAt, PaymentProofUrl, ApprovedBy, ApprovedAt, CancelledBy, CancelledAt

### 2. Database Context
- **ApplicationDbContext.cs**: Cập nhật cấu hình Booking entity với đầy đủ các constraint và relationship

### 3. DTOs (Dtos/Booking/)
- **BookingDto.cs**: DTO trả về chi tiết booking
- **CreateBookingDto.cs**: DTO tạo booking mới
- **UploadPaymentProofDto.cs**: DTO upload ảnh bill thanh toán
- **UpdateBookingStatusDto.cs**: DTO cập nhật trạng thái
- **RejectBookingDto.cs**: DTO từ chối booking

### 4. Repository
- **IBookingRepository.cs**: Interface với các method cần thiết
- **BookingRepository.cs**: Implementation với các method:
  - GetBookedTimeSlotIdsForComplexAsync
  - GetByCustomerAsync
  - GetByOwnerAsync
  - GetDetailAsync
  - IsTimeSlotBookedAsync
  - GetExpiredPendingBookingsAsync

### 5. Service
- **IBookingService.cs**: Interface service
- **BookingService.cs**: Implementation với đầy đủ logic nghiệp vụ:
  - CreateBookingAsync: Tạo booking, tính deposit (30%), set thời gian hold (5 phút)
  - UploadPaymentProofAsync: Upload bill lên MinIO, chuyển sang WaitingForApproval
  - ApproveBookingAsync: Chủ sân duyệt booking
  - RejectBookingAsync: Chủ sân từ chối
  - CancelBookingAsync: Khách/chủ sân hủy booking
  - MarkCompletedAsync: Đánh dấu hoàn thành
  - MarkNoShowAsync: Đánh dấu khách không đến
  - GetBookingsForCustomerAsync: Lấy danh sách booking của khách
  - GetBookingsForOwnerAsync: Lấy danh sách booking của chủ sân
  - ProcessExpiredBookingsAsync: Xử lý booking hết hạn

### 6. Controller
- **BookingsController.cs**: API endpoints:
  - POST /api/bookings - Tạo booking (Customer only)
  - POST /api/bookings/{id}/upload-payment - Upload bill (Customer only)
  - POST /api/bookings/{id}/approve - Duyệt booking (Owner only)
  - POST /api/bookings/{id}/reject - Từ chối booking (Owner only)
  - POST /api/bookings/{id}/cancel - Hủy booking
  - POST /api/bookings/{id}/complete - Đánh dấu hoàn thành (Owner only)
  - POST /api/bookings/{id}/no-show - Đánh dấu không đến (Owner only)
  - GET /api/bookings/my-bookings - Xem booking của khách (Customer only)
  - GET /api/bookings/owner-bookings - Xem booking của chủ sân (Owner only)
  - GET /api/bookings/{id} - Xem chi tiết booking

### 7. Background Job
- **BookingExpirationBackgroundService.cs**: Background service tự động chạy mỗi 1 phút để xử lý các booking Pending đã hết hạn hold (5 phút)

### 8. Program.cs
- Đăng ký BookingService vào DI container
- Đăng ký BookingExpirationBackgroundService

### 9. Field Repository Enhancement
- Thêm method GetFieldWithComplexAsync để lấy Field kèm Complex thông tin

### 10. Test File
- **booking-tests.http**: File test HTTP với đầy đủ các test case

## Flow nghiệp vụ

### Bước 1: Khách tạo booking
- Khách chọn sân, khung giờ, ngày đá
- Hệ thống tính tiền cọc (30% tổng tiền)
- Tạo booking với status = Pending
- Set thời gian hold = 5 phút từ thời điểm tạo
- Trả về booking ID cho khách

### Bước 2: Khách upload bill thanh toán cọc
- Trong vòng 5 phút, khách upload ảnh bill
- Hệ thống lưu ảnh lên MinIO
- Chuyển status sang WaitingForApproval
- Nếu quá 5 phút không upload → status = Expired (tự động qua Background Service)

### Bước 3: Chủ sân duyệt
- Chủ sân xem bill và duyệt → status = Confirmed (giữ sân thành công)
- Hoặc từ chối → status = Rejected (cần upload lại)

### Bước 4: Hoàn tất
- Sau ngày đá, chủ sân đánh dấu:
  - Completed: Khách đã đến và thanh toán đầy đủ
  - NoShow: Khách không đến

### Hủy booking
- Khách hoặc chủ sân có thể hủy ở các trạng thái: Pending, WaitingForApproval, Confirmed
- Status chuyển sang Cancelled

## Validation & Business Rules

1. **Không đặt sân quá khứ**: BookingDate phải >= ngày hiện tại
2. **Không trùng khung giờ**: Kiểm tra field + date + timeslot đã được đặt chưa
3. **Upload bill trong 5 phút**: Sau 5 phút không upload → Expired
4. **Chỉ upload bill khi Pending**: Không cho upload khi đã WaitingForApproval
5. **Chỉ duyệt khi WaitingForApproval**: Không duyệt các trạng thái khác
6. **Chỉ đánh dấu Completed/NoShow sau ngày đá**: Không cho đánh dấu trước ngày đá
7. **Authorization**: Kiểm tra quyền Customer/Owner cho từng action

## Migration

Đã tạo migration UpdateBookingEntity để cập nhật database schema. Chạy:
```bash
dotnet ef database update
```

## Các tính năng đặc biệt

1. **Auto-expire booking**: Background service tự động chạy mỗi 1 phút để expire các booking Pending quá hạn
2. **Upload ảnh lên MinIO**: Tích hợp MinIO để lưu trữ ảnh bill thanh toán
3. **Deposit calculation**: Tự động tính 30% tiền cọc từ giá khung giờ
4. **Hold time**: Giữ chỗ trong 5 phút để khách upload bill
5. **Full audit trail**: Lưu thông tin người approve, người cancel, thời gian approve/cancel

## Testing

Sử dụng file `booking-tests.http` để test các API endpoints. Cần:
1. Đăng nhập để lấy token Customer và Owner
2. Thay thế các biến trong file
3. Test tuần tự theo flow nghiệp vụ

## Notes

- Deposit rate mặc định: 30% (có thể cấu hình qua constant trong BookingService)
- Hold time mặc định: 5 phút (có thể cấu hình qua constant trong BookingService)
- Background service check interval: 1 phút (có thể cấu hình trong BookingExpirationBackgroundService)
