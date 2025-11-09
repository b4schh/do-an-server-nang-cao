### Bảng USER

| Trường         | Kiểu dữ liệu  | Ràng buộc / Ghi chú                                   |
| -------------- | ------------- | ----------------------------------------------------- |
| id             | int           | PRIMARY KEY                                           |
| last_name      | nvarchar(100) | NOT NULL                                              |
| first_name     | nvarchar(100) | NOT NULL                                              |
| email          | varchar(200)  | UNIQUE, CHECK định dạng email hợp lệ                  |
| phone          | varchar(15)   | CHECK (LEN(phone) = 10 AND chỉ chứa số)               |
| password       | varchar(255)  | Mật khẩu được hash                                    |
| role           | tinyint       | CHECK (0=Customer, 1=Owner, 2=Admin)                  |
| avatar_url     | varchar(MAX)  | Ảnh đại diện                                          |
| status         | tinyint       | CHECK (0=Inactive, 1=Active, 2=Banned)                |
| is_deleted     | bit           | DEFAULT 0, CHECK (0=False, 1=True)                    |
| deleted_by     | int           | FK → USER(id), người thực hiện thao tác xóa tài khoản |
| created_at     | datetime      | DEFAULT GETDATE()                                     |
| updated_at     | datetime      | DEFAULT GETDATE(), cập nhật khi record thay đổi       |
| deleted_at     | datetime      | NULL, thời điểm xóa mềm                               |

### Bảng COMPLEX (Cụm sân)

| Trường       | Kiểu dữ liệu  | Ràng buộc / Ghi chú                         |
| ------------ | ------------- | ------------------------------------------- |
| id           | int           | PRIMARY KEY                                 |
| owner_id     | int           | FOREIGN KEY → USER(id), ON DELETE NO ACTION |
| name         | nvarchar(255) | NOT NULL                                    |
| street       | nvarchar(100) |                                             |
| ward         | nvarchar(100) |                                             |
| province     | nvarchar(100) |                                             |
| phone        | varchar(15)   | Số điện thoại cụm sân                       |
| opening_time | time          | Giờ mở cửa                                  |
| closing_time | time          | Giờ đóng cửa                                |
| description  | nvarchar(500) |                                             |
| status       | tinyint       | CHECK (0=Pending, 1=Approved, 2=Rejected)   |
| is_active    | bit           | DEFAULT 1, CHECK (0=False, 1=True)          |
| is_deleted   | bit           | DEFAULT 0, CHECK (0=False, 1=True)          |
| created_at   | datetime      | DEFAULT GETDATE()                           |
| updated_at   | datetime      | DEFAULT GETDATE()                           |
| deleted_at   | datetime      | NULL                                        |

### Bảng FIELD (Sân con)

| Trường       | Kiểu dữ liệu  | Ràng buộc / Ghi chú                            |
| ------------ | ------------- | ---------------------------------------------- |
| id           | int           | PRIMARY KEY                                    |
| complex_id   | int           | FOREIGN KEY → COMPLEX(id), ON DELETE NO ACTION |
| name         | nvarchar(100) | NOT NULL                                       |
| surface_type | nvarchar(50)  | Loại mặt sân (cỏ nhân tạo, cỏ tự nhiên, …)     |
| field_size   | nvarchar(50)  | Kích thước sân (5 người, 7 người, 11 người)    |
| is_active    | bit           | DEFAULT 1, CHECK (0=False, 1=True)             |
| is_deleted   | bit           | DEFAULT 0, CHECK (0=False, 1=True)             |
| created_at   | datetime      | DEFAULT GETDATE()                              |
| updated_at   | datetime      | DEFAULT GETDATE()                              |
| deleted_at   | datetime      | NULL                                           |

### Bảng TIME_SLOT (Khung giờ)

| Trường                | Kiểu dữ liệu  | Ràng buộc / Ghi chú                                             |
| --------------------- | ------------- | --------------------------------------------------------------- |
| id                    | int           | PRIMARY KEY                                                     |
| field_id              | int           | FOREIGN KEY → FIELD(id), ON DELETE CASCADE                      |
| start_time            | time          | CHECK (start_time < end_time)                                   |
| end_time              | time          | CHECK (end_time > start_time)                                   |
| price                 | decimal(10,2) | CHECK (price > 0)                                               |
| is_active             | bit           | DEFAULT 1, CHECK (0=False, 1=True)                              |
| created_at            | datetime      | DEFAULT GETDATE()                                               |
| updated_at            | datetime      | DEFAULT GETDATE()                                               |
| **Ràng buộc bổ sung** |               | UNIQUE(field_id, start_time, end_time) để tránh trùng khung giờ |

### Bảng FAVORITE_COMPLEX (Sân yêu thích)

| Trường                | Kiểu dữ liệu | Ràng buộc / Ghi chú                         |
| --------------------- | ------------ | ------------------------------------------- |
| id                    | int          | PRIMARY KEY                                 |
| user_id               | int          | FK → USER(id), ON DELETE CASCADE            |
| complex_id            | int          | FK → COMPLEX(id), ON DELETE CASCADE         |
| created_at            | datetime     | DEFAULT GETDATE()                           |
| **Ràng buộc bổ sung** |              | UNIQUE(user_id, complex_id) tránh trùng lặp |

### Bảng BOOKING (Đơn đặt sân)

| Trường                | Kiểu dữ liệu  | Ràng buộc / Ghi chú                                                                                       |
| --------------------- | ------------- | --------------------------------------------------------------------------------------------------------- |
| id                    | int           | PRIMARY KEY                                                                                               |
| field_id              | int           | FOREIGN KEY → FIELD(id), ON DELETE NO ACTION                                                              |
| customer_id           | int           | FOREIGN KEY → USER(id), ON DELETE NO ACTION                                                               |
| owner_id              | int           | FOREIGN KEY → USER(id), ON DELETE NO ACTION, lấy từ COMPLEX.owner_id khi tạo booking                      |
| time_slot_id          | int           | FOREIGN KEY → TIME_SLOT(id), ON DELETE NO ACTION                                                          |
| booking_date          | date          | Ngày diễn ra trận đấu, CHECK chỉ cho phép đặt giờ trong tương lai                                         |
| hold_expires_at       | datetime      | Thời gian hết hiệu lực giữ chỗ (5 phút sau khi đặt)                                                       |
| total_amount          | decimal(10,2) | CHECK (total_amount > 0)                                                                                  |
| deposit_amount        | decimal(10,2) | CHECK (deposit_amount >= 0 AND deposit_amount <= total_amount)                                            |
| payment_proof_url     | varchar(MAX)  | Link ảnh bill chuyển khoản                                                                                |
| note                  | nvarchar(255) | Ghi chú khách hàng nhập                                                                                   |
| booking_status        | tinyint       | (0=Pending, 1=WaitingForApproval, 2=Confirmed, 3=Rejected, 4=Cancelled, 5=Completed, 6=Expired, 7=NoShow) |
| approved_by           | int           | FK → USER(id), người duyệt bill                                                                           |
| approved_at           | datetime      | NULL, thời gian chủ sân duyệt                                                                             |
| cancelled_by          | int           | NULL, FK → USER(id), người hủy đơn (chủ sân hoặc khách)                                                   |
| cancelled_at          | datetime      | NULL                                                                                                      |
| created_at            | datetime      | DEFAULT GETDATE()                                                                                         |
| updated_at            | datetime      | DEFAULT GETDATE()                                                                                         |
| **Ràng buộc bổ sung** |               | UNIQUE(field_id, booking_date, time_slot_id) để tránh đặt trùng                                           |

### Bảng REVIEW (Đánh giá sân)              

| Trường      | Kiểu dữ liệu  | Ràng buộc / Ghi chú                              |
| ----------- | ------------- | ------------------------------------------------ |
| id          | int           | PRIMARY KEY                                      |
| booking_id  | int           | FOREIGN KEY → BOOKING(id), ON DELETE CASCADE     |
| customer_id | int           | FOREIGN KEY → USER(id), ON DELETE NO ACTION      |
| complex_id  | int           | FOREIGN KEY → COMPLEX(id), ON DELETE NO ACTION   |
| rating      | tinyint       | CHECK (rating BETWEEN 1 AND 5)                   |
| comment     | nvarchar(255) | Chỉ cho phép đánh giá sau khi hoàn thành booking |
| is_visible  | bit           | DEFAULT 1, CHECK (0=False, 1=True)               |
| is_deleted  | bit           | DEFAULT 0, CHECK (0=False, 1=True)               |
| created_at  | datetime      | DEFAULT GETDATE()                                |
| updated_at  | datetime      | DEFAULT GETDATE()                                |
| deleted_at  | datetime      | NULL                                             |

### Bảng: COMPLEX_IMAGE (Ảnh cụm sân)

| Trường     | Kiểu dữ liệu | Ràng buộc / Ghi chú                          |
| ---------- | ------------ | -------------------------------------------- |
| id         | int          | PRIMARY KEY                                  |
| complex_id | int          | FOREIGN KEY → COMPLEX(id), ON DELETE CASCADE |
| image_url  | varchar(MAX) | CHECK (LEN(image_url) > 0)                   |
| is_main    | bit          | DEFAULT 0, CHECK (0=False, 1=True)           |

### Bảng OWNER_SETTING

| Trường               | Kiểu dữ liệu  | Ràng buộc / Ghi chú                                                                          |
| -------------------- | ------------- | -------------------------------------------------------------------------------------------- |
| id                   | int           | PRIMARY KEY                                                                                  |
| owner_id             | int           | FOREIGN KEY → USER(id), ràng buộc `role = 1 (Owner)`, UNIQUE, ON DELETE CASCADE              |
| deposit_rate         | decimal(5,2)  | Phần trăm tiền cọc, CHECK (deposit_rate BETWEEN 0 AND 1), ví dụ 0.3 = 30%                    |
| min_booking_notice   | int           | Thời gian tối thiểu (phút) trước giờ đá để cho phép đặt sân, CHECK >= 0                      |
| allow_review         | bit           | Cho phép khách hàng đánh giá sân của mình, DEFAULT 1, CHECK (0=False, 1=True)                |
| created_at           | datetime      | DEFAULT GETDATE()                                                                            |
| updated_at           | datetime      | DEFAULT GETDATE()                                                                            |

### Bảng SYSTEM_CONFIG

| Trường       | Kiểu dữ liệu  | Ràng buộc / Ghi chú                                             |
| ------------ | ------------- | --------------------------------------------------------------- |
| id           | int           | PRIMARY KEY                                                     |
| config_key   | nvarchar(100) | UNIQUE, tên cấu hình hệ thống                                   |
| config_value | nvarchar(255) | Giá trị cấu hình                                                |
| data_type    | varchar(20)   | 'string', 'int', 'decimal', 'boolean', 'json', DEFAULT 'string' |
| description  | nvarchar(255) | Mô tả ý nghĩa cấu hình                                          |
| updated_at   | datetime      | DEFAULT GETDATE(), thời điểm cập nhật cấu hình gần nhất         |

### Bảng USER_ACTIVITY_LOG

| Trường       | Kiểu dữ liệu  | Ràng buộc / Ghi chú                                                         |
| ------------ | ------------- | --------------------------------------------------------------------------- |
| id           | bigint        | PRIMARY KEY (tự tăng)                                                       |
| user_id      | int           | FK → USER(id), có thể NULL nếu là hành động public                          |
| action       | nvarchar(100) | Loại hành động (CreateBooking, CancelBooking, AddField, ApproveComplex,...) |
| target_table | nvarchar(100) | Tên bảng bị ảnh hưởng (vd: "Booking", "Field", "Complex")                   |
| target_id    | int           | ID bản ghi bị ảnh hưởng                                                     |
| description  | nvarchar(500) | Mô tả chi tiết hành động (vd: “Chủ sân A đã hủy đơn ###123”)                |
| created_at   | datetime      | DEFAULT GETDATE()                                                           |

### Bảng SYSTEM_LOG

| Trường     | Kiểu dữ liệu  | Ràng buộc / Ghi chú                                                 |
| ---------- | ------------- | ------------------------------------------------------------------- |
| id         | bigint        | PRIMARY KEY (tự tăng)                                               |
| log_level  | varchar(20)   | Mức độ log: INFO, WARNING, ERROR, CRITICAL                          |
| source     | nvarchar(100) | Nguồn log (vd: “BookingService”, “AuthController”)                  |
| message    | nvarchar(MAX) | Nội dung log chi tiết                                               |
| created_at | datetime      | DEFAULT GETDATE()                                                   |

### Bảng NOTIFICATION

| Trường        | Kiểu dữ liệu  | Ràng buộc / Ghi chú                                              |
| ------------- | ------------- | ---------------------------------------------------------------- |
| id            | int           | PRIMARY KEY                                                      |
| user_id       | int           | FOREIGN KEY → USER(id), người nhận thông báo                     |
| sender_id     | int           | FOREIGN KEY → USER(id), người gửi (có thể NULL nếu hệ thống gửi) |
| title         | nvarchar(255) | Tiêu đề ngắn gọn của thông báo                                   |
| message       | nvarchar(MAX) | Nội dung chi tiết                                                |
| type          | tinyint       | CHECK (0=System, 1=Booking, 2=Payment, 3=Review, 4=Other)        |
| related_table | nvarchar(100) | Tên bảng liên quan (Booking, Field, Complex, …)                  |
| related_id    | int           | ID của bản ghi liên quan (vd: booking_id)                        |
| is_read       | bit           | DEFAULT 0, CHECK (0=False, 1=True)                               |
| created_at    | datetime      | DEFAULT GETDATE()                                                |
| read_at       | datetime      | NULL, thời gian người nhận đọc thông báo                         |



