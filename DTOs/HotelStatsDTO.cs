namespace PetHome.DTOs
{
    public class HotelStatsDTO
    {
        public int HotelsWithRating1OrAbove { get; set; }
        public int HotelsWithRating2OrAbove { get; set; }
        public int HotelsWithRating3OrAbove { get; set; }
        public int HotelsWithRating4OrAbove { get; set; }
        public Dictionary<string, int> TagCounts { get; set; }
    }

}
