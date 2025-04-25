using Dapper;
using HotelMangSys.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using AspNetCoreGeneratedDocument;
using HotelMangSys.Models.ViewModels;

namespace HotelMangSys.Controllers
{
    /// <summary>
    /// Controller for managing bookings.
    /// </summary>
    public class BookingController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<BookingController> _logger;
      

        public BookingController(IConfiguration configuration, ILogger<BookingController> logger )
        {
            _configuration = configuration;
            _logger = logger;
            
        }

        /// <summary>
        /// // GET: Create a new booking.
        /// </summary>
        /// <returns></returns>

        [HttpGet]
        public IActionResult CreateBooking()
        {
            var userId = Request.Cookies["UserId"];

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("UserId cookie not found. Redirecting to login.");
                // Redirect to login if cookie not found
                return RedirectToAction("Login", "Account");
            }

            var model = new Booking
            {
                UserId = userId,
                BookingDate = DateTime.Today,
                CheckoutDate = DateTime.Today.AddDays(1)
            };

            return View(model); // This sends the UserId to the form
        }

        /// <summary>
        /// Creates a new booking, save data to database
        /// </summary>
        /// <param name="booking"></param>
        /// <returns></returns>

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBooking(Booking booking)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state is invalid. Returning to view.");
                ModelState.AddModelError("", "Invalid booking details.");
                return View(booking);
            }

            using (IDbConnection db = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                try
                {
                    // Get available room
                    var sqlQuery = @"
                SELECT TOP 1 Id
                FROM Rooms
                WHERE Type = @RoomType AND IsAvailable = 1";

                    var availableRoom = await db.QuerySingleOrDefaultAsync<Room>(sqlQuery, new { RoomType = booking.RoomType });

                    if (availableRoom == null)
                    {
                        _logger.LogWarning("No available room found for the selected type.");
                        ModelState.AddModelError("", "No available room of the selected type.");
                        return View(booking);
                    }

                    booking.RoomId = availableRoom.Id;
                    booking.Status = "Confirmed";

                    var insertQuery = @"
                INSERT INTO Bookings (UserId, RoomId, RoomType, BookingDate, CheckoutDate, Status)
                VALUES (@UserId, @RoomId, @RoomType, @BookingDate, @CheckoutDate, @Status);
                SELECT CAST(SCOPE_IDENTITY() as int);";

                    var bookingId = await db.QuerySingleAsync<int>(insertQuery, booking);

                    await db.ExecuteAsync("UPDATE Rooms SET IsAvailable = 0 WHERE Id = @RoomId", new { booking.RoomId });

                    _logger.LogInformation($"Booking {bookingId} created for user {booking.UserId}.");
                    return RedirectToAction("BookingConfirmation", new { bookingId });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating booking.");
                    ModelState.AddModelError("", "An error occurred while saving the booking.");
                    return View(booking);
                }
            }
        }

        /// <summary>
        /// /// Displays the booking confirmation page after a successful booking.
        /// </summary>
        /// <param name="bookingId"></param>
        /// <returns></returns>
        public IActionResult BookingConfirmation(int bookingId)
        {
            _logger.LogInformation($"Booking confirmation page accessed for booking ID: {bookingId}");
            ViewBag.BookingId = bookingId;
            return View();
        }



        /// <summary>
        /// /// Retrieves the booking history for the logged-in user.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>

        public async Task<IActionResult> BookingHistory(int page = 1, int pageSize = 5)
        {
            // Retrieve the UserId from the cookie
            string userId = Request.Cookies["UserId"];

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("UserId cookie not found. Redirecting to login.");
                return RedirectToAction("Login"); // Redirect to login if no UserId is found
            }

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (IDbConnection db = new SqlConnection(connectionString))
            {
                // Calculate the offset for pagination
                int offset = (page - 1) * pageSize;

                var query = @"
            SELECT b.Id AS BookingId, b.UserId, b.RoomId, r.Type AS RoomType, b.BookingDate, b.CheckoutDate, b.Status
            FROM Bookings b
            INNER JOIN Rooms r ON b.RoomId = r.Id
            WHERE b.UserId = @UserId
            ORDER BY b.BookingDate
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                var parameters = new { UserId = userId, Offset = offset, PageSize = pageSize };

                var bookings = await db.QueryAsync<BookingHistoryDto>(query, parameters);
                _logger.LogInformation($"Fetched {bookings.Count()} bookings for user {userId}.");

                if (!bookings.Any())
                {
                    return View("NoBookings");
                }

                // Calculate total count and total pages
                var totalCountQuery = "SELECT COUNT(*) FROM Bookings WHERE UserId = @UserId";
                int totalCount = await db.QuerySingleAsync<int>(totalCountQuery, new { UserId = userId });
                int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                // Create the view model
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
}
        



            


