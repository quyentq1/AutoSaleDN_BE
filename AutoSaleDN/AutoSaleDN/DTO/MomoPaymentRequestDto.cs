using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AutoSaleDN.DTO
{
    // DTO for initiating a Momo payment request from frontend to backend
    public class MomoPaymentRequestDto
    {
        [Required]
        public int SaleId { get; set; } // The ID of the CarSale record
        [Required]
        public decimal Amount { get; set; }
        [Required]
        public string PaymentPurpose { get; set; } = null!; // "deposit" or "full_payment"
        [Required]
        public string ReturnUrl { get; set; } = null!; // URL to redirect back to frontend after payment
    }

    // DTO for the response from backend to frontend (containing Momo's payUrl)
    public class MomoPaymentResponseDto
    {
        public string PayUrl { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string OrderId { get; set; } = null!;
        public string RequestId { get; set; } = null!;
    }

    // DTO for Momo's IPN (Instant Payment Notification) callback
    public class MomoIpnRequestDto
    {
        public string PartnerCode { get; set; } = null!;
        public string OrderId { get; set; } = null!;
        public string RequestId { get; set; } = null!;
        public long Amount { get; set; }
        public string OrderInfo { get; set; } = null!;
        public string OrderType { get; set; } = null!;
        public long TransId { get; set; }
        public string Message { get; set; } = null!;
        public string ResultCode { get; set; } = null!;
        public string PayType { get; set; } = null!;
        public string ResponseTime { get; set; }
        public string ExtraData { get; set; } = null!;
        public string Signature { get; set; } = null!;
        public string? BankCode { get; set; }
        public string? CardNumber { get; set; }
        public string? StoreId { get; set; }
        public string? RequestType { get; set; }
    }

    // DTO for Momo's response to your IPN endpoint (to acknowledge receipt)
    public class MomoIpnResponseDto
    {
        public string PartnerCode { get; set; } = null!;
        public string OrderId { get; set; } = null!;
        public string RequestId { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string ResultCode { get; set; } = null!;
        public string Signature { get; set; } = null!;
    }
}
