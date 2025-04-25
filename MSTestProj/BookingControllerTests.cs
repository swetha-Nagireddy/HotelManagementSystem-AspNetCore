using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using HotelMangSys.Controllers;
using HotelMangSys.Models;
using HotelMangSys.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace HotelMangSys.Tests.Controllers
{
    [TestClass]
    public class BookingControllerTests
    {
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<ILogger<BookingController>> _loggerMock;
        private readonly BookingController _controller;

        public BookingControllerTests()
        {
            // Mock IConfiguration
            _configurationMock = new Mock<IConfiguration>();
            _configurationMock.Setup(c => c["ConnectionStrings:DefaultConnection"]).Returns("Data Source=(localdb)\\mssqllocaldb;Initial Catalog=HotelMangSys;Integrated Security=True");

            // Mock ILogger<BookingController>
            _loggerMock = new Mock<ILogger<BookingController>>();

            // Create BookingController with mocked dependencies
            _controller = new BookingController(_configurationMock.Object, _loggerMock.Object);

            // Set TempData for success message
            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            _controller.TempData = tempData;
        }

        [TestMethod]
        public void Get_CreateBooking_ReturnsView()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var cookies = new Mock<IRequestCookieCollection>();
            cookies.Setup(c => c["UserId"]).Returns("123"); // Mock the cookie value

            // Use a mock for HttpRequest and set it up to return the mocked cookies
            var requestMock = new Mock<HttpRequest>();
            requestMock.Setup(r => r.Cookies).Returns(cookies.Object);

            // Set up the HttpContext to use the mocked HttpRequest
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(h => h.Request).Returns(requestMock.Object);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContextMock.Object
            };

            // Act
            var result = _controller.CreateBooking();

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = result as ViewResult;
            var model = viewResult.Model as Booking;
            Assert.IsNotNull(model);
            Assert.AreEqual("123", model.UserId);
        }

        [TestMethod]
        public void Get_CreateBooking_RedirectsToLoginIfNoUserId()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var cookies = new Mock<IRequestCookieCollection>();
            cookies.Setup(c => c["UserId"]).Returns((string)null); // Mock no cookie value

            // Use a mock for HttpRequest and set it up to return the mocked cookies
            var requestMock = new Mock<HttpRequest>();
            requestMock.Setup(r => r.Cookies).Returns(cookies.Object);

            // Set up the HttpContext to use the mocked HttpRequest
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(h => h.Request).Returns(requestMock.Object);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContextMock.Object
            };

            // Act
            var result = _controller.CreateBooking();

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirectResult = result as RedirectToActionResult;
            Assert.AreEqual("Login", redirectResult.ActionName);
            Assert.AreEqual("Account", redirectResult.ControllerName);
        }

        [TestMethod]
        public async Task Post_CreateBooking_WithValidData_SavesBookingAndRedirects()
        {
            // Arrange
            var booking = new Booking
            {
                UserId = "123",
                RoomType = "Single",
                BookingDate = DateTime.Today,
                CheckoutDate = DateTime.Today.AddDays(1)
            };

            var cookies = new Mock<IRequestCookieCollection>();
            cookies.Setup(c => c["UserId"]).Returns("123"); // Mock the cookie value

            var requestMock = new Mock<HttpRequest>();
            requestMock.Setup(r => r.Cookies).Returns(cookies.Object);

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(h => h.Request).Returns(requestMock.Object);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContextMock.Object
            };

            // Mock IDbConnection
            var dbMock = new Mock<IDbConnection>();
            dbMock.Setup(db => db.QuerySingleOrDefaultAsync<Room>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<IDbTransaction>(),
                It.IsAny<int?>(),
                It.IsAny<CommandType?>()))
                .ReturnsAsync(new Room { Id = 1, Type = "Single", IsAvailable = true });

            dbMock.Setup(db => db.QuerySingleAsync<int>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<IDbTransaction>(),
                It.IsAny<int?>(),
                It.IsAny<CommandType?>()))
                .ReturnsAsync(1);

            dbMock.Setup(db => db.ExecuteAsync(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<IDbTransaction>(),
                It.IsAny<int?>(),
                It.IsAny<CommandType?>()))
                .ReturnsAsync(1);

            // Act
            var result = await _controller.CreateBooking(booking);

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirectResult = result as RedirectToActionResult;
            Assert.AreEqual("BookingConfirmation", redirectResult.ActionName);
        }

        [TestMethod]
        public async Task Post_CreateBooking_WithInvalidData_ReturnsViewWithErrors()
        {
            // Arrange
            var booking = new Booking
            {
                UserId = "123",
                RoomType = "Single",
                BookingDate = DateTime.Today,
                CheckoutDate = DateTime.Today.AddDays(1)
            };

            var cookies = new Mock<IRequestCookieCollection>();
            cookies.Setup(c => c["UserId"]).Returns("123"); // Mock the cookie value

            var requestMock = new Mock<HttpRequest>();
            requestMock.Setup(r => r.Cookies).Returns(cookies.Object);

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(h => h.Request).Returns(requestMock.Object);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContextMock.Object
            };

            // Invalidate the model state
            _controller.ModelState.AddModelError("RoomType", "Invalid room type.");

            // Act
            var result = await _controller.CreateBooking(booking);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = result as ViewResult;
            var model = viewResult.Model as Booking;
            Assert.IsNotNull(model);
            Assert.AreEqual("123", model.UserId);
        }

        [TestMethod]
        public void BookingConfirmation_ReturnsView()
        {
            // Arrange
            var bookingId = 1;

            // Act
            var result = _controller.BookingConfirmation(bookingId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = result as ViewResult;
            Assert.AreEqual(bookingId, _controller.ViewBag.BookingId);
        }

        [TestMethod]
        public async Task BookingHistory_ReturnsViewWithBookings()
        {
            // Arrange
            var userId = "123";
            var cookies = new Mock<IRequestCookieCollection>();
            cookies.Setup(c => c["UserId"]).Returns(userId); // Mock the cookie value

            var requestMock = new Mock<HttpRequest>();
            requestMock.Setup(r => r.Cookies).Returns(cookies.Object);

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(h => h.Request).Returns(requestMock.Object);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContextMock.Object
            };

            // Mock IDbConnection
            var dbMock = new Mock<IDbConnection>();
            dbMock.Setup(db => db.QueryAsync<BookingHistoryDto>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<IDbTransaction>(),
                It.IsAny<int?>(),
                It.IsAny<CommandType?>()))
                .ReturnsAsync(new List<BookingHistoryDto>
                {
                    new BookingHistoryDto { BookingId = 1, UserId = userId, RoomType = "Single", BookingDate = DateTime.Today, CheckoutDate = DateTime.Today.AddDays(1), Status = "Confirmed" }
                });

            dbMock.Setup(db => db.QuerySingleAsync<int>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<IDbTransaction>(),
                It.IsAny<int?>(),
                It.IsAny<CommandType?>()))
                .ReturnsAsync(1);

            // Act
            var result = await _controller.BookingHistory();

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = result as ViewResult;
            var viewModel = viewResult.Model as BookingHistoryViewModel;
            Assert.IsNotNull(viewModel);
            Assert.AreEqual(1, viewModel.Bookings.Count);
            Assert.AreEqual(1, viewModel.TotalPages);
        }
    }
}