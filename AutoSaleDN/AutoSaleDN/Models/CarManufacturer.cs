using System.ComponentModel.DataAnnotations;

namespace AutoSaleDN.Models
{
    public class CarManufacturer
    {
        [Key]
        public int ManufacturerId { get; set; }
        [Required, StringLength(255)]
        public string Name { get; set; }
        public ICollection<CarModel> CarModels { get; set; }
    }
}
