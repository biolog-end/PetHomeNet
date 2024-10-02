// DTOs/CatalogHotelDTO.cs
using System;
using System.Collections.Generic;
using PetHome.Models;

namespace PetHome.DTOs
{
    public class CatalogHotelDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public DateTime DateAdded { get; set; }
        public int PetsAllowed { get; set; }
        public decimal PricePerNight { get; set; }
        public int AvailablePlaces { get; set; }
        public bool FreeCancellation { get; set; }
        public bool NoPrepayment { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public string SmallLogoUrl { get; set; }
        public double DiscountPercentage { get; set; }
        public List<TagType> Tags { get; set; }
        public List<string> CustomTags { get; set; }
        public string ExtraOption { get; set; }
        public string PhotoUrl { get; set; } 
    }
}
