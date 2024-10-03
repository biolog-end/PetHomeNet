namespace PetHome.DTOs
{
    public class HotelQueryParameters
    {
        public List<string>? Tags { get; set; }
        public int? PetsAllowed { get; set; }

        public double? MinRating { get; set; }
        public decimal? PriceMin { get; set; }
        public decimal? PriceMax { get; set; }
        public string? SearchTerm { get; set; }

        
        public string? SortBy { get; set; } 

        
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 8;
    }
}
