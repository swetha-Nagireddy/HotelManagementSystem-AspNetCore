using HotelMangSys.Models;
using HotelMangSys.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using HotelMangSys.Models.ViewModels;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;
using HotelMangSys.Exceptions;

namespace HotelMangSys.Controllers
{
   
    public class AccountController : Controller
    {

        private readonly ILogger<AccountController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JwtTokenService _jwtTokenService;
        private readonly ApplicationDbContext _context;

        public AccountController(UserManager<ApplicationUser> userManager,
                                  SignInManager<ApplicationUser> signInManager,
                                  JwtTokenService jwtTokenService,
                                  ILogger<AccountController> logger,
                                  ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtTokenService = jwtTokenService;
            _logger = logger;
            _context = context;
        }

        // Register Page
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }


        /// <summary>
        /// Register a new user and save data to the database.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var existingUserByUsername = await _userManager.FindByNameAsync(model.Username);
                    if (existingUserByUsername != null) //checks username presented or not
                    {
                        throw new UserAlreadyExistsException("Username is already taken.");
                    }

                    var existingUserByEmail = await _userManager.FindByEmailAsync(model.Email);
                    if (existingUserByEmail != null)
                    {
                        throw new UserAlreadyExistsException("Email is already registered.");
                    }

                    var user = new ApplicationUser
                    {
                        UserName = model.Username,
                        Email = model.Email,
                        FullName = model.FullName,
                        Address = model.Address,
                        PhoneNumber = model.PhoneNumber
                    };

                    var result = await _userManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        TempData["Success"] = "User registered successfully!";
                        return RedirectToAction("Register");
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
                catch (UserAlreadyExistsException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            return View(model);
        }


        /// <summary>
        /// Login Page verifies user credentials and generates a JWT token.
        /// </summary>
        /// <returns></returns>

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)//check validations
            {
                var user = await _userManager.FindByNameAsync(model.Username);

                if (user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);

                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User logged in.");

                        // Save the UserId in a cookie
                        var userId = user.Id;
                        var cookieOptions = new CookieOptions
                        {
                            HttpOnly = true, // Makes the cookie accessible only through HTTP requests
                            Secure = true,   // Ensures the cookie is sent only over HTTPS
                            SameSite = SameSiteMode.Strict, // Helps mitigate CSRF attacks
                            Expires = DateTime.Now.AddDays(7) // Cookie expires in 7 days
                        };
                        _logger.LogInformation("Setting cookie for UserId: {UserId}", userId);
                        Response.Cookies.Append("UserId", userId, cookieOptions);

                        var token = _jwtTokenService.GenerateJwtToken(user.UserName);
                        _logger.LogInformation("Generated JWT token for user: {Username}", user.UserName);
                        return Json(new { token }); //  JSON response
                    }
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
            _logger.LogWarning("Invalid login attempt.");

            return BadRequest("Invalid login.");
        }

        /// <summary>
        /// GET: User details page.
        /// </summary>
        /// <returns></returns>
       
        public async Task<IActionResult> UserDetails()
        {
            if (Request.Cookies.TryGetValue("UserId", out var userId))
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    _logger.LogInformation("User details retrieved for UserId: {UserId}", userId);
                    // Pass the user directly to the view
                    return View(user);
                }
            }
            _logger.LogWarning("User not found for UserId: {UserId}", Request.Cookies["UserId"]);
            // Redirect to login if user not found or not logged in
            return RedirectToAction("Login", "Account");
        }

        /// <summary>
        /// GET: Update user profile page.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> UpdateProfile()
        {
            if (Request.Cookies.TryGetValue("UserId", out var userId))
            {
                _logger.LogInformation("Fetching user profile for UserId: {UserId}", userId);
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    _logger.LogInformation("User profile found for UserId: {UserId}", userId);
                    return View(user);
                }
            }
            _logger.LogWarning("User not found for UserId: {UserId}", Request.Cookies["UserId"]);
            return RedirectToAction("Login", "Account");
        }

        /// <summary>
        /// POST: Update user profile.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(ApplicationUser model)
        {
            // Check if the user is logged in
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user != null)// check if user is null
                {
                    // Update user properties
                    _logger.LogInformation("Updating profile for UserId: {UserId}", model.Id);
                    user.FullName = model.FullName;
                    user.Address = model.Address;
                    user.Email = model.Email;
                    user.PhoneNumber = model.PhoneNumber;
                    var result = await _userManager.UpdateAsync(user);

                    if (result.Succeeded)
                    {
                        _logger.LogInformation("Profile updated successfully for UserId: {UserId}", model.Id);
                        TempData["SuccessMessage"] = "Profile updated successfully!";
                        return RedirectToAction("UpdateProfile");
                    }
                    foreach (var error in result.Errors)
                    {
                        _logger.LogError(error.Description);
                        ModelState.AddModelError("", error.Description);
                    }
                }
                {
                    _logger.LogInformation("Updating profile for UserId: {UserId}", model.Id);
                    user.FullName = model.FullName;
                    user.Address = model.Address;
                    user.Email = model.Email;
                    user.PhoneNumber = model.PhoneNumber;

                    var result = await _userManager.UpdateAsync(user);

                    if (result.Succeeded)
                    {
                        _logger.LogInformation("Profile updated successfully for UserId: {UserId}", model.Id);
                        TempData["SuccessMessage"] = "Profile updated successfully!";
                        return RedirectToAction("UpdateProfile");
                    }

                    foreach (var error in result.Errors)
                    {
                        _logger.LogError(error.Description);
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }

            return View(model);
        }
        /// <summary>
        /// Logout the user and redirect to login page.
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            // 🧹 Remove the UserId cookie
            Response.Cookies.Delete("UserId");
            _logger.LogInformation("User logged out.");
            return RedirectToAction("Login");
        }



    }
}
