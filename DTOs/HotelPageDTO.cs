namespace PetHome.DTOs
{
    public class HotelPageDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public decimal PricePerNight { get; set; }
        public string LargeLogoUrl { get; set; }
        public List<string> PhotoUrls { get; set; }
        public double AverageRating { get; set; }
        public double Percentage1Star { get; set; }
        public double Percentage2Star { get; set; }
        public double Percentage3Star { get; set; }
        public double Percentage4Star { get; set; }
        public double Percentage5Star { get; set; }
        public decimal GroomerPrice { get; set; }
        public decimal VetPrice { get; set; }
        public decimal CCTVPrice { get; set; }
        public List<ReviewDTO> Reviews { get; set; }
    }
}
