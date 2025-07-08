using System;
using System.ComponentModel.DataAnnotations;

namespace AutoSaleDN.Models
{
    public class DeliveryAddress
    {
        [Key]
        public int AddressId { get; set; } // Khóa chính
        public int UserId { get; set; }
        public string Address { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public User User { get; set; }
    }
}