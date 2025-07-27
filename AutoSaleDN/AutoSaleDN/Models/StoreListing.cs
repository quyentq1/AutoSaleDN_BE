using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoSaleDN.Models
{
    public class StoreListing
    {
        [Key]
        public int StoreListingId { get; set; }

        public int StoreLocationId { get; set; }
        [ForeignKey("StoreLocationId")]
        public StoreLocation StoreLocation { get; set; }

        public int ListingId { get; set; }
        [ForeignKey("ListingId")]
        public CarListing CarListing { get; set; }

        public int InitialQuantity { get; set; }
        public int CurrentQuantity { get; set; }
        public int AvailableQuantity { get; set; }
        public string Status { get; set; } = "In Stock";
        public DateTime AddedDate { get; set; } = DateTime.Now;
        public DateTime? LastSoldDate { get; set; }
        public DateTime? RemovedDate { get; set; }
        public string? ReasonForRemoval { get; set; }
        public DateTime? LastStatusChangeDate { get; set; }
        public decimal? LastPurchasePrice { get; set; }
        public decimal? AverageCost { get; set; }

        public bool IsCurrent { get; set; } = true;

        public ICollection<CarInventory>? Inventories { get; set; }
        public ICollection<CarSale>? CarSales { get; set; }
    }
}