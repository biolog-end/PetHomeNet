using Azure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PetHome.Models
{
    public class Hotel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(200)]
        public string Location { get; set; }

        // Pets: 0 - Cats, 1 - Dogs, 2 - Both
        [Range(0, 2)]
        public int PetsAllowed { get; set; }
        public ICollection<Tag> Tags { get; set; } = new List<Tag>();

        [Range(10, 10000)]
        public decimal PricePerNight { get; set; }

        public DateTime DateAdded { get; set; } = DateTime.UtcNow;

        [Range(0, int.MaxValue)]
        public int AvailablePlaces { get; set; }

        [Range(0, int.MaxValue)]
        public int OccupiedPlaces { get; set; }

        public bool FreeCancellation { get; set; }

        public bool NoPrepayment { get; set; }

        [Range(0, 5)]
        public double AverageRating { get; set; }

        [Range(0, int.MaxValue)]
        public int ReviewCount { get; set; }

        public List<string> PhotoUrls { get; set; } = new List<string>();

        public string LargeLogoUrl { get; set; }

        public string SmallLogoUrl { get; set; }

        [Range(0, 100)]
        public int DiscountPercentage { get; set; }

        public string ExtraOption { get; set; }

        public string Description { get; set; }

        [Range(0, 10000)]
        public decimal GroomerPrice { get; set; }

        [Range(0, 10000)]
        public decimal VetPrice { get; set; }

        [Range(0, 10000)]
        public decimal CCTVPrice { get; set; }

        public ICollection<Review> Reviews { get; set; } = new List<Review>();

        // Percentage breakdown of reviews
        public double Percentage1Star { get; set; }
        public double Percentage2Star { get; set; }
        public double Percentage3Star { get; set; }
        public double Percentage4Star { get; set; }
        public double Percentage5Star { get; set; }

        // Additional custom tags (max 5)
        public ICollection<CustomTag> CustomTags { get; set; } = new List<CustomTag>();
    }
}
