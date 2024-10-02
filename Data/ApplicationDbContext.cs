using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PetHome.Models;

namespace PetHome.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<CustomTag> CustomTags { get; set; }
        public DbSet<Review> Reviews { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Tag>()
                .Property(t => t.TagType)
                .HasConversion<string>();

            builder.Entity<Tag>()
                .HasOne(t => t.Hotel)
                .WithMany(h => h.Tags)
                .HasForeignKey(t => t.HotelId);

            builder.Entity<CustomTag>()
                .HasOne(ct => ct.Hotel)
                .WithMany(h => h.CustomTags)
                .HasForeignKey(ct => ct.HotelId);
           /* builder.Entity<Review>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);*/
        }
    }
}
