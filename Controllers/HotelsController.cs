using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetHome.DTOs;
using PetHome.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PetHome.Data;
using System;
using Twilio.Base;

namespace PetHome.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HotelsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HotelsController> _logger;

        public HotelsController(ApplicationDbContext context, ILogger<HotelsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Hotels/5
        [HttpGet("{id}")]
        public async Task<ActionResult<HotelDTO>> GetHotel(int id)
        {
            var hotel = await _context.Hotels
                .Include(h => h.Tags)
                .Include(h => h.CustomTags)
                .Include(h => h.Reviews)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (hotel == null)
            {
                return NotFound();
            }

            var hotelDTO = new HotelDTO
            {
                Id = hotel.Id,
                Name = hotel.Name,
                Location = hotel.Location,
                PetsAllowed = hotel.PetsAllowed,
                PricePerNight = hotel.PricePerNight,
                AvailablePlaces = hotel.AvailablePlaces,
                OccupiedPlaces = hotel.OccupiedPlaces,
                FreeCancellation = hotel.FreeCancellation,
                NoPrepayment = hotel.NoPrepayment,
                PhotoUrls = hotel.PhotoUrls,
                LargeLogoUrl = hotel.LargeLogoUrl,
                SmallLogoUrl = hotel.SmallLogoUrl,
                DiscountPercentage = hotel.DiscountPercentage,
                ExtraOption = hotel.ExtraOption,
                Description = hotel.Description,
                GroomerPrice = hotel.GroomerPrice,
                VetPrice = hotel.VetPrice,
                CCTVPrice = hotel.CCTVPrice,
                AverageRating = hotel.AverageRating,
                ReviewCount = hotel.ReviewCount,
                Percentage1Star = hotel.Percentage1Star,
                Percentage2Star = hotel.Percentage2Star,
                Percentage3Star = hotel.Percentage3Star,
                Percentage4Star = hotel.Percentage4Star,
                Percentage5Star = hotel.Percentage5Star,
                Tags = hotel.Tags.Select(t => t.TagType).ToList(),
                CustomTags = hotel.CustomTags.Select(ct => ct.Tag).ToList(),
                Reviews = hotel.Reviews.Select(r => new ReviewDTO
                {
                    UserId = r.UserId,
                    Username = r.Username,
                    AvatarUrl = r.AvatarUrl,
                    Rating = r.Rating,
                    Text = r.Text,
                    DateAdded = r.DateAdded
                }).ToList()
            };

            return Ok(hotelDTO);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHotel(int id)
        {
            _logger.LogInformation($"Attempting to delete hotel with ID: {id}");

            var hotel = await _context.Hotels
                .Include(h => h.Tags)
                .Include(h => h.CustomTags)
                .Include(h => h.Reviews)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (hotel == null)
            {
                _logger.LogWarning($"Hotel with ID: {id} not found.");
                return NotFound();
            }

            _context.Tags.RemoveRange(hotel.Tags);
            _context.CustomTags.RemoveRange(hotel.CustomTags);
            _context.Reviews.RemoveRange(hotel.Reviews);

            _context.Hotels.Remove(hotel);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Hotel with ID: {id} deleted successfully.");
            return NoContent();
        }

        // POST: api/Hotels
        [HttpPost]
        public async Task<ActionResult<HotelDTO>> CreateHotel(CreateHotelDTO hotelDTO)
        {
            _logger.LogInformation("CreateHotel called");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state.");
                return BadRequest(ModelState);
            }

            var hotel = new Hotel
            {
                Name = hotelDTO.Name,
                Location = hotelDTO.Location,
                PetsAllowed = hotelDTO.PetsAllowed,
                PricePerNight = hotelDTO.PricePerNight,
                DateAdded = DateTime.UtcNow,
                AvailablePlaces = hotelDTO.AvailablePlaces,
                OccupiedPlaces = hotelDTO.OccupiedPlaces,
                FreeCancellation = hotelDTO.FreeCancellation,
                NoPrepayment = hotelDTO.NoPrepayment,
                PhotoUrls = hotelDTO.PhotoUrls,
                LargeLogoUrl = hotelDTO.LargeLogoUrl,
                SmallLogoUrl = hotelDTO.SmallLogoUrl,
                DiscountPercentage = hotelDTO.DiscountPercentage,
                ExtraOption = hotelDTO.ExtraOption,
                Description = hotelDTO.Description,
                GroomerPrice = hotelDTO.GroomerPrice,
                VetPrice = hotelDTO.VetPrice,
                CCTVPrice = hotelDTO.CCTVPrice,

                AverageRating = 0,
                ReviewCount = 0,
                Percentage1Star = 0,
                Percentage2Star = 0,
                Percentage3Star = 0,
                Percentage4Star = 0,
                Percentage5Star = 0
            };

            if (hotelDTO.Tags != null)
            {
                foreach (var tag in hotelDTO.Tags)
                {
                    hotel.Tags.Add(new Tag { TagType = tag });
                }
            }

            if (hotelDTO.CustomTags != null)
            {
                foreach (var customTag in hotelDTO.CustomTags.Take(5))
                {
                    hotel.CustomTags.Add(new CustomTag { Tag = customTag });
                }
            }

            if (hotelDTO.Reviews != null)
            {
                foreach (var reviewDTO in hotelDTO.Reviews)
                {
                    var review = new Review
                    {
                        UserId = reviewDTO.UserId,
                        Username = reviewDTO.Username,
                        AvatarUrl = reviewDTO.AvatarUrl,
                        Rating = reviewDTO.Rating,
                        Text = reviewDTO.Text,
                        DateAdded = reviewDTO.DateAdded != default
                            ? reviewDTO.DateAdded
                            : DateTime.UtcNow
                    };
                    hotel.Reviews.Add(review);
                }

                hotel.ReviewCount = hotel.Reviews.Count;
                if (hotel.ReviewCount > 0)
                {
                    hotel.AverageRating = hotel.Reviews.Average(r => r.Rating);
                    hotel.Percentage1Star = (double)hotel.Reviews.Count(r => r.Rating == 1) / hotel.ReviewCount * 100;
                    hotel.Percentage2Star = (double)hotel.Reviews.Count(r => r.Rating == 2) / hotel.ReviewCount * 100;
                    hotel.Percentage3Star = (double)hotel.Reviews.Count(r => r.Rating == 3) / hotel.ReviewCount * 100;
                    hotel.Percentage4Star = (double)hotel.Reviews.Count(r => r.Rating == 4) / hotel.ReviewCount * 100;
                    hotel.Percentage5Star = (double)hotel.Reviews.Count(r => r.Rating == 5) / hotel.ReviewCount * 100;
                }
            }

            _context.Hotels.Add(hotel);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Hotel created with ID: {hotel.Id}");

            var createdHotelDTO = new HotelDTO
            {
                Id = hotel.Id,
                Name = hotel.Name,
                Location = hotel.Location,
                PetsAllowed = hotel.PetsAllowed,
                PricePerNight = hotel.PricePerNight,
                AvailablePlaces = hotel.AvailablePlaces,
                OccupiedPlaces = hotel.OccupiedPlaces,
                FreeCancellation = hotel.FreeCancellation,
                NoPrepayment = hotel.NoPrepayment,
                PhotoUrls = hotel.PhotoUrls,
                LargeLogoUrl = hotel.LargeLogoUrl,
                SmallLogoUrl = hotel.SmallLogoUrl,
                DiscountPercentage = hotel.DiscountPercentage,
                ExtraOption = hotel.ExtraOption,
                Description = hotel.Description,
                GroomerPrice = hotel.GroomerPrice,
                VetPrice = hotel.VetPrice,
                CCTVPrice = hotel.CCTVPrice,
                AverageRating = hotel.AverageRating,
                ReviewCount = hotel.ReviewCount,
                Percentage1Star = hotel.Percentage1Star,
                Percentage2Star = hotel.Percentage2Star,
                Percentage3Star = hotel.Percentage3Star,
                Percentage4Star = hotel.Percentage4Star,
                Percentage5Star = hotel.Percentage5Star,
                Tags = hotel.Tags.Select(t => t.TagType).ToList(),
                CustomTags = hotel.CustomTags.Select(ct => ct.Tag).ToList(),
                Reviews = hotel.Reviews.Select(r => new ReviewDTO
                {
                    UserId = r.UserId,
                    Username = r.Username,
                    AvatarUrl = r.AvatarUrl,
                    Rating = r.Rating,
                    Text = r.Text,
                    DateAdded = r.DateAdded
                }).ToList()
            };

            return CreatedAtAction(nameof(GetHotel), new { id = hotel.Id }, createdHotelDTO);
        }
        [HttpGet("catalog")]
        public async Task<ActionResult<PagedResponse<CatalogHotelDTO>>> GetHotelCatalog([FromQuery] HotelQueryParameters queryParameters)
        {
            IQueryable<Hotel> hotelsQuery = _context.Hotels
                .Include(h => h.Tags)
                .Include(h => h.CustomTags)
                .AsQueryable();

            if (queryParameters.Tags != null && queryParameters.Tags.Any())
            {
                List<TagType> tagTypes = new List<TagType>();
                foreach (var tag in queryParameters.Tags)
                {
                    if (Enum.TryParse<TagType>(tag, true, out var tagType))
                    {
                        tagTypes.Add(tagType);
                    }
                    else
                    {
                        _logger.LogWarning($"Invalid tag type: {tag}");
                    }
                }

                if (tagTypes.Any())
                {
                    hotelsQuery = hotelsQuery.Where(h =>
                        h.Tags.Any(t => tagTypes.Contains(t.TagType)) ||
                        h.CustomTags.Any(ct => queryParameters.Tags.Contains(ct.Tag)));
                }
            }

            if (queryParameters.MinRating.HasValue)
            {
                hotelsQuery = hotelsQuery.Where(h => h.AverageRating >= queryParameters.MinRating.Value);
            }

            if (queryParameters.PetsAllowed.HasValue)
            {
                if (queryParameters.PetsAllowed.Value == 0)
                {
                    hotelsQuery = hotelsQuery.Where(h => h.PetsAllowed == 0 || h.PetsAllowed == 2);
                }
                else if (queryParameters.PetsAllowed.Value == 1)
                {
                    hotelsQuery = hotelsQuery.Where(h => h.PetsAllowed == 1 || h.PetsAllowed == 2);
                }
                else if (queryParameters.PetsAllowed.Value == 2)
                {
                    hotelsQuery = hotelsQuery.Where(h => h.PetsAllowed == 2);
                }
            }

            if (queryParameters.PriceMin.HasValue)
            {
                hotelsQuery = hotelsQuery.Where(h => h.PricePerNight >= queryParameters.PriceMin.Value);
            }
            if (queryParameters.PriceMax.HasValue)
            {
                hotelsQuery = hotelsQuery.Where(h => h.PricePerNight <= queryParameters.PriceMax.Value);
            }

            if (!string.IsNullOrWhiteSpace(queryParameters.SearchTerm))
            {
                hotelsQuery = hotelsQuery.Where(h => h.Name.Contains(queryParameters.SearchTerm));
            }

            if (!string.IsNullOrWhiteSpace(queryParameters.SortBy))
            {
                switch (queryParameters.SortBy.ToLower())
                {
                    case "best_selling":
                        hotelsQuery = hotelsQuery.OrderByDescending(h => h.ReviewCount).ThenByDescending(h => h.AverageRating);
                        break;
                    case "name_asc":
                        hotelsQuery = hotelsQuery.OrderBy(h => h.Name);
                        break;
                    case "name_desc":
                        hotelsQuery = hotelsQuery.OrderByDescending(h => h.Name);
                        break;
                    case "price_asc":
                        hotelsQuery = hotelsQuery.OrderBy(h => h.PricePerNight);
                        break;
                    case "price_desc":
                        hotelsQuery = hotelsQuery.OrderByDescending(h => h.PricePerNight);
                        break;
                    case "date_old_new":
                        hotelsQuery = hotelsQuery.OrderBy(h => h.DateAdded);
                        break;
                    case "date_new_old":
                        hotelsQuery = hotelsQuery.OrderByDescending(h => h.DateAdded);
                        break;
                    default:
                        hotelsQuery = hotelsQuery.OrderByDescending(h => h.ReviewCount).ThenByDescending(h => h.AverageRating);
                        break;
                }
            }
            else
            {
                hotelsQuery = hotelsQuery.OrderByDescending(h => h.ReviewCount).ThenByDescending(h => h.AverageRating);
            }

            var totalCount = await hotelsQuery.CountAsync();

            var hotels = await hotelsQuery
                .Skip((queryParameters.PageNumber - 1) * queryParameters.PageSize)
                .Take(queryParameters.PageSize)
                .Select(h => new CatalogHotelDTO
                {
                    Id = h.Id,
                    Name = h.Name,
                    Location = h.Location,
                    DateAdded = h.DateAdded,
                    PetsAllowed = h.PetsAllowed,
                    PricePerNight = h.PricePerNight,
                    AvailablePlaces = h.AvailablePlaces - h.OccupiedPlaces,
                    FreeCancellation = h.FreeCancellation,
                    NoPrepayment = h.NoPrepayment,
                    AverageRating = h.AverageRating,
                    ReviewCount = h.ReviewCount,
                    SmallLogoUrl = h.SmallLogoUrl,
                    DiscountPercentage = h.DiscountPercentage,
                    Tags = h.Tags
                        .Select(t => t.TagType)
                        .ToList(),
                    CustomTags = h.CustomTags.Select(ct => ct.Tag).ToList(),
                    ExtraOption = h.ExtraOption,
                    PhotoUrl = h.PhotoUrls.FirstOrDefault()
                })
                .ToListAsync();

            var response = new PagedResponse<CatalogHotelDTO>(hotels, totalCount, queryParameters.PageNumber, queryParameters.PageSize);

            return Ok(response);
        }
    }


    public class PaginatedList<T> : List<T>
    {
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }

        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            this.AddRange(items);
        }

        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }
    }
    public class PagedResponse<T>
    {
        public List<T> Items { get; set; }
        public int PageIndex { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }

        public PagedResponse(List<T> items, int count, int pageIndex, int pageSize)
        {
            Items = items;
            TotalCount = count;
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        }
    }
}
