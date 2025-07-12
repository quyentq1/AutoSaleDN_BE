using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoSaleDN.Models
{
    public class CarColor
    {
        [Key]
        public int ColorId { get; set; }
        public string Name { get; set; }
    }

    public class CarInventory
    {
        [Key]
        public int InventoryId { get; set; }
        public int StoreListingId { get; set; }
        public int TransactionType { get; set; } // 1: Nhập hàng, 2: Xuất hàng, 3: Điều chỉnh
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