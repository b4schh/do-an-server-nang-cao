using FootballField.API.Shared.Dtos;
using FootballField.API.Modules.BookingManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FootballField.API.Shared.Dtos.BookingManagement;
using FootballField.API.Modules.BookingManagement.Entities;

namespace FootballField.API.Modules.BookingManagement.Controllers
{
    [ApiController]
    [Route("api/bookings")]
    [Authorize]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        // Helper method to get current user ID from JWT
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("Không thể xác định user");
            }
            return userId;
        }

        // POST api/bookings - Khách tạo booking (Bước 1)
        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto dto)
        {
            try
            {
                var customerId = GetCurrentUserId();
                var booking = await _bookingService.CreateBookingAsync(customerId, dto);
                return CreatedAtAction(
                    nameof(GetBookingById), 
                    new { id = booking.Id }, 
                    ApiResponse<BookingDto>.Ok(booking, "Tạo booking thành công. Vui lòng upload bill thanh toán trong 5 phút.", 201)
                );
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<string>.Fail(ex.Message, 400));
            }
        }

        // POST api/bookings/{id}/upload-payment - Khách upload bill (Bước 2)
        [HttpPost("{id}/upload-payment")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> UploadPaymentProof(int id, [FromForm] UploadPaymentProofDto dto)
        {
            try
            {
                var customerId = GetCurrentUserId();
                var booking = await _bookingService.UploadPaymentProofAsync(id, customerId, dto);
                return Ok(ApiResponse<BookingDto>.Ok(booking, "Upload bill thanh toán thành công. Đang chờ chủ sân duyệt."));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<string>.Fail(ex.Message, 400));
            }
        }

        // POST api/bookings/{id}/approve - Chủ sân duyệt (Bước 3)
        [HttpPost("{id}/approve")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> ApproveBooking(int id)
        {
            try
            {
                var ownerId = GetCurrentUserId();
                var booking = await _bookingService.ApproveBookingAsync(id, ownerId);
                return Ok(ApiResponse<BookingDto>.Ok(booking, "Duyệt booking thành công"));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<string>.Fail(ex.Message, 400));
            }
        }

        // POST api/bookings/{id}/reject - Chủ sân từ chối
        [HttpPost("{id}/reject")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> RejectBooking(int id, [FromBody] RejectBookingDto dto)
        {
            try
            {
                var ownerId = GetCurrentUserId();
                var booking = await _bookingService.RejectBookingAsync(id, ownerId, dto.Reason);
                return Ok(ApiResponse<BookingDto>.Ok(booking, "Từ chối booking thành công"));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<string>.Fail(ex.Message, 400));
            }
        }

        // POST api/bookings/{id}/cancel - Hủy booking
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelBooking(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var booking = await _bookingService.CancelBookingAsync(id, userId);
                return Ok(ApiResponse<BookingDto>.Ok(booking, "Hủy booking thành công"));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<string>.Fail(ex.Message, 400));
            }
        }

        // POST api/bookings/{id}/complete - Chủ sân đánh dấu hoàn thành
        [HttpPost("{id}/complete")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> MarkCompleted(int id)
        {
            try
            {
                var ownerId = GetCurrentUserId();
                var booking = await _bookingService.MarkCompletedAsync(id, ownerId);
                return Ok(ApiResponse<BookingDto>.Ok(booking, "Đánh dấu hoàn thành thành công"));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<string>.Fail(ex.Message, 400));
            }
        }

        // POST api/bookings/{id}/no-show - Chủ sân đánh dấu khách không đến
        [HttpPost("{id}/no-show")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> MarkNoShow(int id)
        {
            try
            {
                var ownerId = GetCurrentUserId();
                var booking = await _bookingService.MarkNoShowAsync(id, ownerId);
                return Ok(ApiResponse<BookingDto>.Ok(booking, "Đánh dấu không đến thành công"));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<string>.Fail(ex.Message, 400));
            }
        }

        // GET api/bookings/my-bookings - Khách xem booking của mình
        [HttpGet("my-bookings")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyBookings([FromQuery] BookingStatus? status = null)
        {
            var customerId = GetCurrentUserId();
            var bookings = await _bookingService.GetBookingsForCustomerAsync(customerId, status);
            return Ok(ApiResponse<IEnumerable<BookingDto>>.Ok(bookings, "Lấy danh sách booking thành công"));
        }

        // GET api/bookings/owner-bookings - Chủ sân xem booking của mình
        [HttpGet("owner-bookings")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> GetOwnerBookings([FromQuery] BookingStatus? status = null)
        {
            var ownerId = GetCurrentUserId();
            var bookings = await _bookingService.GetBookingsForOwnerAsync(ownerId, status);
            return Ok(ApiResponse<IEnumerable<BookingDto>>.Ok(bookings, "Lấy danh sách booking thành công"));
        }

        // GET api/bookings/{id} - Xem chi tiết booking
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBookingById(int id)
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null)
            {
                return NotFound(ApiResponse<string>.Fail("Không tìm thấy booking", 404));
            }

            // Check authorization - only customer, owner, or admin can view
            var userId = GetCurrentUserId();
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            
            if (userRole != "Admin" && booking.CustomerId != userId && booking.OwnerId != userId)
            {
                return Forbid();
            }

            return Ok(ApiResponse<BookingDto>.Ok(booking, "Lấy thông tin booking thành công"));
        }
    }
}
