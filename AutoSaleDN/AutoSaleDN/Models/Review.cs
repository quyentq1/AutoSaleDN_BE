using System.ComponentModel.DataAnnotations;

namespace AutoSaleDN.Models
{
    public class Review
    {
        public int ReviewId { get; set; }
        public int ListingId { get; set; }
        public int UserId { get; set; }
        public int Rating { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }

        // Thêm các trường sau:
        public string? Reply { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public CarListing Listing { get; set; }
        public User User { get; set; }
    }
}
