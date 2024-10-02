using System.ComponentModel.DataAnnotations;

namespace PetHome.Models
{
    public class CustomTag
    {
        public int Id { get; set; }

        [MaxLength(50)]
        public string Tag { get; set; }

        public int HotelId { get; set; }
        public Hotel Hotel { get; set; }
    }
}
