using System.Data;
using HotelMangSys.Models;
using HotelMangSys.Models.ViewModels;
using Microsoft.Data.SqlClient;

namespace HotelMangSys.Services
{
    public class BookingService
    {
        private readonly IConfiguration _configuration;
        private readonly IDapperWrapper _dapperWrapper;

        public BookingService(IConfiguration configuration, IDapperWrapper dapperWrapper)
        {
            _configuration = configuration;
            _dapperWrapper = dapperWrapper;
        }

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

        public async Task<int> CreateBookingAsync(Booking booking)
        {
            var connectionString = _configuration["ConnectionStrings:DefaultConnection"];
            if (string.IsNullOrEmpty(connectionString))
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
                return bookingId;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new Exception("Error saving booking: " + ex.Message, ex);
            }
        }

        public async Task<IEnumerable<BookingHistoryDto>> GetBookingHistoryAsync(string userId, int offset, int pageSize)
        {
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

            return await _dapperWrapper.QueryAsync<BookingHistoryDto>(db, sql, new { UserId = userId, Offset = offset, PageSize = pageSize });
        }

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