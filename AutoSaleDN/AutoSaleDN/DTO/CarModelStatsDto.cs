namespace AutoSaleDN.DTO
{
    public class CarModelStatsDto
    {
        public string ModelName { get; set; }
        public string ManufacturerName { get; set; }
        public int CurrentQuantity { get; set; }
        public int AvailableQuantity { get; set; }
        public decimal AverageCost { get; set; }
        public decimal LastPurchasePrice { get; set; }
        public DateTime? LastImportDate { get; set; }
    }
}
