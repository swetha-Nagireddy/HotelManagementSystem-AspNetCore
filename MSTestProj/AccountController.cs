using HotelMangSys.Controllers;
using HotelMangSys.Models.ViewModels;
using HotelMangSys.Models;
using HotelMangSys.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;


using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging; // Add this using directive to resolve 'IConfiguration'
[TestClass]
public class AccountControllerTests
{
    private Mock<UserManager<ApplicationUser>> _userManagerMock;
    private Mock<SignInManager<ApplicationUser>> _signInManagerMock;
    private Mock<JwtTokenService> _jwtTokenServiceMock;
    private Mock<ILogger<AccountController>> _loggerMock;
    private Mock<ApplicationDbContext> _dbContextMock;

    private AccountController _controller;

    [TestInitialize]
    public void Setup()
    {
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);
        _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
            _userManagerMock.Object, Mock.Of<IHttpContextAccessor>(), Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(), null, null, null, null);
        _jwtTokenServiceMock = new Mock<JwtTokenService>();
        _loggerMock = new Mock<ILogger<AccountController>>();
        _dbContextMock = new Mock<ApplicationDbContext>();

        _controller = new AccountController(
            _userManagerMock.Object,
            _signInManagerMock.Object,
            _jwtTokenServiceMock.Object,
            _loggerMock.Object,
            _dbContextMock.Object);
    }

    [TestMethod]
    public async Task Register_Post_ValidUser_RedirectsToRegister()
    {
        // Arrange
        var model = new RegisterViewModel
        {
            Username = "newuser",
            Email = "newuser@example.com",
            FullName = "Test User",
            Address = "123 Main St",
            PhoneNumber = "1234567890",
            Password = "Test@1234"
        };

        _userManagerMock.Setup(u => u.FindByNameAsync(model.Username)).ReturnsAsync((ApplicationUser?)null);
        _userManagerMock.Setup(u => u.FindByEmailAsync(model.Email)).ReturnsAsync((ApplicationUser?)null);
        _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), model.Password))
                        .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _controller.Register(model) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Register", result.ActionName);
        Assert.AreEqual("User registered successfully!", _controller.TempData["Success"]);
    }
}
