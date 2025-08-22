using System.ComponentModel.DataAnnotations;
namespace AutoSaleDN.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
        public string? Name { get; set; }
        public string? Password { get; set; }
        [Required, StringLength(100)]
        public string Email { get; set; }
        [Required, StringLength(100)]
        public string FullName { get; set; }
        [StringLength(15)]
        public string? Mobile { get; set; }

        public string? Role { get; set; }
        [Required, StringLength(100)]
        public string Province { get; set; }
        public bool Status { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public int? StoreLocationId { get; set; }

        public StoreLocation? StoreLocation { get; set; }

        public ICollection<CarListing>? CarListings { get; set; }
        public ICollection<Payment>? Payments { get; set; }
        public ICollection<Report>? Reports { get; set; }
        public ICollection<Review>? Reviews { get; set; }
        public ICollection<BlogPost>? BlogPosts { get; set; }
        public ICollection<CarSale>? CarSales { get; set; }


        public ICollection<DeliveryAddress>? DeliveryAddresses { get; set; }
    }
}