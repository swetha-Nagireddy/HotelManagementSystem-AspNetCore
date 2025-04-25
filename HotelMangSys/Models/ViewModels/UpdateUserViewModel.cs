namespace HotelMangSys.Models.ViewModels
{
    public class UpdateUserViewModel
    {
        public string Id { get; set; }  // Needed for updating the correct user

        
        public string FullName { get; set; }

        public string Address { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }
    }
}
