using PetHome.Data;

namespace PetHome.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}
