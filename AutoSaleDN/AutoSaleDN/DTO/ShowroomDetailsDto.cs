namespace AutoSaleDN.DTO
{
    public class ShowroomDetailsDto
    {
        public int TotalCars { get; set; }
        public int AvailableCars { get; set; }
        public List<CarBrandStatsDto> Brands { get; set; } = new List<CarBrandStatsDto>();
        public List<CarModelStatsDto> Models { get; set; } = new List<CarModelStatsDto>();
    }
}
