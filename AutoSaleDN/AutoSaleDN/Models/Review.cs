using System.ComponentModel.DataAnnotations;

namespace AutoSaleDN.Models
{
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }
        public int ListingId { get; set; }
        public CarListing Listing { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        [Range(1, 5)]
        public int Rating { get; set; }
        public string? Feedback { get; set; }
        [StringLength(512)]
        public string? ImageUrl { get; set; }
        [StringLength(512)]
        public string? VideoUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
