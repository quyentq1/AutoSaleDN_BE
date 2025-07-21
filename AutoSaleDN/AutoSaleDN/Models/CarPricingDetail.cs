using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoSaleDN.Models
{
    public class CarPricingDetail
    {
        [Key]
        public int PricingDetailId { get; set; }
        public int ListingId { get; set; }
        public CarListing Listing { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxRate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal RegistrationFee { get; set; }
    }
}
