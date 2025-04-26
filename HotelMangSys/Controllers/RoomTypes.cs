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

        // GET: /Rooms
        // Main Page
        // Show Rooms with Pagination
        // Show Rooms with Pagination
        [HttpGet]
        public async Task<IActionResult> GetPagedRooms(int pageNumber = 1)
        {
            var allRooms = await _roomService.GetAllRoomsAsync(); // Fetch all once
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

           

        // For autocomplete API
        [HttpGet("/api/room/autocomplete")]
        public async Task<IActionResult> AutoComplete(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Ok(new List<Room>());

            var rooms = await _roomService.SearchRoomsAsync(query);
            return Ok(rooms);
        }

        [HttpGet("/Rooms/FilterByRoomType")]
        public async Task<IActionResult> FilterByRoomType(string type)
        {
            var rooms = await _roomService.SearchRoomsAsync(type);

            var model = new RoomListViewModel
            {
                Rooms = rooms,
                PageNumber = 1 // Search result page is 1
            };

            return View("GetpagedRooms", model); // reuse same view
        }


        public IActionResult AddRoom() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRoom(Room room)
        {
            if (!ModelState.IsValid)
                return View(room);

            await _roomService.AddRoomAsync(room);
            TempData["SuccessMessage"] = "Room added successfully.";
            return RedirectToAction(nameof(GetPagedRooms));
        }

        public async Task<IActionResult> EditRoom(int id)
        {
            var room = await _roomService.GetRoomByIdAsync(id);
            if (room == null)
                return NotFound();

            return View(room);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRoom(int id, Room room)
        {
            if (id != room.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(room);

            await _roomService.UpdateRoomAsync(room);
            TempData["SuccessMessage"] = "Room updated successfully.";
            return RedirectToAction(nameof(GetPagedRooms));
        }

        public async Task<IActionResult> DeleteRoom(int id)
        {
            var room = await _roomService.GetRoomByIdAsync(id);
            if (room == null)
                return NotFound();

            return View(room);
        }

        [HttpPost, ActionName("DeleteRoom")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles="Admin")]
        public async Task<IActionResult> DeleteRoomConfirmed(int id)
        {
            try
            {
                await _roomService.DeleteRoomAsync(id);
                TempData["SuccessMessage"] = "Room deleted successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting room with ID {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the room.";
            }

            return RedirectToAction(nameof(GetPagedRooms));
        }

        public IActionResult AccessDenied()
        {
            TempData["ErrorMessage"] = "You are not authorized to access this page.";
            return View();
        }
    }
}