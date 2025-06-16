using System.ComponentModel.DataAnnotations;

namespace AutoSaleDN.Models
{
    public class CarModel
    {
        [Key]
        public int ModelId { get; set; }
        public int ManufacturerId { get; set; }
        public CarManufacturer Manufacturer { get; set; }
        [Required, StringLength(255)]
        public string Name { get; set; }
        public ICollection<CarListing>? CarListings { get; set; }
    }
}
