using System.ComponentModel.DataAnnotations;

namespace AutoSaleDN.Models
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }
        public int ListingId { get; set; }
        public CarListing Listing { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        [Required]
        public DateTime BookingStartDate { get; set; }
        [Required]
        public DateTime BookingEndDate { get; set; }
        [Required]
        public decimal TotalPrice { get; set; }
        [Required]
        public decimal PaidPrice { get; set; }
        public string? BookingStatus { get; set; } = "pending";
        public string? PaymentStatus { get; set; } = "pending";
        public string? TransactionId { get; set; }
        public ICollection<Payment>? Payments { get; set; }
        public ICollection<CarSale>? CarSales { get; set; }
    }
}
