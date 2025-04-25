using AutoMapper;
using HotelMangSys.Models.ViewModels;

namespace HotelMangSys.Models
{
    public class FeedbackProfile :Profile
    {
        public FeedbackProfile()
        {
            CreateMap<Feedback, FeedbackViewModel>()
                .ForMember(dest => dest.RatingStars,
                    opt => opt.MapFrom(src => new string('⭐', src.Rating)))
                .ForMember(dest => dest.DateFormatted,
                    opt => opt.MapFrom(src => src.SubmittedOn.ToString("MMMM dd, yyyy")));
        }
    }
}
