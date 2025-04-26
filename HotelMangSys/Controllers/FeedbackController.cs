using HotelMangSys.Models;
using HotelMangSys.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelMangSys.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly FeedbackService _feedbackService;

        public FeedbackController(FeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        [HttpGet]
        public IActionResult SubmitFeedback()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SubmitFeedback(Feedback model)
        {
            if (!Request.Cookies.TryGetValue("UserId", out string userId))
            {
                return RedirectToAction("Login", "Account");
            }

            await _feedbackService.SubmitFeedbackAsync(model, userId);
            TempData["FeedbackSuccess"] = "Feedback submitted successfully!";
            return RedirectToAction("SubmitFeedback");
        }
    }
}
