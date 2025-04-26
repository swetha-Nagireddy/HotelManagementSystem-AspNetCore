using Dapper;
using HotelMangSys.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace HotelMangSys.Services
{
    public class RoomService : IRoomService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<RoomService> _logger;
        private readonly IDapperWrapper _dapperWrapper;

        public RoomService(IConfiguration configuration, ILogger<RoomService> logger, IDapperWrapper dapperWrapper)
        {
            _configuration = configuration;
            _logger = logger;
            _dapperWrapper = dapperWrapper;
        }

        // private IDbConnection Connection => new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        // Method to get paginated rooms
        // RoomService.cs

        // Add this method
        public async Task<List<Room>> GetAllRoomsAsync()
        {
            var connectionString = _configuration["ConnectionStrings:DefaultConnection"];
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException("Connection string is not configured.");

            using IDbConnection db = new SqlConnection(connectionString);

            string sql = "SELECT * FROM Rooms ORDER BY Id";
            return (await _dapperWrapper.QueryAsync<Room>(db, sql)).ToList();
        }

        public async Task<List<Room>> SearchRoomsAsync(string query)
        {
            var connectionString = _configuration["ConnectionStrings:DefaultConnection"];
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException("Connection string is not configured.");

            using IDbConnection db = new SqlConnection(connectionString);

            string sql = @"SELECT * FROM Rooms 
                   WHERE LOWER(Type) LIKE LOWER(@Query + '%') 
                   OR CAST(Price AS VARCHAR) LIKE @Query + '%'";

            return (await _dapperWrapper.QueryAsync<Room>(db, sql, new { Query = query })).ToList();
        }

        public async Task AddRoomAsync(Room room)
        {
            var connectionString = _configuration["ConnectionStrings:DefaultConnection"];
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string is not configured.");
            }

            using IDbConnection db = new SqlConnection(connectionString);
            string query = "INSERT INTO Rooms (Type, Price, IsAvailable) VALUES (@Type, @Price, @IsAvailable)";
            await _dapperWrapper.ExecuteAsync(db, query, room);
        }

        public async Task<Room?> GetRoomByIdAsync(int id)
        {
            var connectionString = _configuration["ConnectionStrings:DefaultConnection"];
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string is not configured.");
            }

            using IDbConnection db = new SqlConnection(connectionString);
            return await _dapperWrapper.QuerySingleAsync<Room>(db, "SELECT * FROM Rooms WHERE Id = @Id", new { Id = id });
        }

        public async Task UpdateRoomAsync(Room room)
        {
            var connectionString = _configuration["ConnectionStrings:DefaultConnection"];
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string is not configured.");
            }

            using IDbConnection db = new SqlConnection(connectionString);
            string query = "UPDATE Rooms SET Type = @Type, Price = @Price, IsAvailable = @IsAvailable WHERE Id = @Id";
            await _dapperWrapper.ExecuteAsync(db, query, room);
        }

        public async Task DeleteRoomAsync(int id)
        {
            var connectionString = _configuration["ConnectionStrings:DefaultConnection"];
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string is not configured.");
            }

            using IDbConnection db = new SqlConnection(connectionString);
            string query = "DELETE FROM Rooms WHERE Id = @Id";
            await _dapperWrapper.ExecuteAsync(db, query, new { Id = id });
        }
    }
}