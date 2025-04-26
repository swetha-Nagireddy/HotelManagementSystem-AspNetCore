using HotelMangSys.Models;
using HotelMangSys.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace HotelMangSys.Tests.Services
{
    [TestClass]
    public class FeedbackServiceTests
    {
        private ApplicationDbContext _context;
        private FeedbackService _service;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "FeedbackTestDb")
                .Options;

            _context = new ApplicationDbContext(options);
            _service = new FeedbackService(_context);
        }

        [TestMethod]
        public async Task SubmitFeedbackAsync_ShouldAddFeedbackWithUserIdAndDate()
        {
            // Arrange
            var feedback = new Feedback
            {
                Comment = "This is a test feedback",
                Rating = 5
            };
            string userId = "test-user";

            // Act
            await _service.SubmitFeedbackAsync(feedback, userId);

            // Assert
            var savedFeedback = await _context.Feedbacks.FirstOrDefaultAsync(f => f.UserId == userId);
            Assert.IsNotNull(savedFeedback);
            Assert.AreEqual("test-user", savedFeedback.UserId);
            Assert.AreEqual("This is a test feedback", savedFeedback.Comment);
            Assert.IsTrue(savedFeedback.SubmittedOn <= DateTime.Now);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
