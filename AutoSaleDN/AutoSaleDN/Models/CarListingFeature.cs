namespace AutoSaleDN.Models
{
    public class CarListingFeature
    {
        public int ListingId { get; set; }
        public CarListing Listing { get; set; }
        public int FeatureId { get; set; }
        public CarFeature Feature { get; set; }
    }
}
