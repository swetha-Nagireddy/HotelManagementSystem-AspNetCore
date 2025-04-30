namespace HotelMangSys.Models
{
    // This class represents a booking history entry in the hotel management system.
    public class BookingHistoryDto
    {
        public int BookingId { get; set; }
        public string UserId { get; set; }
        public int RoomId { get; set; }
        public string RoomType { get; set; }
        public DateTime BookingDate { get; set; }
        public DateTime? CheckoutDate { get; set; }
        public string Status { get; set; }
    }
}
