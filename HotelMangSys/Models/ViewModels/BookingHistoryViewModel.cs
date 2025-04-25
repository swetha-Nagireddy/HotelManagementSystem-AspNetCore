namespace HotelMangSys.Models.ViewModels
{
    public class BookingHistoryViewModel
    {
        public List<BookingHistoryDto> Bookings { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
    }
}
