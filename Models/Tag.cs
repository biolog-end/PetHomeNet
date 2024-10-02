namespace PetHome.Models
{
    public class Tag
    {
        public int Id { get; set; }
        public TagType TagType { get; set; }

        public int HotelId { get; set; }
        public Hotel Hotel { get; set; }
    }
}
