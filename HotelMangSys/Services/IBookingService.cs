using HotelMangSys.Models;

namespace HotelMangSys.Services
{
    /// <summary>
    ///  Interface for booking service to handle booking operations
    /// </summary>
    public interface IBookingService
    {
        Task<Booking?> CreateBookingAsync(Booking booking);
        Task<List<BookingHistoryDto>> GetBookingHistoryAsync(string userId, int page, int pageSize);
        Task<int> GetTotalBookingCountAsync(string userId);
    }
}
