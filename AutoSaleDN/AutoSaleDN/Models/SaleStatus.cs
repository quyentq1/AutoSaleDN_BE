using System.ComponentModel.DataAnnotations;

namespace AutoSaleDN.Models
{
    public class SaleStatus
    {
        [Key]
        public int SaleStatusId { get; set; }
        [Required, StringLength(50)]
        public string StatusName { get; set; }
        public ICollection<CarSale>? CarSales { get; set; }
    }
}
