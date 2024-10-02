using System;
using System.ComponentModel.DataAnnotations;

namespace PetHome.Models
{
    public class Review
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; }

        public DateTime DateAdded { get; set; }

        [Url]
        public string AvatarUrl { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Text { get; set; }
    }
}
