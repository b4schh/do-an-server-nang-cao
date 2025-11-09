# Test Cases for Booking Module

## Mục lục
1. [Tạo Booking (CreateBooking)](#1-tạo-booking-createbooking)
2. [Upload Bill Thanh Toán (UploadPaymentProof)](#2-upload-bill-thanh-toán-uploadpaymentproof)
3. [Duyệt Booking (ApproveBooking)](#3-duyệt-booking-approvebooking)
4. [Từ Chối Booking (RejectBooking)](#4-từ-chối-booking-rejectbooking)
5. [Hủy Booking (CancelBooking)](#5-hủy-booking-cancelbooking)
6. [Đánh Dấu Hoàn Thành (MarkCompleted)](#6-đánh-dấu-hoàn-thành-markcompleted)
7. [Đánh Dấu Không Đến (MarkNoShow)](#7-đánh-dấu-không-đến-marknoshow)
8. [Xem Danh Sách Booking](#8-xem-danh-sách-booking)
9. [Auto-Expire Booking](#9-auto-expire-booking)

---

## 1. Tạo Booking (CreateBooking)

### TC-BOOKING-001: Tạo booking thành công
**Mô tả**: Khách hàng tạo booking cho sân còn trống

**Preconditions**:
- User đã đăng nhập với role Customer
- Field ID = 1 tồn tại và active
- TimeSlot ID = 1 tồn tại và active
- Ngày đặt = 2024-12-25 (trong tương lai)
- Khung giờ chưa được đặt

**Steps**:
1. POST /api/bookings
2. Header: Authorization Bearer {customerToken}
3. Body:
```json
{
  "fieldId": 1,
  "timeSlotId": 1,
  "bookingDate": "2024-12-25",
  "note": "Đặt sân giao hữu"
}
```

**Expected Result**:
- Status Code: 201 Created
- Response có bookingId
- BookingStatus = Pending (0)
- HoldExpiresAt = hiện tại + 5 phút
- DepositAmount = 30% TotalAmount
- Message: "Tạo booking thành công. Vui lòng upload bill thanh toán trong 5 phút."

---

### TC-BOOKING-002: Tạo booking thất bại - Đặt sân quá khứ
**Mô tả**: Không cho phép đặt sân cho ngày đã qua

**Preconditions**:
- User đã đăng nhập với role Customer

**Steps**:
1. POST /api/bookings
2. Body: bookingDate = "2024-01-01" (quá khứ)

**Expected Result**:
- Status Code: 400 Bad Request
- Message: "Không thể đặt sân cho ngày trong quá khứ"

---

### TC-BOOKING-003: Tạo booking thất bại - Sân không tồn tại
**Mô tả**: FieldId không tồn tại trong hệ thống

**Preconditions**:
- User đã đăng nhập với role Customer

**Steps**:
1. POST /api/bookings
2. Body: fieldId = 99999 (không tồn tại)

**Expected Result**:
- Status Code: 400 Bad Request
- Message: "Sân không tồn tại"

---

### TC-BOOKING-004: Tạo booking thất bại - TimeSlot không tồn tại
**Mô tả**: TimeSlotId không tồn tại

**Preconditions**:
- User đã đăng nhập với role Customer
- Field ID = 1 tồn tại

**Steps**:
1. POST /api/bookings
2. Body: timeSlotId = 99999 (không tồn tại)

**Expected Result**:
- Status Code: 400 Bad Request
- Message: "Khung giờ không tồn tại"

---

### TC-BOOKING-005: Tạo booking thất bại - TimeSlot không thuộc Field
**Mô tả**: TimeSlot không thuộc Field đang đặt

**Preconditions**:
- User đã đăng nhập với role Customer
- Field ID = 1, TimeSlot ID = 5 (nhưng thuộc Field ID = 2)

**Steps**:
1. POST /api/bookings
2. Body: fieldId = 1, timeSlotId = 5

**Expected Result**:
- Status Code: 400 Bad Request
- Message: "Khung giờ không thuộc sân này"

---

### TC-BOOKING-006: Tạo booking thất bại - Khung giờ đã được đặt
**Mô tả**: Khung giờ đã có booking Confirmed/WaitingForApproval

**Preconditions**:
- User đã đăng nhập với role Customer
- Đã có booking: Field ID = 1, Date = 2024-12-25, TimeSlot ID = 1, Status = Confirmed

**Steps**:
1. POST /api/bookings
2. Body: fieldId = 1, timeSlotId = 1, bookingDate = "2024-12-25"

**Expected Result**:
- Status Code: 400 Bad Request
- Message: "Khung giờ này đã được đặt"

---

### TC-BOOKING-007: Tạo booking thất bại - Không có quyền
**Mô tả**: User không phải Customer

**Preconditions**:
- User đã đăng nhập với role Owner hoặc chưa đăng nhập

**Steps**:
1. POST /api/bookings

**Expected Result**:
- Status Code: 403 Forbidden hoặc 401 Unauthorized

---

## 2. Upload Bill Thanh Toán (UploadPaymentProof)

### TC-UPLOAD-001: Upload bill thành công
**Mô tả**: Khách upload bill trong thời gian hold (5 phút)

**Preconditions**:
- Booking ID = 1, Status = Pending
- HoldExpiresAt chưa quá thời gian hiện tại
- Customer là người tạo booking

**Steps**:
1. POST /api/bookings/1/upload-payment
2. Header: Authorization Bearer {customerToken}
3. FormData:
   - PaymentProofImage: file ảnh .jpg/.png
   - PaymentNote: "Đã chuyển khoản"

**Expected Result**:
- Status Code: 200 OK
- PaymentProofUrl có giá trị (URL MinIO)
- BookingStatus = WaitingForApproval (1)
- Note được cập nhật nếu có PaymentNote
- Message: "Upload bill thanh toán thành công. Đang chờ chủ sân duyệt."

---

### TC-UPLOAD-002: Upload bill thất bại - Quá thời gian hold
**Mô tả**: Upload sau 5 phút kể từ lúc tạo booking

**Preconditions**:
- Booking ID = 1, Status = Pending
- HoldExpiresAt < hiện tại (đã quá 5 phút)

**Steps**:
1. POST /api/bookings/1/upload-payment
2. FormData: file ảnh hợp lệ

**Expected Result**:
- Status Code: 400 Bad Request
- BookingStatus tự động chuyển sang Expired (6)
- Message: "Booking đã hết hạn giữ chỗ"

---

### TC-UPLOAD-003: Upload bill thất bại - Booking không phải Pending
**Mô tả**: Không cho phép upload khi status khác Pending

**Preconditions**:
- Booking ID = 1, Status = WaitingForApproval hoặc Confirmed

**Steps**:
1. POST /api/bookings/1/upload-payment

**Expected Result**:
- Status Code: 400 Bad Request
- Message: "Chỉ có thể upload bill cho booking đang ở trạng thái Pending"

---

### TC-UPLOAD-004: Upload bill thất bại - Không phải chủ booking
**Mô tả**: Customer khác cố upload bill cho booking không phải của mình

**Preconditions**:
- Booking ID = 1, CustomerId = 2
- User đăng nhập với CustomerId = 3

**Steps**:
1. POST /api/bookings/1/upload-payment

**Expected Result**:
- Status Code: 403 Forbidden
- Message: "Bạn không có quyền cập nhật booking này"

---

### TC-UPLOAD-005: Upload bill thất bại - Không có file ảnh
**Mô tả**: Request thiếu PaymentProofImage

**Preconditions**:
- Booking ID = 1, Status = Pending

**Steps**:
1. POST /api/bookings/1/upload-payment
2. FormData: chỉ có PaymentNote, không có file

**Expected Result**:
- Status Code: 400 Bad Request
- Validation Error: "File ảnh là bắt buộc"

---

### TC-UPLOAD-006: Upload bill thất bại - File không hợp lệ
**Mô tả**: Upload file không phải ảnh (.txt, .pdf, ...)

**Preconditions**:
- Booking ID = 1, Status = Pending

**Steps**:
1. POST /api/bookings/1/upload-payment
2. FormData: PaymentProofImage = file .txt

**Expected Result**:
- Status Code: 400 Bad Request
- Message lỗi validation file type

---

## 3. Duyệt Booking (ApproveBooking)

### TC-APPROVE-001: Duyệt booking thành công
**Mô tả**: Chủ sân duyệt bill thanh toán

**Preconditions**:
- Booking ID = 1, Status = WaitingForApproval
- OwnerId = 5
- User đăng nhập với OwnerId = 5

**Steps**:
1. POST /api/bookings/1/approve
2. Header: Authorization Bearer {ownerToken}

**Expected Result**:
- Status Code: 200 OK
- BookingStatus = Confirmed (2)
- ApprovedBy = 5
- ApprovedAt = thời gian hiện tại
- Message: "Duyệt booking thành công"

---

### TC-APPROVE-002: Duyệt thất bại - Không phải chủ sân
**Mô tả**: Owner khác cố duyệt booking không phải của mình

**Preconditions**:
- Booking ID = 1, OwnerId = 5
- User đăng nhập với OwnerId = 6

**Steps**:
1. POST /api/bookings/1/approve

**Expected Result**:
- Status Code: 403 Forbidden
- Message: "Bạn không có quyền duyệt booking này"

---

### TC-APPROVE-003: Duyệt thất bại - Status không phải WaitingForApproval
**Mô tả**: Không cho duyệt booking đang Pending, Confirmed, Rejected, ...

**Preconditions**:
- Booking ID = 1, Status = Pending hoặc Confirmed

**Steps**:
1. POST /api/bookings/1/approve

**Expected Result**:
- Status Code: 400 Bad Request
- Message: "Chỉ có thể duyệt booking đang ở trạng thái WaitingForApproval"

---

### TC-APPROVE-004: Duyệt thất bại - Booking không tồn tại
**Mô tả**: BookingId không tồn tại

**Steps**:
1. POST /api/bookings/99999/approve

**Expected Result**:
- Status Code: 400 Bad Request
- Message: "Không tìm thấy booking"

---

## 4. Từ Chối Booking (RejectBooking)

### TC-REJECT-001: Từ chối booking thành công
**Mô tả**: Chủ sân từ chối bill không rõ ràng

**Preconditions**:
- Booking ID = 1, Status = WaitingForApproval
- OwnerId = 5
- User đăng nhập với OwnerId = 5

**Steps**:
1. POST /api/bookings/1/reject
2. Body:
```json
{
  "reason": "Ảnh bill không rõ ràng, vui lòng chụp lại"
}
```

**Expected Result**:
- Status Code: 200 OK
- BookingStatus = Rejected (3)
- Note = "Từ chối: Ảnh bill không rõ ràng, vui lòng chụp lại"
- Message: "Từ chối booking thành công"

---

### TC-REJECT-002: Từ chối thành công - Không có lý do
**Mô tả**: Lý do từ chối là optional

**Preconditions**:
- Booking ID = 1, Status = WaitingForApproval

**Steps**:
1. POST /api/bookings/1/reject
2. Body: {}

**Expected Result**:
- Status Code: 200 OK
- BookingStatus = Rejected (3)
- Note không thay đổi

---

### TC-REJECT-003: Từ chối thất bại - Không phải chủ sân
**Preconditions**:
- Booking ID = 1, OwnerId = 5
- User đăng nhập với OwnerId = 6

**Steps**:
1. POST /api/bookings/1/reject

**Expected Result**:
- Status Code: 403 Forbidden
- Message: "Bạn không có quyền từ chối booking này"

---

### TC-REJECT-004: Từ chối thất bại - Status không phải WaitingForApproval
**Preconditions**:
- Booking ID = 1, Status = Confirmed

**Steps**:
1. POST /api/bookings/1/reject

**Expected Result**:
- Status Code: 400 Bad Request
- Message: "Chỉ có thể từ chối booking đang ở trạng thái WaitingForApproval"

---

## 5. Hủy Booking (CancelBooking)

### TC-CANCEL-001: Khách hủy booking Pending thành công
**Mô tả**: Khách hủy booking chưa upload bill

**Preconditions**:
- Booking ID = 1, Status = Pending, CustomerId = 2
- User đăng nhập với CustomerId = 2

**Steps**:
1. POST /api/bookings/1/cancel

**Expected Result**:
- Status Code: 200 OK
- BookingStatus = Cancelled (4)
- CancelledBy = 2
- CancelledAt = thời gian hiện tại
- Message: "Hủy booking thành công"

---

### TC-CANCEL-002: Khách hủy booking WaitingForApproval thành công
**Preconditions**:
- Booking ID = 1, Status = WaitingForApproval, CustomerId = 2

**Steps**:
1. POST /api/bookings/1/cancel

**Expected Result**:
- Status Code: 200 OK
- BookingStatus = Cancelled (4)

---

### TC-CANCEL-003: Khách hủy booking Confirmed thành công
**Preconditions**:
- Booking ID = 1, Status = Confirmed, CustomerId = 2

**Steps**:
1. POST /api/bookings/1/cancel

**Expected Result**:
- Status Code: 200 OK
- BookingStatus = Cancelled (4)

---

### TC-CANCEL-004: Chủ sân hủy booking thành công
**Mô tả**: Owner cũng có thể hủy booking

**Preconditions**:
- Booking ID = 1, Status = Confirmed, OwnerId = 5
- User đăng nhập với OwnerId = 5

**Steps**:
1. POST /api/bookings/1/cancel

**Expected Result**:
- Status Code: 200 OK
- BookingStatus = Cancelled (4)
- CancelledBy = 5

---

### TC-CANCEL-005: Hủy thất bại - Không có quyền
**Preconditions**:
- Booking ID = 1, CustomerId = 2, OwnerId = 5
- User đăng nhập với UserId = 7 (không phải customer hay owner)

**Steps**:
1. POST /api/bookings/1/cancel

**Expected Result**:
- Status Code: 403 Forbidden
- Message: "Bạn không có quyền hủy booking này"

---

### TC-CANCEL-006: Hủy thất bại - Status không hợp lệ
**Mô tả**: Không cho hủy booking Completed, NoShow, Expired, Rejected

**Preconditions**:
- Booking ID = 1, Status = Completed

**Steps**:
1. POST /api/bookings/1/cancel

**Expected Result**:
- Status Code: 400 Bad Request
- Message: "Không thể hủy booking ở trạng thái này"

---

## 6. Đánh Dấu Hoàn Thành (MarkCompleted)

### TC-COMPLETE-001: Đánh dấu hoàn thành thành công
**Mô tả**: Sau ngày đá, chủ sân xác nhận khách đã đến và thanh toán

**Preconditions**:
- Booking ID = 1, Status = Confirmed, BookingDate = 2024-11-01
- Ngày hiện tại = 2024-11-02 (sau ngày đá)
- OwnerId = 5

**Steps**:
1. POST /api/bookings/1/complete

**Expected Result**:
- Status Code: 200 OK
- BookingStatus = Completed (5)
- Message: "Đánh dấu hoàn thành thành công"

---

### TC-COMPLETE-002: Đánh dấu hoàn thành thất bại - Chưa đến ngày đá
**Preconditions**:
- Booking ID = 1, Status = Confirmed, BookingDate = 2024-12-25
- Ngày hiện tại = 2024-11-10 (trước ngày đá)

**Steps**:
1. POST /api/bookings/1/complete

**Expected Result**:
- Status Code: 400 Bad Request
- Message: "Chỉ có thể đánh dấu hoàn thành sau ngày đá"

---

### TC-COMPLETE-003: Đánh dấu hoàn thành thất bại - Status không phải Confirmed
**Preconditions**:
- Booking ID = 1, Status = Pending hoặc WaitingForApproval

**Steps**:
1. POST /api/bookings/1/complete

**Expected Result**:
- Status Code: 400 Bad Request
- Message: "Chỉ có thể đánh dấu hoàn thành cho booking đã Confirmed"

---

### TC-COMPLETE-004: Đánh dấu hoàn thành thất bại - Không phải chủ sân
**Preconditions**:
- Booking ID = 1, OwnerId = 5
- User đăng nhập với OwnerId = 6

**Steps**:
1. POST /api/bookings/1/complete

**Expected Result**:
- Status Code: 403 Forbidden
- Message: "Bạn không có quyền đánh dấu booking này"

---

### TC-COMPLETE-005: Đánh dấu hoàn thành thành công - Đúng ngày đá
**Mô tả**: Có thể đánh dấu trong ngày đá

**Preconditions**:
- Booking ID = 1, Status = Confirmed, BookingDate = 2024-11-10
- Ngày hiện tại = 2024-11-10

**Steps**:
1. POST /api/bookings/1/complete

**Expected Result**:
- Status Code: 200 OK
- BookingStatus = Completed (5)

---

## 7. Đánh Dấu Không Đến (MarkNoShow)

### TC-NOSHOW-001: Đánh dấu NoShow thành công
**Mô tả**: Khách không đến sân, chủ sân giữ tiền cọc

**Preconditions**:
- Booking ID = 1, Status = Confirmed, BookingDate = 2024-11-01
- Ngày hiện tại = 2024-11-02
- OwnerId = 5

**Steps**:
1. POST /api/bookings/1/no-show

**Expected Result**:
- Status Code: 200 OK
- BookingStatus = NoShow (7)
- Message: "Đánh dấu không đến thành công"

---

### TC-NOSHOW-002: Đánh dấu NoShow thất bại - Chưa đến ngày đá
**Preconditions**:
- Booking ID = 1, Status = Confirmed, BookingDate = 2024-12-25
- Ngày hiện tại = 2024-11-10

**Steps**:
1. POST /api/bookings/1/no-show

**Expected Result**:
- Status Code: 400 Bad Request
- Message: "Chỉ có thể đánh dấu NoShow sau ngày đá"

---

### TC-NOSHOW-003: Đánh dấu NoShow thất bại - Status không phải Confirmed
**Preconditions**:
- Booking ID = 1, Status = Pending

**Steps**:
1. POST /api/bookings/1/no-show

**Expected Result**:
- Status Code: 400 Bad Request
- Message: "Chỉ có thể đánh dấu NoShow cho booking đã Confirmed"

---

### TC-NOSHOW-004: Đánh dấu NoShow thất bại - Không phải chủ sân
**Preconditions**:
- Booking ID = 1, OwnerId = 5
- User đăng nhập với CustomerId = 2

**Steps**:
1. POST /api/bookings/1/no-show

**Expected Result**:
- Status Code: 403 Forbidden
- Message: "Bạn không có quyền đánh dấu booking này"

---

## 8. Xem Danh Sách Booking

### TC-LIST-001: Khách xem danh sách booking của mình
**Preconditions**:
- User đăng nhập với CustomerId = 2
- Có 5 booking với CustomerId = 2

**Steps**:
1. GET /api/bookings/my-bookings

**Expected Result**:
- Status Code: 200 OK
- Trả về danh sách 5 booking
- Sắp xếp theo CreatedAt giảm dần
- Message: "Lấy danh sách booking thành công"

---

### TC-LIST-002: Khách xem booking theo status
**Preconditions**:
- User đăng nhập với CustomerId = 2
- Có 2 booking Confirmed, 3 booking Pending

**Steps**:
1. GET /api/bookings/my-bookings?status=2

**Expected Result**:
- Status Code: 200 OK
- Chỉ trả về 2 booking có status = Confirmed

---

### TC-LIST-003: Chủ sân xem danh sách booking của mình
**Preconditions**:
- User đăng nhập với OwnerId = 5
- Có 10 booking với OwnerId = 5

**Steps**:
1. GET /api/bookings/owner-bookings

**Expected Result**:
- Status Code: 200 OK
- Trả về danh sách 10 booking
- Có đầy đủ thông tin Customer

---

### TC-LIST-004: Chủ sân xem booking theo status
**Preconditions**:
- User đăng nhập với OwnerId = 5
- Có 5 booking WaitingForApproval

**Steps**:
1. GET /api/bookings/owner-bookings?status=1

**Expected Result**:
- Status Code: 200 OK
- Chỉ trả về 5 booking WaitingForApproval

---

### TC-LIST-005: Xem chi tiết booking thành công
**Preconditions**:
- Booking ID = 1, CustomerId = 2
- User đăng nhập với CustomerId = 2

**Steps**:
1. GET /api/bookings/1

**Expected Result**:
- Status Code: 200 OK
- Trả về đầy đủ thông tin booking
- Có Field, TimeSlot, Complex, Customer, Owner info
- BookingStatusText hiển thị tiếng Việt

---

### TC-LIST-006: Xem chi tiết booking thất bại - Không có quyền
**Preconditions**:
- Booking ID = 1, CustomerId = 2, OwnerId = 5
- User đăng nhập với UserId = 7

**Steps**:
1. GET /api/bookings/1

**Expected Result**:
- Status Code: 403 Forbidden

---

### TC-LIST-007: Admin có thể xem mọi booking
**Preconditions**:
- Booking ID = 1
- User đăng nhập với role Admin

**Steps**:
1. GET /api/bookings/1

**Expected Result**:
- Status Code: 200 OK
- Trả về thông tin booking

---

## 9. Auto-Expire Booking

### TC-EXPIRE-001: Tự động expire booking sau 5 phút
**Mô tả**: Background service chạy mỗi 1 phút và expire booking Pending

**Preconditions**:
- Booking ID = 1, Status = Pending
- HoldExpiresAt = 10:00:00
- Thời gian hiện tại = 10:06:00 (quá 5 phút)

**Steps**:
1. Background service chạy tự động
2. Gọi ProcessExpiredBookingsAsync()

**Expected Result**:
- Booking ID = 1 tự động chuyển Status = Expired (6)
- UpdatedAt được cập nhật

---

### TC-EXPIRE-002: Không expire booking đã upload bill
**Preconditions**:
- Booking ID = 1, Status = WaitingForApproval
- HoldExpiresAt đã quá thời gian

**Steps**:
1. Background service chạy

**Expected Result**:
- Booking ID = 1 vẫn giữ Status = WaitingForApproval
- Không bị expire

---

### TC-EXPIRE-003: Không expire booking Confirmed
**Preconditions**:
- Booking ID = 1, Status = Confirmed
- HoldExpiresAt đã quá thời gian

**Steps**:
1. Background service chạy

**Expected Result**:
- Booking ID = 1 vẫn giữ Status = Confirmed
- Không bị expire

---

### TC-EXPIRE-004: Expire nhiều booking cùng lúc
**Preconditions**:
- Có 5 booking Pending với HoldExpiresAt đã quá

**Steps**:
1. Background service chạy

**Expected Result**:
- Cả 5 booking đều chuyển sang Expired
- UpdateRangeAsync được gọi 1 lần

---

## 10. Integration Test Cases

### TC-INTEGRATION-001: Flow hoàn chỉnh - Booking thành công
**Mô tả**: Test toàn bộ flow từ tạo booking đến hoàn thành

**Steps**:
1. Customer tạo booking → Status = Pending
2. Customer upload bill trong 5 phút → Status = WaitingForApproval
3. Owner duyệt → Status = Confirmed
4. Sau ngày đá, Owner đánh dấu Completed → Status = Completed

**Expected Result**:
- Tất cả các bước thành công
- Status transition đúng
- Audit trail đầy đủ

---

### TC-INTEGRATION-002: Flow bị từ chối và đặt lại
**Steps**:
1. Customer tạo booking → Pending
2. Customer upload bill → WaitingForApproval
3. Owner từ chối → Rejected
4. Customer tạo booking mới với cùng field/timeslot → Pending
5. Customer upload bill mới → WaitingForApproval
6. Owner duyệt → Confirmed

**Expected Result**:
- Tất cả các bước thành công
- Booking cũ vẫn ở trạng thái Rejected

---

### TC-INTEGRATION-003: Flow hết hạn và không thể đặt lại
**Steps**:
1. Customer tạo booking → Pending
2. Chờ 6 phút không upload bill → Expired (auto)
3. Customer khác tạo booking cùng field/timeslot/date → Thành công

**Expected Result**:
- Booking 1 tự động Expired
- Booking 2 được tạo thành công vì slot đã được release

---

### TC-INTEGRATION-004: Flow hủy booking
**Steps**:
1. Customer tạo booking → Pending
2. Customer upload bill → WaitingForApproval
3. Customer hủy booking → Cancelled
4. Customer khác có thể đặt cùng slot

**Expected Result**:
- Booking 1 bị Cancelled
- Slot được release, có thể đặt lại

---

## 11. Performance Test Cases

### TC-PERF-001: Load test - Tạo booking đồng thời
**Mô tả**: Test khi nhiều user đặt cùng 1 slot

**Steps**:
1. 10 users đồng thời POST /api/bookings với cùng field/timeslot/date

**Expected Result**:
- Chỉ 1 booking được tạo thành công
- 9 requests còn lại nhận lỗi "Khung giờ này đã được đặt"
- Không có race condition

---

### TC-PERF-002: Load test - Background service
**Mô tả**: Test performance khi có nhiều booking expired

**Steps**:
1. Tạo 1000 booking Pending với HoldExpiresAt đã quá
2. Background service chạy

**Expected Result**:
- Tất cả 1000 booking chuyển sang Expired
- Thời gian xử lý < 10 giây
- Database không bị lock

---

## 12. Security Test Cases

### TC-SEC-001: SQL Injection trong BookingDate
**Steps**:
1. POST /api/bookings
2. Body: bookingDate = "2024-01-01'; DROP TABLE BOOKING; --"

**Expected Result**:
- Request bị reject hoặc parse error
- Database không bị ảnh hưởng

---

### TC-SEC-002: XSS trong Note
**Steps**:
1. POST /api/bookings
2. Body: note = "<script>alert('XSS')</script>"

**Expected Result**:
- Note được lưu nhưng bị escape khi trả về
- Không execute script trên client

---

### TC-SEC-003: IDOR - Truy cập booking của người khác
**Steps**:
1. User A tạo booking ID = 1
2. User B (khác) gọi GET /api/bookings/1

**Expected Result**:
- Status Code: 403 Forbidden
- User B không xem được booking của User A

---

### TC-SEC-004: JWT Token hết hạn
**Steps**:
1. Sử dụng expired token
2. POST /api/bookings

**Expected Result**:
- Status Code: 401 Unauthorized

---

## Notes

- Tất cả test cases nên chạy với database test riêng
- Sau mỗi test case, clean up data để đảm bảo independent
- Mock MinIO service khi test upload file
- Mock DateTime.Now khi test logic thời gian
- Test với timezone khác nhau nếu hệ thống support multiple regions
