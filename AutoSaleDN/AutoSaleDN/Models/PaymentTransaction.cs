using System.ComponentModel.DataAnnotations;

namespace AutoSaleDN.Models
{
    public class PaymentTransaction
    {
        [Key]
        public int TransactionLogId { get; set; }
        public int PaymentId { get; set; }
        public Payment Payment { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.Now;
        public string? GatewayResponseCode { get; set; }
        public string? GatewayResponseMessage { get; set; }
        public string? TransactionStatus { get; set; }
        public string? AdditionalDetails { get; set; }
    }
}
