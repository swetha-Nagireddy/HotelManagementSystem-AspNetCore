using HotelMangSys.Models;
using HotelMangSys.Models.ViewModels;
using HotelMangSys.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace HotelMangSys.Tests.Services
{
    [TestClass]
    public class BookingServiceTests
    {
        private Mock<IConfiguration> _mockConfig;
        private Mock<IDapperWrapper> _mockDapper;
        private BookingService _bookingService;
        private const string ConnectionString = "Server=.;Database=HotelTestDb;Trusted_Connection=True;";

        [TestInitialize]
        public void Setup()
        {
            _mockConfig = new Mock<IConfiguration>();
            _mockDapper = new Mock<IDapperWrapper>();

            _mockConfig.Setup(c => c["ConnectionStrings:DefaultConnection"])
             .Returns(ConnectionString);

            _bookingService = new BookingService(_mockConfig.Object, _mockDapper.Object);
        }

        [TestMethod]
        public async Task GetAvailableRoomAsync_ReturnsRoom_WhenAvailable()
        {
            // Arrange
            var roomType = "Deluxe";
            var expectedRoom = new Room { Id = 1, Type = roomType, IsAvailable = true };

            _mockDapper.Setup(d => d.QueryAsync<Room>(
                It.IsAny<IDbConnection>(),
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<IDbTransaction>()
            )).ReturnsAsync(new List<Room> { expectedRoom });

            // Act
            var result = await _bookingService.GetAvailableRoomAsync(roomType);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedRoom.Type, result.Type);
        }

        [TestMethod]
        public async Task CreateBookingAsync_ReturnsBookingId_WhenSuccessful()
        {
            // Arrange
            var booking = new Booking
            {
                UserId = "user1",
                RoomId = 1,
                RoomType = "Standard",
                BookingDate = DateTime.Now,
                CheckoutDate = DateTime.Now.AddDays(2),
                Status = "Confirmed"
            };

            _mockDapper.Setup(d => d.QuerySingleAsync<int>(
                It.IsAny<IDbConnection>(),
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<IDbTransaction>()
            )).ReturnsAsync(101);

            _mockDapper.Setup(d => d.ExecuteAsync(
                It.IsAny<IDbConnection>(),
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<IDbTransaction>()
            )).ReturnsAsync(1);

            // Act
            var bookingId = await _bookingService.CreateBookingAsync(booking);

            // Assert
            Assert.AreEqual(101, bookingId);
        }

        [TestMethod]
        public async Task GetBookingHistoryAsync_ReturnsBookingList()
        {
            // Arrange
            var userId = "user1";
            int offset = 0;
            int pageSize = 5;

            var history = new List<BookingHistoryDto>
            {
                new BookingHistoryDto
                {
                    BookingId = 1,
                    UserId = userId,
                    RoomId = 10,
                    RoomType = "Deluxe",
                    BookingDate = DateTime.Today.AddDays(-1),
                    CheckoutDate = DateTime.Today,
                    Status = "Completed"
                }
            };

            _mockDapper.Setup(d => d.QueryAsync<BookingHistoryDto>(
                It.IsAny<IDbConnection>(),
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<IDbTransaction>()
            )).ReturnsAsync(history);

            // Act
            var result = await _bookingService.GetBookingHistoryAsync(userId, offset, pageSize);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, ((List<BookingHistoryDto>)result).Count);
        }

        [TestMethod]
        public async Task GetBookingCountAsync_ReturnsCount()
        {
            // Arrange
            var userId = "user1";
            _mockDapper.Setup(d => d.QuerySingleAsync<int>(
                It.IsAny<IDbConnection>(),
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<IDbTransaction>()
            )).ReturnsAsync(5);

            // Act
            var result = await _bookingService.GetBookingCountAsync(userId);

            // Assert
            Assert.AreEqual(5, result);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task CreateBookingAsync_ThrowsException_WhenInsertFails()
        {
            // Arrange
            var booking = new Booking
            {
                UserId = "user1",
                RoomId = 1,
                RoomType = "Standard",
                BookingDate = DateTime.Now,
                CheckoutDate = DateTime.Now.AddDays(2),
                Status = "Confirmed"
            };

            _mockDapper.Setup(d => d.QuerySingleAsync<int>(
                It.IsAny<IDbConnection>(),
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<IDbTransaction>()
            )).ThrowsAsync(new Exception("DB failure"));

            // Act
            await _bookingService.CreateBookingAsync(booking);

            // Assert: handled by ExpectedException
        }
    }
}
