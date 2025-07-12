namespace AutoSaleDN.DTO
{
    public class CarBrandStatsDto
    {
        public string BrandName { get; set; }
        public int TotalCars { get; set; }
        public int AvailableCars { get; set; }
        public decimal AverageCost { get; set; }
        public decimal LastPurchasePrice { get; set; }
    }
}
