using HotelMangSys.Models;

namespace HotelMangSys.Services
{
    public interface IBookingService
    {
        Task<Booking?> CreateBookingAsync(Booking booking);
        Task<List<BookingHistoryDto>> GetBookingHistoryAsync(string userId, int page, int pageSize);
        Task<int> GetTotalBookingCountAsync(string userId);
    }
}
