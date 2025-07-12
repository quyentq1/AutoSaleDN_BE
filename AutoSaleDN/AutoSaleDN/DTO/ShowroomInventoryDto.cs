namespace AutoSaleDN.DTO
{
    public class ShowroomInventoryDto
    {
        public Dictionary<string, ShowroomDetailsDto> Showrooms { get; set; } = new Dictionary<string, ShowroomDetailsDto>();

    }
}
