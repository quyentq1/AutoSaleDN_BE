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

        public string? OrderNumber { get; set; }
        public decimal? DepositAmount { get; set; }
        public decimal? RemainingBalance { get; set; }
        public string? DeliveryOption { get; set; }

        public int? ShippingAddressId { get; set; }        
        public int? PickupStoreLocationId { get; set; }    

        public DateTime? ExpectedDeliveryDate { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }  

        public int? DepositPaymentId { get; set; }         
        public int? FullPaymentId { get; set; }            

        public string? OrderType { get; set; }             
        public string? Notes { get; set; }


        public StoreListing StoreListing { get; set; } = null!;
        public User Customer { get; set; } = null!;
        public SaleStatus SaleStatus { get; set; } = null!;

        public DeliveryAddress? ShippingAddress { get; set; }
        public StoreLocation? PickupStoreLocation { get; set; }
        public Payment? DepositPayment { get; set; }
        public Payment? FullPayment { get; set; }

        public ICollection<SaleStatusHistory> StatusHistory { get; set; } = new List<SaleStatusHistory>();
    }
}
