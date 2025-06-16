using System.ComponentModel.DataAnnotations;

namespace AutoSaleDN.Models
{
    public class CarListing
    {
        [Key]
        public int ListingId { get; set; }
        public int ModelId { get; set; }
        public CarModel Model { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int? Year { get; set; }
        public int? Mileage { get; set; }
        public decimal? Price { get; set; }
        public string? Location { get; set; }
        public string? Condition { get; set; }
        public string? ListingStatus { get; set; } = "active";
        public DateTime DatePosted { get; set; } = DateTime.Now;
        public DateTime DateUpdated { get; set; } = DateTime.Now;
        public bool Certified { get; set; } = false;
        public string? Vin { get; set; }
        public string? Description { get; set; }
        public string? RentSell { get; set; }
        public ICollection<CarSpecification>? Specifications { get; set; }
        public ICollection<CarListingFeature>? CarListingFeatures { get; set; }
        public ICollection<CarImage>? CarImages { get; set; }
        public ICollection<Booking>? Bookings { get; set; }
        public ICollection<CarServiceHistory>? CarServiceHistories { get; set; }
        public ICollection<CarPricingDetail>? CarPricingDetails { get; set; }
        public ICollection<Review>? Reviews { get; set; }
        public ICollection<CarSale>? CarSales { get; set; }
        public ICollection<Payment>? Payments { get; set; }
    }
}
