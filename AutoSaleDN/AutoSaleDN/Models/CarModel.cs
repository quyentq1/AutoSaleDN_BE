using System.ComponentModel.DataAnnotations;

namespace AutoSaleDN.Models
{
    public class CarModel
    {
        [Key]
        public int ModelId { get; set; }
        public int ManufacturerId { get; set; }

        public CarManufacturer? Manufacturer { get; set; }

        [Required(ErrorMessage = "Model name is required."), StringLength(255)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Status is required.")]
        public string Status { get; set; } = "Active";

        public ICollection<CarListing>? CarListings { get; set; }
    }
}