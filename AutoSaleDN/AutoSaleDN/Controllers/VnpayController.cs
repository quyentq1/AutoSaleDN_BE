using dsc_backend.DAO;
using AutoSaleDN.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Microsoft.EntityFrameworkCore;
using AutoSaleDN.Services; // Add this for IEmailService
using System.ComponentModel.DataAnnotations; // For [Required] attribute

namespace AutoSaleDN.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Route("[controller]")]
    public class VnpayController : ControllerBase
    {
        private readonly AutoSaleDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IEmailService _emailService; // Inject IEmailService

        public VnpayController(AutoSaleDbContext context, IWebHostEnvironment webHostEnvironment, IEmailService emailService)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _emailService = emailService; // Assign it
        }

        // DTO for Car Sale Payment Request
        [HttpPost("create_payment_url")]
        public IActionResult CreatePaymentUrl([FromBody] PaymentRequest request) // <-- CHANGED from [FromForm] to [FromBody]
        {
            // Set timezone
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            DateTime date = TimeZoneInfo.ConvertTime(DateTime.Now, timeZone);

            string createDate = date.ToString("yyyyMMddHHmmss");
            string orderId = date.ToString("ddHHmmss"); // This is used for vnp_TxnRef

            // Get client IP
            string ipAddr = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";

            // Config params
            string tmnCode = "3AJ5FXBB";
            string secretKey = "GSMYNKXFMYYUDFUCHAVBEJXXLIQZZUED";
            string vnpUrl = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
            string returnUrl = request.returnUrl;
            //string bankCode = "VNBANK";
            string locale = "vn";

            // Build vnp_Params
            var vnpParams = new Dictionary<string, string>
            {
                { "vnp_Version", "2.1.0" },
                { "vnp_Command", "pay" },
                { "vnp_TmnCode", tmnCode },
                { "vnp_Locale", locale },
                { "vnp_CurrCode", "VND" },
                { "vnp_TxnRef", orderId },
                { "vnp_OrderInfo", request.SaleId.ToString() }, // Using request.TournamentId for vnp_OrderInfo
                { "vnp_OrderType", "other" },
                { "vnp_Amount", (request.Amount).ToString() }, // Amount from the request
                { "vnp_ReturnUrl", returnUrl }, // Use the dynamic returnUrl from the request
                { "vnp_IpAddr", ipAddr },
                { "vnp_CreateDate", createDate },
                { "vnp_BankCode", "VNBANK" }
            };


            // Sort params
            var sortedParams = new SortedDictionary<string, string>(vnpParams);

            // Build query string
            var queryBuilder = new StringBuilder();
            foreach (var param in sortedParams)
            {
                if (!string.IsNullOrEmpty(param.Value))
                {
                    queryBuilder.Append(WebUtility.UrlEncode(param.Key));
                    queryBuilder.Append("=");
                    queryBuilder.Append(WebUtility.UrlEncode(param.Value));
                    queryBuilder.Append("&");
                }
            }
            string signData = queryBuilder.ToString().TrimEnd('&');

            // Create HMAC SHA512 signature
            using (var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(secretKey)))
            {
                var signBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(signData));
                var sign = BitConverter.ToString(signBytes).Replace("-", "").ToLower();
                sortedParams.Add("vnp_SecureHash", sign);
            }

            // Build final URL
            var finalUrl = vnpUrl + "?" + string.Join("&",
                sortedParams.Select(p => $"{WebUtility.UrlEncode(p.Key)}={WebUtility.UrlEncode(p.Value)}"));

            return Ok(new { paymentUrl = finalUrl });
        }


        [HttpGet("vnpay_ipn")]
        public IActionResult VnPayIPN([FromQuery] Dictionary<string, string> vnpParams)
        {
            try
            {
                string secureHash = vnpParams.GetValueOrDefault("vnp_SecureHash");
                string bookingName = vnpParams.GetValueOrDefault("vnp_OrderInfo");
                string orderId = vnpParams.GetValueOrDefault("vnp_TxnRef");
                string rspCode = vnpParams.GetValueOrDefault("vnp_ResponseCode");
                string OrderInfo = vnpParams.GetValueOrDefault("vnp_OrderInfo");

                vnpParams.Remove("vnp_SecureHash");
                vnpParams.Remove("vnp_SecureHashType");

                string secretKey = "GSMYNKXFMYYUDFUCHAVBEJXXLIQZZUED";


                var sortedParams = new SortedDictionary<string, string>(vnpParams);
                var signData = string.Join("&", sortedParams
                    .Where(kv => !string.IsNullOrEmpty(kv.Value))
                    .Select(kv => $"{kv.Key}={kv.Value}"));

                using (var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(secretKey)))
                {
                    var signBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(signData));
                    var signed = BitConverter.ToString(signBytes).Replace("-", "").ToLower();

                    if (!string.Equals(signed, secureHash, StringComparison.OrdinalIgnoreCase))
                    {
                        return Ok(new { RspCode = "97", Message = "Invalid Signature" });
                    }

                    string paymentStatus = "0";


                    if (paymentStatus == "0")
                    {
                        if (rspCode == "00")
                        {

                            return Ok(new { RspCode = "00", Message = "Success", SaleId = OrderInfo });
                        }
                        else
                        {
                            // Thanh toán thất bại (Payment failed)
                            // Update your order status in the database to 'Failed' or 'Cancelled'
                            return Ok(new { RspCode = "24", Message = "Failed", SaleId = OrderInfo });
                        }
                    }
                    else
                    {
                        // Đơn hàng đã được cập nhật trạng thái thanh toán (Order status already updated)
                        return Ok(new { RspCode = "02", Message = "This order has been updated to the payment status", SaleId = OrderInfo });
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.Error.WriteLine($"VNPay IPN Error: {ex.Message} - {ex.StackTrace}");
                return StatusCode(500, new { RspCode = "99", Message = "Unknown error occurred on server." }); // Return 99 for unknown error to VNPAY
            }
        }
}
}
