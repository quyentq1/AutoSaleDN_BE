using System.ComponentModel.DataAnnotations;

namespace AutoSaleDN.Models
{
    public class CarServiceHistory
    {
        [Key]
        public int HistoryId { get; set; }
        public int ListingId { get; set; }
        public CarListing Listing { get; set; }
        public bool RecentServicing { get; set; } = false;
        public bool NoAccidentHistory { get; set; } = false;
        public bool Modifications { get; set; } = false;
    }
}
