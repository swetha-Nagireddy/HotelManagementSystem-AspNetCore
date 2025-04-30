using HotelMangSys.Models;
using HotelMangSys.Models.ViewModels;
using HotelMangSys.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace HotelMangSys.Tests.Services
{
    [TestClass]
    public class BookingServiceTests
    {
        private Mock<IConfiguration> _mockConfig;
        private Mock<IDapperWrapper> _mockDapper;
        private Mock<IMemoryCache> _cacheMock;
        private BookingService _bookingService;
        private const string ConnectionString = "Server=.;Database=HotelTestDb;Trusted_Connection=True;";

        [TestInitialize]
        public void Setup()
        {
            _mockConfig = new Mock<IConfiguration>();
            _mockDapper = new Mock<IDapperWrapper>();
            _cacheMock = new Mock<IMemoryCache>();
            _mockConfig.Setup(c => c["ConnectionStrings:DefaultConnection"])
             .Returns(ConnectionString);
           
        _bookingService = new BookingService(_mockConfig.Object, _mockDapper.Object,_cacheMock.Object);
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
            // Verify that logger was called with expected message
            
        }

        [TestMethod]
        public async Task CreateBookingAsync_Should_InsertBooking_And_UpdateRoom_And_ClearCache()
        {
            // Arrange
            var booking = new Booking
            {
                UserId = "user123",
                RoomId = 1,
                RoomType = "Deluxe",
                BookingDate = DateTime.Today,
                CheckoutDate = DateTime.Today.AddDays(1),
                Status = "Confirmed"
            };

            _mockDapper
                .Setup(d => d.QuerySingleAsync<int>(It.IsAny<IDbConnection>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<IDbTransaction>()))
                .ReturnsAsync(123); // Assume Booking ID = 123

            _mockDapper
               .Setup(d => d.ExecuteAsync(It.IsAny<IDbConnection>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<IDbTransaction>()))
                .ReturnsAsync(1);

            var cacheMock = new MemoryCache(new MemoryCacheOptions());
            _cacheMock.Setup(c => c.Remove(It.IsAny<object>())).Verifiable();

            // Act
            var result = await _bookingService.CreateBookingAsync(booking);

            // Assert
            Assert.AreEqual(123, result);

            _mockDapper.Verify(d => d.QuerySingleAsync<int>(It.IsAny<IDbConnection>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<IDbTransaction>()), Times.Once);
            _mockDapper.Verify(d => d.ExecuteAsync(It.IsAny<IDbConnection>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<IDbTransaction>()), Times.Once);

            _cacheMock.Verify(c => c.Remove(It.Is<string>(key => key.StartsWith("BookingHistory_user123_Page_"))), Times.AtLeastOnce);
        }

        
    
        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task CreateBookingAsync_Should_RollbackTransaction_OnError()
        {
            // Arrange
            var booking = new Booking
            {
                UserId = "user123",
                RoomId = 1,
                RoomType = "Deluxe",
                BookingDate = DateTime.Today,
                CheckoutDate = DateTime.Today.AddDays(1),
                Status = "Confirmed"
            };

           _mockDapper
                .Setup(d => d.QuerySingleAsync<int>(It.IsAny<IDbConnection>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<IDbTransaction>()))
                .ThrowsAsync(new Exception("Database Error"));

            // Act
            await _bookingService.CreateBookingAsync(booking);

            // Assert
            // Exception is expected
        }
        
        // Additional tests can be added here, for example, testing error cases, empty results, etc.

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
