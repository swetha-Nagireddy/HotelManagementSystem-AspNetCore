using AspNetCoreGeneratedDocument;

using HotelMangSys.Models;
using HotelMangSys.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelMangSys.Controllers
{
    /// <summary>
    /// Controller for managing room types.
    /// </summary>
    public class RoomTypes : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RoomTypes> _logger;

        public RoomTypes(ApplicationDbContext context,ILogger<RoomTypes> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Fetches available room types from the database.
        /// </summary>
        /// <param name="searchQuery"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>

        public async Task<IActionResult> RoomTypesAvailable(string searchQuery, int page = 1, int pageSize = 5)
        {
            IQueryable<Room> rooms = _context.Rooms.AsNoTracking(); // Use AsNoTracking for better performance

            if (!string.IsNullOrEmpty(searchQuery))
            {
                _logger.LogInformation("Searching for room types with query: {SearchQuery}", searchQuery);
                // Use DbSet's FromSqlInterpolated instead of FromSqlRaw for better safety and readability
                rooms = _context.Rooms.FromSqlInterpolated($"SELECT * FROM Rooms WHERE LOWER(Type) LIKE LOWER({searchQuery} + '%') OR Price LIKE {searchQuery} + '%'");
            }

            // Calculate total count and total pages
            int totalCount = await rooms.CountAsync();
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Apply pagination
            var roomList = await rooms.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            // Create the view model
            var viewModel = new RoomListViewModel
            {
                Rooms = roomList,
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalCount = totalCount,
                SearchQuery = searchQuery // Ensure search query is passed to the view
            };

            _logger.LogInformation("Fetched room types from the database.");
            return View(viewModel);
        }

        /// <summary>
        /// GET: Add Room
        /// </summary>
        /// <returns></returns>
        public IActionResult AddRoom()
        {
            return View();
        }

        // POST: Add Room
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRoom(Room room)
        {
            if (ModelState.IsValid)
            {
                // Add the room to the database
                _context.Rooms.Add(room);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Added a new room type to the database.");
                

                return RedirectToAction(nameof(RoomTypesAvailable));  // Redirect to Room Types after adding
            }
            _logger.LogWarning("Failed to add a new room type. Model state is invalid.");
            return View(room);  // If the form is invalid, return the same view
        }


        //updte
        // GET: Edit Room
        //[Authorize(Roles ="Admin")]
        public async Task<IActionResult> EditRoom(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("EditRoom called with null ID.");
                return NotFound();
            }

            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }
            return View(room);
        }

        // POST: Edit Room
        //[Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRoom(int id, Room room)
        {
            
            if (id != room.Id)
            {
                _logger.LogWarning("EditRoom called with mismatched ID. Expected: {ExpectedId}, Actual: {ActualId}", id, room.Id);
                return NotFound();
            }


            if (ModelState.IsValid)
            {
                try
                {
                    _logger.LogInformation("Updating room with ID {Id}.", id);
                    _context.Update(room);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Room with ID {Id} updated successfully.", id);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Rooms.Any(r => r.Id == id))
                    {
                        _logger.LogWarning("Room with ID {Id} not found for update.", id);
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(RoomTypesAvailable));
            }
            return View(room);
        }

       

        // GET: Delete Room
        public async Task<IActionResult> DeleteRoom(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }
            return View(room);
        }



        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("DeleteRoom")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRoomConfirmed(int id)
        {
            //// Check if the current user is in the Admin role
            //if (!User.IsInRole("Admin"))
            //{
            //    TempData["ErrorMessage"] = "You are not authorized to delete a room.";
            //    _logger.LogWarning("Unauthorized delete attempt by user: {User}", User.Identity.Name);
            //    return RedirectToAction("RoomTypesAvailable");
            //}

            try
            {
                var room = await _context.Rooms.FindAsync(id);
                if (room == null)
                {
                    _logger.LogWarning("Attempted to delete room with ID {Id}, but it was not found.", id);
                    return NotFound();
                }

                _context.Rooms.Remove(room);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Deleted room with ID {Id} successfully.", id);
                TempData["SuccessMessage"] = "Room deleted successfully.";
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Error occurred while deleting room with ID {Id}.", id);
                TempData["ErrorMessage"] = "Unable to delete the room. It may be linked to a booking.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while deleting room with ID {Id}.", id);
                TempData["ErrorMessage"] = "An unexpected error occurred.";
                _logger.LogError(ex, "Unexpected error occurred while deleting room with ID {Id}.", id);
            }

            return RedirectToAction("RoomTypesAvailable");
        }

        public IActionResult AccessDenied()
        {
            TempData["ErrorMessage"] = "You are not authorized to access this page.";
            return View();
        }

    }
}
