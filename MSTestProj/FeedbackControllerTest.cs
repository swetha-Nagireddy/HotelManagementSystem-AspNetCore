using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using HotelMangSys.Controllers;
using HotelMangSys.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HotelMangSys.Tests.Controllers
{
    [TestClass]
    public class FeedbackControllerTests
    {
        private readonly Mock<ApplicationDbContext> _contextMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly FeedbackController _controller;

        public FeedbackControllerTests()
        {
            // Create a valid DbContextOptions<ApplicationDbContext> instance
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            // Mock ApplicationDbContext with valid DbContextOptions<ApplicationDbContext>
            _contextMock = new Mock<ApplicationDbContext>(options);

            // Mock IMapper
            _mapperMock = new Mock<IMapper>();

            // Create FeedbackController with mocked dependencies
            _controller = new FeedbackController(_contextMock.Object, _mapperMock.Object);

            // Set TempData for success message
            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            _controller.TempData = tempData;
        }

        [TestMethod]
        public void Get_SubmitFeedback_ReturnsView()
        {
            // Act
            var result = _controller.SubmitFeedback();

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = result as ViewResult;
            Assert.IsNull(viewResult.ViewName); // Default view name
        }

        

        [TestMethod]
        public async Task Post_SubmitFeedback_WithoutUserIdInCookies_RedirectsToLogin()
        {
            // Arrange
            var feedback = new Feedback
            {
                UserId = "123",
                Comment = "Test feedback",
                Rating = 5
            };

            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = await _controller.SubmitFeedback(feedback);

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirectResult = result as RedirectToActionResult;
            Assert.AreEqual("Login", redirectResult.ActionName);
            Assert.AreEqual("Account", redirectResult.ControllerName);
        }
    }
}