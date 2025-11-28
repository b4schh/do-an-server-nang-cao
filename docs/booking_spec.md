## Cấu trúc bảng BOOKING

| Cột chính                                        | Giải thích                                               |
| ------------------------------------------------ | -------------------------------------------------------- |
| **id**                                           | Mã đơn đặt sân (PRIMARY KEY)                             |
| **field_id**                                     | FK → FIELD(id) — sân con được đặt                        |
| **customer_id**                                  | FK → USER(id) — người đặt sân                            |
| **owner_id**                                     | FK → USER(id) — chủ sân sở hữu sân đó                    |
| **time_slot_id**                                 | FK → TIME_SLOT(id) — khung giờ cụ thể                    |
| **booking_date**                                 | Ngày diễn ra trận đấu                                    |
| **hold_expires_at**                              | Thời điểm hết hạn giữ chỗ (ví dụ 5 phút sau khi tạo)     |
| **total_amount**                                 | Tổng giá trị sân (toàn bộ số tiền nếu khách đến đá)      |
| **deposit_amount**                               | Số tiền cọc mà khách phải chuyển khoản trước khi giữ sân |
| **payment_proof_url**                            | Ảnh bill chuyển khoản tiền cọc do khách upload           |
| **payment_note**                                 | Ghi chú hoặc mã giao dịch khách nhập thủ công            |
| **booking_status**                               | Trạng thái của đơn đặt sân                               |
| **approved_by**                                  | Ai duyệt đơn cọc                                         |
| **approved_at**                                  | Thời điểm duyệt                                          |
| **cancelled_by**                                 | Ai hủy đơn                                               |
| **cancelled_at**                                 | Thời điểm hủy                                            |
| **created_at**                                   | Thời điểm tạo                                            |
| **updated_at**                                   | Thời điểm cập nhật                                       |
| **UNIQUE(field_id, booking_date, time_slot_id)** | Tránh đặt trùng sân cùng giờ                             |

### Danh sách trạng thái booking_status
| Mã    | Trạng thái         | Mô tả                                          |
| ----- | ------------------ | ---------------------------------------------- |
| **0** | Pending            | Khách vừa tạo booking, chưa upload bill cọc    |
| **1** | WaitingForApproval | Khách đã upload bill, chờ chủ sân xác nhận cọc |
| **2** | Confirmed          | Chủ sân duyệt cọc → giữ chỗ thành công         |
| **3** | Rejected           | Chủ sân từ chối bill                           |
| **4** | Cancelled          | Khách hoặc chủ sân hủy đơn                     |
| **5** | Completed          | Trận đấu diễn ra và hoàn tất                   |
| **6** | Expired            | Quá hạn giữ chỗ (khách không upload bill)      |
| **7** | NoShow             | Khách không đến sân → giữ tiền cọc             |

### Quy trình nghiệp vụ đặt sân
Bước 1: Khách tìm và chọn sân + khung giờ
Lấy dữ liệu từ FIELD và TIME_SLOT.

Bước 2: Khách nhấn “Đặt sân” → Tạo booking Pending
Hệ thống tạo đơn tạm Pending.
hold_expires_at = now + 5 phút.

Bước 3: Hiển thị QR và thông tin thanh toán
Khách chuẩn bị chuyển khoản đặt cọc.

Bước 4: Khách chuyển khoản và upload bill
Chuyển trạng thái → WaitingForApproval.

Bước 5: Chủ sân duyệt hoặc từ chối
Approved → Confirmed
Rejected → Rejected

Bước 6: Đến ngày đá → Hoàn thành
Nếu khách đến → Completed

Bước 7: Khách không đến
Chủ sân đặt trạng thái → NoShow

Bước 8: Hủy đơn
Khách hoặc chủ sân hủy trước trận → Cancelled.

Bước 9: Tự động hết hạn
Nếu khách không upload bill trong 5 phút → Expired.