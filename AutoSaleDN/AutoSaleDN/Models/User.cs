using System.ComponentModel.DataAnnotations;

namespace AutoSaleDN.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required, StringLength(50)]
        public string Name { get; set; }
        [Required, StringLength(255)]
        public string Password { get; set; }
        [Required, StringLength(100)]
        public string Email { get; set; }
        [Required, StringLength(100)]
        public string FullName { get; set; }
        [StringLength(15)]
        public string? Mobile { get; set; }
        [Required, StringLength(20)]
        public string Role { get; set; }

        [Required, StringLength(100)]
        public string Province { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public ICollection<CarListing> CarListings { get; set; }
        public ICollection<Booking> Bookings { get; set; }
        public ICollection<Payment> Payments { get; set; }
        public ICollection<Report> Reports { get; set; }
        public ICollection<Review> Reviews { get; set; }
        public ICollection<BlogPost> BlogPosts { get; set; }
    }
}
