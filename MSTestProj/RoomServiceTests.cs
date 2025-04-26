using HotelMangSys.Models;
using HotelMangSys.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace HotelMangSys.Tests.Services
{
    [TestClass]
    public class RoomServiceTests
    {
        private Mock<IConfiguration> _configurationMock;
        private Mock<ILogger<RoomService>> _loggerMock;
        private Mock<IDapperWrapper> _dapperWrapperMock;
        private RoomService _roomService;
        private const string ConnectionString = "Server=.;Database=HotelTestDb;Trusted_Connection=True;";


        [TestInitialize]
        public void Setup()
        {
            _configurationMock = new Mock<IConfiguration>();
            _loggerMock = new Mock<ILogger<RoomService>>();
            _dapperWrapperMock = new Mock<IDapperWrapper>();

            _configurationMock.Setup(c => c["ConnectionStrings:DefaultConnection"])
            .Returns(ConnectionString);


            _roomService = new RoomService(
                _configurationMock.Object,
                _loggerMock.Object,
                _dapperWrapperMock.Object
            );
        }

        [TestMethod]
        public async Task GetAllRoomsAsync_ReturnsRoomList()
        {
            // Arrange
            var expectedRooms = new List<Room>
            {
                new Room { Id = 1, Type = "Single", Price = 100, IsAvailable = true },
                new Room { Id = 2, Type = "Double", Price = 200, IsAvailable = false }
            };

            _dapperWrapperMock.Setup(d => d.QueryAsync<Room>(It.IsAny<IDbConnection>(), It.IsAny<string>(), null, null))
                              .ReturnsAsync(expectedRooms);

            // Act
            var result = await _roomService.GetAllRoomsAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("Single", result[0].Type);
            Assert.AreEqual("Double", result[1].Type);
        }

        [TestMethod]
        public async Task SearchRoomsAsync_WithQuery_ReturnsMatchingRooms()
        {
            // Arrange
            var searchQuery = "sin"; // user types 'sin'
            var expectedRooms = new List<Room>
            {
                new Room { Id = 1, Type = "Single", Price = 100, IsAvailable = true }
            };

            _dapperWrapperMock.Setup(d => d.QueryAsync<Room>(It.IsAny<IDbConnection>(), It.IsAny<string>(), It.IsAny<object>(), null))
                              .ReturnsAsync(expectedRooms);

            // Act
            var result = await _roomService.SearchRoomsAsync(searchQuery);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Single", result[0].Type);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task GetAllRoomsAsync_ThrowsException_WhenConnectionStringMissing()
        {
            // Arrange
            _configurationMock.Setup(c => c["ConnectionStrings:DefaultConnection"]).Returns<string>(null);

            var service = new RoomService(_configurationMock.Object, Mock.Of<ILogger<RoomService>>(), _dapperWrapperMock.Object);

            // Act
            await service.GetAllRoomsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task SearchRoomsAsync_ThrowsException_WhenConnectionStringMissing()
        {
            // Arrange
            _configurationMock.Setup(c => c["ConnectionStrings:DefaultConnection"]).Returns<string>(null);

            var service = new RoomService(_configurationMock.Object, Mock.Of<ILogger<RoomService>>(), _dapperWrapperMock.Object);

            // Act
            await service.SearchRoomsAsync("sin");
        }

        [TestMethod]
        public async Task AddRoomAsync_CallsExecuteAsync()
        {
            // Arrange
            var room = new Room { Id = 3, Type = "Suite", Price = 300, IsAvailable = true };

            _dapperWrapperMock
                .Setup(x => x.ExecuteAsync(
                    It.IsAny<IDbConnection>(),
                    It.Is<string>(q => q.StartsWith("INSERT INTO Rooms")),
                    It.IsAny<object>(),
                    null))
                .ReturnsAsync(1);

            // Act
            await _roomService.AddRoomAsync(room);

            // Assert
            _dapperWrapperMock.Verify(x => x.ExecuteAsync(
                It.IsAny<IDbConnection>(),
                It.Is<string>(q => q.StartsWith("INSERT INTO Rooms")),
                It.IsAny<object>(),
                null), Times.Once);
        }

        [TestMethod]
        public async Task GetRoomByIdAsync_ReturnsRoom()
        {
            // Arrange
            var expectedRoom = new Room { Id = 4, Type = "Family", Price = 400, IsAvailable = true };

            _dapperWrapperMock
                .Setup(x => x.QuerySingleAsync<Room>(
                    It.IsAny<IDbConnection>(),
                    "SELECT * FROM Rooms WHERE Id = @Id",
                    It.IsAny<object>(),
                    null))
                .ReturnsAsync(expectedRoom);

            // Act
            var room = await _roomService.GetRoomByIdAsync(4);

            // Assert
            Assert.IsNotNull(room);
            Assert.AreEqual(4, room.Id);
        }

        [TestMethod]
        public async Task UpdateRoomAsync_CallsExecuteAsync()
        {
            // Arrange
            var room = new Room { Id = 5, Type = "King", Price = 500, IsAvailable = false };

            _dapperWrapperMock
                .Setup(x => x.ExecuteAsync(
                    It.IsAny<IDbConnection>(),
                    It.Is<string>(q => q.StartsWith("UPDATE Rooms")),
                    It.IsAny<object>(),
                    null))
                .ReturnsAsync(1);

            // Act
            await _roomService.UpdateRoomAsync(room);

            // Assert
            _dapperWrapperMock.Verify(x => x.ExecuteAsync(
                It.IsAny<IDbConnection>(),
                It.Is<string>(q => q.StartsWith("UPDATE Rooms")),
                It.IsAny<object>(),
                null), Times.Once);
        }

        [TestMethod]
        public async Task DeleteRoomAsync_CallsExecuteAsync()
        {
            // Arrange
            _dapperWrapperMock
                .Setup(x => x.ExecuteAsync(
                    It.IsAny<IDbConnection>(),
                    It.Is<string>(q => q.StartsWith("DELETE FROM Rooms")),
                    It.IsAny<object>(),
                    null))
                .ReturnsAsync(1);

            // Act
            await _roomService.DeleteRoomAsync(5);

            // Assert
            _dapperWrapperMock.Verify(x => x.ExecuteAsync(
                It.IsAny<IDbConnection>(),
                It.Is<string>(q => q.StartsWith("DELETE FROM Rooms")),
                It.IsAny<object>(),
                null), Times.Once);
        }
    }
}
