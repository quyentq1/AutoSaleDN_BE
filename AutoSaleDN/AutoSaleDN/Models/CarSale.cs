using System.ComponentModel.DataAnnotations;

namespace AutoSaleDN.Models
{
    public class CarSale
    {
        [Key]
        public int SaleId { get; set; }
        public int ListingId { get; set; }
        public int? BookingId { get; set; } // Thêm dòng này, nullable nếu không phải sale nào cũng có booking
        public int CustomerId { get; set; } // Thêm dòng này
        public int SaleStatusId { get; set; }
        public decimal FinalPrice { get; set; }
        public DateTime? SaleDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        public CarListing Listing { get; set; }
        public Booking? Booking { get; set; }
        public SaleStatus SaleStatus { get; set; }
    }
}
