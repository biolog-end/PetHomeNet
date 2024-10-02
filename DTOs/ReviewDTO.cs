using System;
using System.Collections.Generic;
using PetHome.Models;

namespace PetHome.DTOs
{
    public class ReviewDTO
    {
        public string UserId { get; set; }//test only
        public string Username { get; set; }
        public DateTime DateAdded { get; set; }
        public string AvatarUrl { get; set; }
        public int Rating { get; set; }
        public string Text { get; set; }
    }

}