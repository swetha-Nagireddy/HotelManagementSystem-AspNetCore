using System.Data;
using HotelMangSys.Controllers;
using HotelMangSys.Models;
using HotelMangSys.Models.ViewModels;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;

namespace HotelMangSys.Services
{
    public class BookingService
    {
        private readonly IConfiguration _configuration;
        private readonly IDapperWrapper _dapperWrapper;
        private readonly IMemoryCache _cache;


        public BookingService(IConfiguration configuration, IDapperWrapper dapperWrapper,IMemoryCache cache)
        {
            _configuration = configuration;
            _dapperWrapper = dapperWrapper;
            _cache = cache;

        }
        /// <summary>
        /// method to get an available room of a specific type
        /// </summary>
        /// <param name="roomType"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<Room?> GetAvailableRoomAsync(string roomType)
        {
            var connectionString = _configuration["ConnectionStrings:DefaultConnection"];
            if (string.IsNullOrEmpty(connectionString))
            {
                
                throw new InvalidOperationException("Connection string is not configured.");
            }

            using IDbConnection db = new SqlConnection(connectionString);
           
            string sql = @"SELECT TOP 1 * FROM Rooms WHERE Type = @RoomType AND IsAvailable = 1";
            return (await _dapperWrapper.QueryAsync<Room>(db, sql, new { RoomType = roomType })).FirstOrDefault();
        }

        /// <summary>
        /// method to create a booking and update the room availability
        /// </summary>
        /// <param name="booking"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<int> CreateBookingAsync(Booking booking)
        {
            var connectionString = _configuration["ConnectionStrings:DefaultConnection"];
            if (string.IsNullOrEmpty(connectionString))//checking if connection string is configured or not
            {
                
                throw new InvalidOperationException("Connection string is not configured.");
            }

            using IDbConnection db = new SqlConnection(connectionString);
            db.Open();
            using var transaction = db.BeginTransaction();

            try
            {
                
                string insertBooking = @"
                INSERT INTO Bookings (UserId, RoomId, RoomType, BookingDate, CheckoutDate, Status)
                VALUES (@UserId, @RoomId, @RoomType, @BookingDate, @CheckoutDate, @Status);
                SELECT CAST(SCOPE_IDENTITY() as int);";

                int bookingId = await _dapperWrapper.QuerySingleAsync<int>(db, insertBooking, booking, transaction);
               
                string updateRoom = "UPDATE Rooms SET IsAvailable = 0 WHERE Id = @RoomId";
                await _dapperWrapper.ExecuteAsync(db, updateRoom, new { booking.RoomId }, transaction);

                transaction.Commit();
                // 🧹 CLEAR booking history cache after a successful booking
                ClearBookingHistoryCache(booking.UserId);

                return bookingId;
            }
            catch (Exception ex)
            {
               
                transaction.Rollback();
               
                throw new Exception("Error saving booking: " + ex.Message, ex);
            }
        }
        private void ClearBookingHistoryCache(string userId)
        {
            // Clear cached pages, assuming user might have visited multiple pages
            for (int page = 1; page <= 5; page++) // You can adjust how many pages you want to clear
            {
                string cacheKey = $"BookingHistory_{userId}_Page_{page}_Size_5"; // if pageSize = 5 fixed
                _cache.Remove(cacheKey);
            }
        }

        /// <summary>
        /// method to get booking history for a user
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="offset"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<IEnumerable<BookingHistoryDto>> GetBookingHistoryAsync(string userId, int offset, int pageSize)
        {
            var cacheKey = $"BookingHistory_{userId}_{offset}_{pageSize}"; // Unique per user and paging

            if (_cache.TryGetValue(cacheKey, out IEnumerable<BookingHistoryDto> cachedBookingHistory))
            {
                return cachedBookingHistory; // Return cached result if found
            }

            var connectionString = _configuration["ConnectionStrings:DefaultConnection"];
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string is not configured.");
            }

            using IDbConnection db = new SqlConnection(connectionString);

            string sql = @"
        SELECT b.Id AS BookingId, b.UserId, b.RoomId, r.Type AS RoomType, b.BookingDate, b.CheckoutDate, b.Status
        FROM Bookings b
        INNER JOIN Rooms r ON b.RoomId = r.Id
        WHERE b.UserId = @UserId
        ORDER BY b.BookingDate
        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var bookingHistory = await _dapperWrapper.QueryAsync<BookingHistoryDto>(db, sql, new { UserId = userId, Offset = offset, PageSize = pageSize });

            // Set in cache
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5)); // Cache duration (you can adjust)

            _cache.Set(cacheKey, bookingHistory, cacheEntryOptions);

            return bookingHistory;
        }

        /// <summary>
        /// method to get the total number of bookings for a user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<int> GetBookingCountAsync(string userId)
        {
            var connectionString = _configuration["ConnectionStrings:DefaultConnection"];
            if (string.IsNullOrEmpty(connectionString))
            {
                
                throw new InvalidOperationException("Connection string is not configured.");
            }

            using IDbConnection db = new SqlConnection(connectionString);
           
            string sql = "SELECT COUNT(*) FROM Bookings WHERE UserId = @UserId";
           
            return await _dapperWrapper.QuerySingleAsync<int>(db, sql, new { UserId = userId });
            
        }
    }
}