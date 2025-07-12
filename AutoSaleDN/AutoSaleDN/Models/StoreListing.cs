using System.ComponentModel.DataAnnotations;

namespace AutoSaleDN.Models
{
    public class StoreListing
    {
        [Key]
        public int StoreListingId { get; set; }
        public int StoreLocationId { get; set; }
        public int ListingId { get; set; }
        public int InitialQuantity { get; set; }
        public int CurrentQuantity { get; set; }
        public int AvailableQuantity { get; set; }
        public string Status { get; set; } = "IN_STOCK";
        public DateTime AddedDate { get; set; }
        public DateTime? LastSoldDate { get; set; }
        public DateTime? RemovedDate { get; set; }
        public string? ReasonForRemoval { get; set; }
        public DateTime? LastStatusChangeDate { get; set; }
        public decimal? LastPurchasePrice { get; set; }
        public decimal? AverageCost { get; set; }

        public StoreLocation StoreLocation { get; set; }
        public CarListing CarListing { get; set; }
        public ICollection<CarSale> CarSales { get; set; }
        public ICollection<CarInventory> Inventories { get; set; }
    }
}
