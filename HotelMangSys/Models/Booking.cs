using HotelMangSys.Models;

namespace HotelMangSys.Models{
public class Booking
{
    public int Id { get; set; }

    public string UserId { get; set; }

    // 🔥 Make navigation properties optional
    public ApplicationUser? User { get; set; }

    public string RoomType { get; set; }

        public int RoomId { get; set; }

        public Room? Room { get; set; }

    public DateTime BookingDate { get; set; }

    public DateTime? CheckoutDate { get; set; }

    public string Status { get; set; } = "Booked";
}
}

