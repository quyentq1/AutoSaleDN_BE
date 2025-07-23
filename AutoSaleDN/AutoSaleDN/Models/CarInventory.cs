using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoSaleDN.Models
{
    public class CarInventory
    {
        [Key]
        public int InventoryId { get; set; }
        public int StoreListingId { get; set; }
        public int TransactionType { get; set; }
        public int Quantity { get; set; }
        public decimal? UnitPrice { get; set; } 
        public string ReferenceId { get; set; }
        public string Notes { get; set; }
        public string CreatedBy { get; set; }
        public DateTime TransactionDate { get; set; }
        public DateTime CreatedAt { get; set; }

        [ForeignKey("StoreListingId")]
        public StoreListing StoreListing { get; set; }
    }
}