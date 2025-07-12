namespace AutoSaleDN.DTO
{
    public class TopSellingCarDto
    {
        public int ModelId { get; set; }
        public string ModelName { get; set; }
        public string ManufacturerName { get; set; }
        public string ImageUrl { get; set; }
        public int TotalSold { get; set; }
        public decimal Revenue { get; set; }
        public int AverageRating { get; set; }
        public int TotalReviews { get; set; }
    }
}
