using System.ComponentModel.DataAnnotations;

namespace AutoSaleDN.Models
{
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        [Required]
        public string TransactionId { get; set; }
        [Required]
        public decimal Amount { get; set; }
        public string? PaymentMethod { get; set; }
        public string? PaymentStatus { get; set; } = "pending";
        public DateTime DateOfPayment { get; set; } = DateTime.Now;
        public int ListingId { get; set; }
        public CarListing Listing { get; set; }
        public int BookingId { get; set; }
        public Booking Booking { get; set; }
        public string? AdditionalDetails { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public ICollection<PaymentTransaction>? PaymentTransactions { get; set; }
    }
}
