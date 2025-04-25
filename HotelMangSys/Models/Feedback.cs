namespace HotelMangSys.Models
{
    public class Feedback
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Comment { get; set; }
        public int Rating { get; set; } // 1 to 5
        public DateTime SubmittedOn { get; set; }
    }
}
