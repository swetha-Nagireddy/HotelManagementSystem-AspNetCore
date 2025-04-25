using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;
using HotelMangSys.Controllers;
using HotelMangSys.Models;
using HotelMangSys.Data;
using HotelMangSys.Services; // Ensure this namespace is correct and contains JwtTokenService

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
        // UserManager mock
        var userStore = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStore.Object, null, null, null, null, null, null, null, null
        );

        // SignInManager mock
        _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
            _userManagerMock.Object,
            new Mock<IHttpContextAccessor>().Object,
            new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>().Object,
            null, null, null, null
        );

        // Other dependencies
        _jwtTokenServiceMock = new Mock<JwtTokenService>();
        _loggerMock = new Mock<ILogger<AccountController>>();
        _dbContextMock = new Mock<ApplicationDbContext>();

        // Create controller instance
        _controller = new AccountController(
            _userManagerMock.Object,
            _signInManagerMock.Object,
            _jwtTokenServiceMock.Object,
            _loggerMock.Object,
            _dbContextMock.Object
        );

        // Set TempData
        var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
        _controller.TempData = tempData;
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

        _userManagerMock.Setup(u => u.FindByNameAsync(model.Username)).ReturnsAsync((ApplicationUser)null);
        _userManagerMock.Setup(u => u.FindByEmailAsync(model.Email)).ReturnsAsync((ApplicationUser)null);
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
