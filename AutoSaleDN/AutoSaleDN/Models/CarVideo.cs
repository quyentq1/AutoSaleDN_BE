using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AutoSaleDN.Models
{
    public class CarVideo
    {
        [Key]
        public int VideoId { get; set; }

        [Required]
        public int ListingId { get; set; }

        [Required]
        public string Url { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation property với ForeignKey attribute
        [ForeignKey("ListingId")]
        public virtual CarListing CarListing { get; set; }
    }
}
