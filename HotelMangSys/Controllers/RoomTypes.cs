using HotelMangSys.Models;
using HotelMangSys.Models.ViewModels;
using HotelMangSys.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelMangSys.Controllers
{
    public class RoomTypesController : Controller
    {
        private readonly IRoomService _roomService;
        private readonly ILogger<RoomTypesController> _logger;

        public RoomTypesController(IRoomService roomService, ILogger<RoomTypesController> logger)
        {
            _roomService = roomService;
            _logger = logger;
        }

        /// <summary>
        /// method to get all the room types available in the hotel
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetPagedRooms(int pageNumber = 1)
        {
            var allRooms = await _roomService.GetAllRoomsAsync(); // Fetch all once
            _logger.LogInformation("Fetched all rooms for pagination.");
            int pageSize = 5;

            var roomsToShow = allRooms
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var model = new RoomListViewModel
            {
                Rooms = roomsToShow,
                PageNumber = pageNumber,
                TotalPages = (int)Math.Ceiling(allRooms.Count / (double)pageSize)
            };

            return View(model);
        }



        /// <summary>
        /// method for autocomplete room search
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet("/api/room/autocomplete")]
        public async Task<IActionResult> AutoComplete(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Ok(new List<Room>());
            _logger.LogInformation("Searching for rooms with query: {Query}", query);

            var rooms = await _roomService.SearchRoomsAsync(query);
            _logger.LogInformation("Found {Count} rooms matching query: {Query}", rooms.Count, query);
            return Ok(rooms);
        }

        /// <summary>
        ///  method to filter rooms by type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpGet("/Rooms/FilterByRoomType")]
        public async Task<IActionResult> FilterByRoomType(string type)
        {
            var rooms = await _roomService.SearchRoomsAsync(type);
            _logger.LogInformation("Filtered rooms by type: {Type}", type);

            var model = new RoomListViewModel
            {
                Rooms = rooms,
                PageNumber = 1 // Search result page is 1
            };

            return View("GetpagedRooms", model); // reuse same view
        }

        /// <summary>
        /// method to add a new room
        /// </summary>
        /// <returns></returns>
        public IActionResult AddRoom() => View();
        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRoom(Room room)
        {
            if (!ModelState.IsValid)
                return View(room);
            _logger.LogInformation("Adding new room: {Room}", room);

            await _roomService.AddRoomAsync(room);
            TempData["SuccessMessage"] = "Room added successfully.";
            return RedirectToAction(nameof(GetPagedRooms));
            _logger.LogInformation("Room added successfully: {Room}", room);
        }

        /// <summary>
        /// /// method to edit a room
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> EditRoom(int id)
        {
            var room = await _roomService.GetRoomByIdAsync(id);
            _logger.LogInformation("Editing room with ID: {Id}", id);
            if (room == null)
                return NotFound();
            _logger.LogInformation("Room fetched for editing: {Room}", room);

            return View(room);
            
        }

        /// <summary>
        /// /// method to update a room
        /// </summary>
        /// <param name="id"></param>
        /// <param name="room"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRoom(int id, Room room)
        {
            //if (id != room.Id)
            //    _logger.LogWarning("Room ID mismatch: {Id} != {RoomId}", id, room.Id);
            //return NotFound();

            //if (!ModelState.IsValid)
            //    _logger.LogWarning("Invalid room model: {ModelState}", ModelState);
            //return View(room);

            await _roomService.UpdateRoomAsync(room);
            _logger.LogInformation("Room updated successfully: {Room}", room);
            TempData["SuccessMessage"] = "Room updated successfully.";
            return RedirectToAction(nameof(GetPagedRooms));
            
        }

        /// <summary>
        /// /// method to delete a room
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> DeleteRoom(int id)
        {
            var room = await _roomService.GetRoomByIdAsync(id);
            _logger.LogInformation("Deleting room with ID: {Id}", id);
            if (room == null)
            _logger.LogWarning("Room not found for deletion: {Id}", id);
            return NotFound();

            return View(room);
        }

        /// <summary>
        /// /// method to confirm the deletion of a room
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost, ActionName("DeleteRoom")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles="Admin")]
        public async Task<IActionResult> DeleteRoomConfirmed(int id)
        {
            try
            {
                await _roomService.DeleteRoomAsync(id);
                _logger.LogInformation("Room deleted successfully: {Id}", id);
                TempData["SuccessMessage"] = "Room deleted successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting room with ID {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the room.";
            }

            return RedirectToAction(nameof(GetPagedRooms));
        }

        /// <summary>
        /// /// method to handle unauthorized access
        /// </summary>
        /// <returns></returns>

        public IActionResult AccessDenied()
        {
            TempData["ErrorMessage"] = "You are not authorized to access this page.";
            _logger.LogWarning("Access denied for user: {User}", User.Identity.Name);
            return View();
        }
    }
}