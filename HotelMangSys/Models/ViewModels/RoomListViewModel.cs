using HotelMangSys.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelMangSys.Models.ViewModels
{
    public class RoomListViewModel
    {
        //public List<Room> Rooms { get; set; }
        //public int PageNumber { get; set; }
        //public int PageSize { get; set; }
        //public int TotalPages { get; set; }
        //public int TotalCount { get; set; }

        //public string SearchQuery { get; set; } // Add this property

        public List<Room> Rooms { get; set; } = new List<Room>();
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
    }
}
