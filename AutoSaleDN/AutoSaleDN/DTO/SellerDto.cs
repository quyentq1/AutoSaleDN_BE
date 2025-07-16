namespace AutoSaleDN.DTO
{
    public class SellerDto
    {
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Mobile { get; set; }
        public string Province { get; set; }

        public string? Password { get; set; }

        public int storeLocationId { get; set; }

    }
}
