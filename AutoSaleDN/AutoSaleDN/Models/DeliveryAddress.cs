using System;
using System.ComponentModel.DataAnnotations;

namespace AutoSaleDN.Models
{
    public class DeliveryAddress
    {
        [Key]
        public int AddressId { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        [Required]
        public string Address { get; set; } = null!;
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // THUỘC TÍNH MỚI BỔ SUNG
        public string? RecipientName { get; set; }  
        public string? RecipientPhone { get; set; } 
        public bool IsDefault { get; set; } = false;
        public string? AddressType { get; set; }    
    }
}