using System.ComponentModel.DataAnnotations;

namespace AutoSaleDN.Models
{
    public class CarColor
    {
        [Key]
        public int ColorId { get; set; }
        public string Name { get; set; }

        public bool Status { get; set; } = true;
    }
}
