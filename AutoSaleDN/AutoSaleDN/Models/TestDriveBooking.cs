using System;
using System.ComponentModel.DataAnnotations;

namespace AutoSaleDN.Models
{
    public class TestDriveBooking
    {
        [Key]
        public int BookingId { get; set; }

        [Required]
        public int StoreListingId { get; set; }
        public StoreListing StoreListing { get; set; }

        [Required]
        public int UserId { get; set; }
        public User User { get; set; }

        [Required]
        public DateTime BookingDate { get; set; }

        [Required]
        public bool HasLicense { get; set; }

        public string? Notes { get; set; }

        [Required]
        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}