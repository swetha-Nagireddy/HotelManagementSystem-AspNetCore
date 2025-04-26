
using HotelMangSys.Models;

namespace HotelMangSys.Services
{
    public class FeedbackService
    {
        private readonly ApplicationDbContext _context;

        public FeedbackService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SubmitFeedbackAsync(Feedback feedback, string userId)
        {
            feedback.UserId = userId;
            feedback.SubmittedOn = DateTime.Now;

            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();
        }
    }
}
