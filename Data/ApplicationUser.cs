using Microsoft.AspNetCore.Identity;
using PetHome.Models;
using System.ComponentModel.DataAnnotations;

namespace PetHome.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string Nickname { get; set; }

        [Url]
        public string AvatarUrl { get; set; }
        public bool[] UserBadges { get; set; } = new bool[12]; 

        public ICollection<Pet> Pets { get; set; }
        public ICollection<Booking> Bookings { get; set; }
    }
}

/*private string GenerateSecureResetCode()
{
    using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
    {
        var bytes = new byte[32];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}*/