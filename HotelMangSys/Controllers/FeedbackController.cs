using System.Security.Claims;
using AutoMapper;
using HotelMangSys.Models;
using Microsoft.AspNetCore.Mvc;

namespace HotelMangSys.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public FeedbackController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        /// <summary>
        /// GET: Submit feedback form
        /// </summary>
        /// <returns></returns>

        [HttpGet]
        public IActionResult SubmitFeedback()
        {
            return View();
        }

        /// <summary>
        /// POST: Submit feedback form
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> SubmitFeedback(Feedback model)
        {
            if (Request.Cookies.TryGetValue("UserId", out string userId))
            {
                model.UserId = userId;
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }

            model.SubmittedOn = DateTime.Now;

            _context.Feedbacks.Add(model);
            await _context.SaveChangesAsync();

            TempData["FeedbackSuccess"] = "Feedback submitted successfully!";
            return RedirectToAction("SubmitFeedback");
        }
    }
}
