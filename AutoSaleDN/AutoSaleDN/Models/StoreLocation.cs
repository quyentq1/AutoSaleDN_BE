using System;
namespace AutoSaleDN.Models
{
    public class StoreLocation
    {
        public int StoreLocationId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Liên kết với Seller
        public int UserId { get; set; }
        public User User { get; set; } // Navigation property
    }
}