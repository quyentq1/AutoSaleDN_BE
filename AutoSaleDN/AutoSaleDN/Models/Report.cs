using System.ComponentModel.DataAnnotations;

namespace AutoSaleDN.Models
{
    public class Report
    {
        [Key]
        public int ReportId { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        [Required]
        public string ReportType { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        public int? TotalListings { get; set; }
        public int? ActiveListings { get; set; }
        public int? SoldListings { get; set; }
        public int? RentedListings { get; set; }
        public decimal? AverageListingPrice { get; set; }
        public decimal? TotalListingValue { get; set; }
        public int? TotalBookings { get; set; }
        public int? PendingBookings { get; set; }
        public int? ConfirmedBookings { get; set; }
        public int? CanceledBookings { get; set; }
        public int? CompletedBookings { get; set; }
        public decimal? TotalBookingValue { get; set; }
        public int? TotalPayments { get; set; }
        public int? SuccessfulPayments { get; set; }
        public int? FailedPayments { get; set; }
        public int? PendingPayments { get; set; }
        public int? RefundedPayments { get; set; }
        public decimal? TotalRevenue { get; set; }
        public int? TotalReviews { get; set; }
        public decimal? AverageRating { get; set; }
        public int? FiveStarReviews { get; set; }
        public int? FourStarReviews { get; set; }
        public int? ThreeStarReviews { get; set; }
        public int? TwoStarReviews { get; set; }
        public int? OneStarReviews { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.Now;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
