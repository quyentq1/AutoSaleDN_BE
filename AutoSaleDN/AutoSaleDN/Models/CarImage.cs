using System.ComponentModel.DataAnnotations;

namespace AutoSaleDN.Models
{
    public class CarImage
    {
        [Key]
        public int ImageId { get; set; }
        public int ListingId { get; set; }
        public string? Url { get; set; }
        public string? Filename { get; set; }

        public virtual CarListing CarListing { get; set; }
    }
}
