# Booking Feature Implementation

## Tổng quan
Đã implement đầy đủ tính năng Booking theo đúng nghiệp vụ yêu cầu trong prompt.md

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
