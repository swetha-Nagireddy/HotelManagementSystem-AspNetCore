
using HotelMangSys.Models;

namespace HotelMangSys.Services
{
    /// <summary>
    /// class  to handle feedback submission and storage
    /// </summary>
    public class FeedbackService
    {
        private readonly ApplicationDbContext _context;

        public FeedbackService(ApplicationDbContext context)
        {
            _context = context;
        }
        /// <summary>
        ///  method to submit feedback
        /// </summary>
        /// <param name="feedback"></param>
        /// <param name="userId"></param>
        /// <returns></returns>

        public async Task SubmitFeedbackAsync(Feedback feedback, string userId)
        {
            feedback.UserId = userId;
            feedback.SubmittedOn = DateTime.Now;

            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();
        }
    }
}
