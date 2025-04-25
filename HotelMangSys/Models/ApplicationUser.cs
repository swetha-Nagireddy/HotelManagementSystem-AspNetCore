using Microsoft.AspNetCore.Identity;

namespace HotelMangSys.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public string Address { get; set; }

        
    }
}
