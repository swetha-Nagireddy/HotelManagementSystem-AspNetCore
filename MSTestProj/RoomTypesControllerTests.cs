using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelMangSys.Controllers;
using HotelMangSys.Models;
using HotelMangSys.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Threading;

namespace HotelMangSys.Tests.Controllers
{
    [TestClass]
    public class RoomTypesControllerTests
    {
        private readonly Mock<ApplicationDbContext> _contextMock;
        private readonly Mock<ILogger<RoomTypes>> _loggerMock;
        private readonly RoomTypes _controller;

        public RoomTypesControllerTests()
        {
            // Mock ApplicationDbContext
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
            _contextMock = new Mock<ApplicationDbContext>(options);

            // Mock ILogger<RoomTypes>
            _loggerMock = new Mock<ILogger<RoomTypes>>();

            // Create RoomTypes controller with mocked dependencies
            _controller = new RoomTypes(_contextMock.Object, _loggerMock.Object);

            // Set TempData for success message
            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            _controller.TempData = tempData;
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Dispose the in-memory database
            _contextMock.Object.Dispose();
        }

        [TestMethod]
        public async Task RoomTypesAvailable_ReturnsViewWithRoomList()
        {
            // Arrange
            var searchQuery = "test";
            var page = 1;
            var pageSize = 5;

            // Act
            var result = await _controller.RoomTypesAvailable(searchQuery, page, pageSize);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = result as ViewResult;
            var viewModel = viewResult.Model as RoomListViewModel;
            Assert.IsNotNull(viewModel);
            Assert.AreEqual(2, viewModel.Rooms.Count());
            Assert.AreEqual(1, viewModel.TotalPages);
        }
        [TestMethod]
        public void AddRoom_Get_ReturnsView()
        {
            // Act
            var result = _controller.AddRoom();

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

       

        [TestMethod]
        public async Task AddRoom_Post_WithInvalidData_ReturnsViewWithErrors()
        {
            // Arrange
            var room = new Room { Type = "", Price = 0 };
            _controller.ModelState.AddModelError("Type", "The Type field is required.");

            // Act
            var result = await _controller.AddRoom(room);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = result as ViewResult;
            var model = viewResult.Model as Room;
            Assert.IsNotNull(model);
            Assert.AreEqual("", model.Type);
        }

        

       

        [TestMethod]
        public async Task EditRoom_Post_WithInvalidData_ReturnsViewWithErrors()
        {
            // Arrange
            var room = new Room { Id = 1, Type = "", Price = 0 };
            _controller.ModelState.AddModelError("Type", "The Type field is required.");

            // Act
            var result = await _controller.EditRoom(room.Id, room);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = result as ViewResult;
            var model = viewResult.Model as Room;
            Assert.IsNotNull(model);
            Assert.AreEqual("", model.Type);
        }
       
        [TestMethod]
        public async Task DeleteRoomConfirmed_Post_WithNonExistentRoom_ReturnsNotFound()
        {
            // Arrange
            int nonExistentId = 999;

            // Act
            var result = await _controller.DeleteRoomConfirmed(nonExistentId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirectResult = result as RedirectToActionResult;
            Assert.AreEqual("RoomTypesAvailable", redirectResult.ActionName);
        }

        


    }
}