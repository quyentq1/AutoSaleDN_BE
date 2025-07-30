namespace dsc_backend.DAO
{
    public class PaymentRequest
    {
        public decimal Amount { get; set; }
        public int SaleId { get; set; }

        public string returnUrl { get; set; }
    }
}
