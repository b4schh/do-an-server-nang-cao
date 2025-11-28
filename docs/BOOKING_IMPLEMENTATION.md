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