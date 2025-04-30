using HotelMangSys.Models;

namespace HotelMangSys.Services
{
    /// <summary>
    /// /// Interface for room service to handle room operations
    /// </summary>
    public interface IRoomService
    {

        Task<List<Room>> GetAllRoomsAsync();
        Task<List<Room>> SearchRoomsAsync(string query);
        Task AddRoomAsync(Room room);
        Task<Room?> GetRoomByIdAsync(int id);
        Task UpdateRoomAsync(Room room);
        Task DeleteRoomAsync(int id);
    }
}
