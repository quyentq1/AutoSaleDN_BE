using System.ComponentModel.DataAnnotations;

namespace AutoSaleDN.Models
{
    public class CarSpecification
    {
        [Key]
        public int SpecificationId { get; set; }
        public int ListingId { get; set; }
        public CarListing Listing { get; set; }
        public string? Engine { get; set; }
        public string? Transmission { get; set; }
        public string? FuelType { get; set; }
        public int? SeatingCapacity { get; set; }
        public string? InteriorColor { get; set; }
        public string? ExteriorColor { get; set; }
        public string? CarType { get; set; }
    }
}
