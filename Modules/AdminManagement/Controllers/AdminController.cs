using FootballField.API.Modules.BookingManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FootballField.API.Shared.Middlewares;

namespace FootballField.API.Modules.AdminManagement.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public AdminController(IBookingService authService)
        {
            _bookingService = authService;
        }

        [HttpPatch("{id}/force-complete")]
        [HasPermission("booking.force_complete")]
        public async Task<IActionResult> AdminForceCompleteBooking(int id)
        {
            try
            {
                await _bookingService.AdminForceCompleteBookingAsync(id);
                return Ok(new { message = "Booking đã được chuyển sang trạng thái Completed (FOR TESTING)" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
