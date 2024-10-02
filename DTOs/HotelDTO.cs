// DTOs/HotelDTO.cs
using System;
using System.Collections.Generic;
using PetHome.Models;


namespace PetHome.DTOs
{
    public class HotelDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public int PetsAllowed { get; set; }
        public List<TagType> Tags { get; set; }
        public decimal PricePerNight { get; set; }
        public DateTime DateAdded { get; set; }
        public int AvailablePlaces { get; set; }
        public int OccupiedPlaces { get; set; }
        public bool FreeCancellation { get; set; }
        public bool NoPrepayment { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public List<string> PhotoUrls { get; set; }
        public string LargeLogoUrl { get; set; }
        public string SmallLogoUrl { get; set; }
        public int DiscountPercentage { get; set; }
        public string ExtraOption { get; set; }
        public string Description { get; set; }
        public decimal GroomerPrice { get; set; }
        public decimal VetPrice { get; set; }
        public decimal CCTVPrice { get; set; }
        public List<ReviewDTO> Reviews { get; set; }
        public double Percentage1Star { get; set; }
        public double Percentage2Star { get; set; }
        public double Percentage3Star { get; set; }
        public double Percentage4Star { get; set; }
        public double Percentage5Star { get; set; }
        public List<string> CustomTags { get; set; }
    }
}
