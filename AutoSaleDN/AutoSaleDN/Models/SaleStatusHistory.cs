using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AutoSaleDN.Models
{
    public class SaleStatusHistory
    {
        [Key]
        public int Id { get; set; }

        public int SaleId { get; set; }
        public int SaleStatusId { get; set; }
        public int? UserId { get; set; }
        public string? Notes { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;

        [ForeignKey("SaleId")]
        public CarSale CarSale { get; set; } = null!;

        [ForeignKey("SaleStatusId")]
        public SaleStatus SaleStatus { get; set; } = null!;

        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}
