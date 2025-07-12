namespace AutoSaleDN.DTO
{
    public class ShowroomDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public int TotalCars { get; set; }
        public int SoldThisMonth { get; set; }
        public decimal Revenue { get; set; }
        public decimal RevenueGrowth { get; set; }
    }
}
