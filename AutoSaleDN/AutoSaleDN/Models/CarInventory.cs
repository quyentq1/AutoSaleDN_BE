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
        public int ModelId { get; set; }
        public int? ColorId { get; set; }
        public int QuantityImported { get; set; }
        public int QuantityAvailable { get; set; }
        public int QuantitySold { get; set; }
        public DateTime ImportDate { get; set; }
        public decimal ImportPrice { get; set; }
        public string Notes { get; set; }

        [ForeignKey("ModelId")]
        public CarModel Model { get; set; }
        [ForeignKey("ColorId")]
        public CarColor Color { get; set; }
    }
}