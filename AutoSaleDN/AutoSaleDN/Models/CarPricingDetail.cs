using System.ComponentModel.DataAnnotations;

namespace AutoSaleDN.Models
{
    public class CarPricingDetail
    {
        [Key]
        public int PricingDetailId { get; set; }
        public int ListingId { get; set; }
        public CarListing Listing { get; set; }
        public decimal TaxRate { get; set; } = 0.08M;
        public decimal RegistrationFee { get; set; } = 300M;
    }
}
