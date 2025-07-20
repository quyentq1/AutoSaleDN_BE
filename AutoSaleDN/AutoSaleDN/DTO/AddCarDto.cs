namespace AutoSaleDN.DTO
{
    public class AddCarDto
    {
        public int ModelId { get; set; }
        public int Year { get; set; }
        public int Mileage { get; set; }
        public decimal Price { get; set; }
        public string Location { get; set; }
        public string Condition { get; set; }
        public string RentSell { get; set; }
        public string Description { get; set; }
        public bool Certified { get; set; }
        public string Vin { get; set; }
        public string Color { get; set; }
        public string InteriorColor { get; set; }
        public string Transmission { get; set; }
        public string Engine { get; set; }
        public string FuelType { get; set; }
        public string CarType { get; set; }
        public int SeatingCapacity { get; set; }
        public decimal RegistrationFee { get; set; }
        public decimal TaxRate { get; set; }
        public int ColorId { get; set; }
        public int QuantityImported { get; set; }
        public DateTime ImportDate { get; set; }
        public decimal ImportPrice { get; set; }
        public string Notes { get; set; }
        public List<string> ImageUrls { get; set; }
        public List<int> FeatureIds { get; set; }
        public int? UserId { get; set; }
    }

}
