using static AutoSaleDN.Controllers.AdminController;

namespace AutoSaleDN.DTO
{
    public class CarDetailResponseDto
    {
        public int ListingId { get; set; }
        public int ModelId { get; set; }
        public int UserId { get; set; }
        public int Year { get; set; }
        public double Mileage { get; set; }
        public double Price { get; set; }
        public string Condition { get; set; }
        public string RentSell { get; set; }
        public string Description { get; set; }
        public bool Certified { get; set; }
        public string Vin { get; set; }
        public DateTime DatePosted { get; set; }
        public DateTime DateUpdated { get; set; }
        public string ModelName { get; set; }
        public string Manufacturer { get; set; }

        public string Color { get; set; }
        public string InteriorColor { get; set; }
        public string Transmission { get; set; }
        public string Engine { get; set; }
        public string FuelType { get; set; }
        public string CarType { get; set; }
        public int? SeatingCapacity { get; set; }

        public decimal RegistrationFee { get; set; }
        public decimal TaxRate { get; set; }

        public List<string> ImageUrl { get; set; }

        public List<string> VideoUrl { get; set; }

        public List<CarFeatureDto> Features { get; set; }

        // From StoreListings
        public List<CarInventoryDto> Showrooms { get; set; }

        // Derived status (can be calculated on frontend or backend)
        public string Status { get; set; }
        public int? AvailableUnits { get; set; }
    }
}
