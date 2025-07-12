using System.ComponentModel.DataAnnotations;

namespace AutoSaleDN.Models
{
    public class CarSale
    {
        [Key]
        public int SaleId { get; set; }
        public int StoreListingId { get; set; }
        public int CustomerId { get; set; }
        public int SaleStatusId { get; set; }
        public decimal FinalPrice { get; set; }
        public DateTime? SaleDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        public StoreListing StoreListing { get; set; }
        public User Customer { get; set; }
        public SaleStatus SaleStatus { get; set; }
    }
}
