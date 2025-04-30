using Microsoft.AspNetCore.Identity;

namespace HotelMangSys.Models
{
    // This class represents the user in the application.
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public string Address { get; set; }

        
    }
}
