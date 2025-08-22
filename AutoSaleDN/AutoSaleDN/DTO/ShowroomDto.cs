namespace AutoSaleDN.DTO
{
    public class ShowroomDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }

        // Financial metrics
        public decimal Revenue { get; set; }
        public decimal RevenueGrowth { get; set; }

        // Inventory metrics
        public int TotalCars { get; set; }

        // Performance metrics
        public int SoldThisMonth { get; set; }

        public int TotalSoldAllTime { get; set; }
        public SellersDto MainSeller { get; set; }
        public List<SellersDto> AllSellers { get; set; }

        // Additional detailed information
        public List<BrandPerformanceDto> Brands { get; set; }
        public List<SalesDataDto> SalesData { get; set; }
        public List<InventoryItemDto> Inventory { get; set; }
        public List<ModelPerformanceDto> PopularModels { get; set; }
    }

    public class SellersDto
    {
        public int? SellerId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class BrandPerformanceDto
    {
        public string Name { get; set; }
        public int Count { get; set; }
        public decimal Revenue { get; set; }
    }

    public class SalesDataDto
    {
        public string Date { get; set; }
        public int Sold { get; set; }
    }

    public class InventoryItemDto
    {
        public string Model { get; set; }
        public string Date { get; set; }
        public int Quantity { get; set; }
        public string Type { get; set; }
    }

    public class ModelPerformanceDto
    {
        public string Name { get; set; }
        public string Brand { get; set; }
        public string ImageUrl { get; set; }
        public int Count { get; set; }
        public int Sold { get; set; }
    }
}