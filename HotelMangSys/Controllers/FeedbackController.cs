using HotelMangSys.Models;
using HotelMangSys.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelMangSys.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly FeedbackService _feedbackService;
        private readonly ILogger<AccountController> _logger;

        public FeedbackController(FeedbackService feedbackService, ILogger<AccountController> logger)
        {
            _feedbackService = feedbackService;
            _logger = logger;
        }

        /// <summary>
        /// method to submit feedback
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult SubmitFeedback()
        {
            _logger.LogInformation("Feedback form displayed.");
            return View();
            
        }
        /// <summary>
        /// method to handle the feedback submission
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> SubmitFeedback(Feedback model)
        {
            if (!Request.Cookies.TryGetValue("UserId", out string userId))
            {
                _logger.LogWarning("UserId cookie not found. Redirecting to login.");
                return RedirectToAction("Login", "Account");
                
            }

            await _feedbackService.SubmitFeedbackAsync(model, userId);
            TempData["FeedbackSuccess"] = "Feedback submitted successfully!";
            _logger.LogInformation("Feedback submitted successfully by user {UserId}.", userId);
            return RedirectToAction("SubmitFeedback");
           
        }
    }
}
