using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AutoSaleDN.Models
{
    public class CarManufacturer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ManufacturerId { get; set; }

        [Required(ErrorMessage = "Manufacturer name is required.")]
        [StringLength(255, ErrorMessage = "Manufacturer name cannot exceed 255 characters.")]
        public string Name { get; set; } = string.Empty;

        [JsonIgnore]
        public ICollection<CarModel>? CarModels { get; set; }
    }
}