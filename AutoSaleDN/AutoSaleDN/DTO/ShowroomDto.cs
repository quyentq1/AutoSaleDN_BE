namespace AutoSaleDN.DTO
{
    public class ShowroomDto
    {
        public int Id { get; set; }

        public int SellerId { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public decimal Revenue { get; set; }
        public decimal RevenueGrowth { get; set; }
        public int TotalCars { get; set; }
        public int SoldThisMonth { get; set; }

        public string SellerName { get; set; }
        public List<BrandPerformanceDto> Brands { get; set; }
        public List<SalesDataDto> SalesData { get; set; }
        public List<InventoryItemDto> Inventory { get; set; }
        public List<ModelPerformanceDto> Models { get; set; }
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
