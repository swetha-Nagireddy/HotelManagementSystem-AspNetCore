namespace HotelMangSys.Models
{
    public class Room
    {
        /// This class represents a room in the hotel management system.
        public int Id { get; set; }
        public string Type { get; set; }
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
    }
}
