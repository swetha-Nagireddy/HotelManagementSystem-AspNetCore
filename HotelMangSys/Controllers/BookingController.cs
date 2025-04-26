// BookingController.cs (simplified)
using HotelMangSys.Models;
using HotelMangSys.Models.ViewModels;
using HotelMangSys.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelMangSys.Controllers
{
    [Route("Booking")]
    public class BookingController : Controller
    {
        private readonly BookingService _bookingService;
        private readonly ILogger<BookingController> _logger;

        public BookingController(BookingService bookingService, ILogger<BookingController> logger)
        {
            _bookingService = bookingService;
            _logger = logger;
        }

        [HttpGet("book-room")]
        public IActionResult CreateBooking()
        {
            var userId = Request.Cookies["UserId"];
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            var model = new Booking
            {
                UserId = userId,
                BookingDate = DateTime.Today,
                CheckoutDate = DateTime.Today.AddDays(1)
            };

            return View(model);
        }

        [HttpPost("CreatingBooking")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBooking(Booking booking)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid booking model.");
                ModelState.AddModelError("", "Invalid booking details.");
                return View(booking);
            }

            try
            {
                var availableRoom = await _bookingService.GetAvailableRoomAsync(booking.RoomType);
                if (availableRoom == null)
                {
                    _logger.LogWarning("No available room found.");
                    ModelState.AddModelError("", "No available room of the selected type.");
                    return View(booking);
                }

                booking.RoomId = availableRoom.Id;
                booking.Status = "Confirmed";

                int bookingId = await _bookingService.CreateBookingAsync(booking);

                _logger.LogInformation($"Booking {bookingId} created.");
                return RedirectToAction("BookingConfirmation", new { bookingId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating booking.");
                ModelState.AddModelError("", "An error occurred while saving the booking.");
                return View(booking);
            }
        }
        [HttpGet("BookingConfirmation/{bookingId}")]
        public IActionResult BookingConfirmation(int bookingId)
        {
            ViewBag.BookingId = bookingId;
            return View();
        }

        [HttpGet("History")]
        public async Task<IActionResult> BookingHistory(int page = 1, int pageSize = 5)
        {
            string userId = Request.Cookies["UserId"];

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("UserId cookie not found. Redirecting to login.");
                return RedirectToAction("Login");
            }

            int offset = (page - 1) * pageSize;

            var bookings = await _bookingService.GetBookingHistoryAsync(userId, offset, pageSize);
            if (!bookings.Any())
                return View("NoBookings");

            int totalCount = await _bookingService.GetBookingCountAsync(userId);
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var viewModel = new BookingHistoryViewModel
            {
                Bookings = bookings.ToList(),
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalCount = totalCount
            };

            return View(viewModel);
        }

    }
}
