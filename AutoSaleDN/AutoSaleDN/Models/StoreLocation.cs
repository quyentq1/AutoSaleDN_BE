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
        public ICollection<User>? Users { get; set; }
        public ICollection<StoreListing>? StoreListings { get; set; }
    }
}