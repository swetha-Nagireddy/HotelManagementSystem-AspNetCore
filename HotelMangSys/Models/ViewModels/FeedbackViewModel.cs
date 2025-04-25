namespace HotelMangSys.Models.ViewModels
{
    public class FeedbackViewModel
    {
        public string UserId { get; set; }
        public string Comment { get; set; }
        public string RatingStars { get; set; } // "⭐⭐⭐⭐"
        public string DateFormatted { get; set; }
    }
}
