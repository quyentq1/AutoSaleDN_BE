using AutoSaleDN.DTO;
using AutoSaleDN.Models; // For AutoSaleDbContext
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace AutoSaleDN.Services
{
    public class MomoService : IMomoService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly AutoSaleDbContext _context;
        private readonly IEmailService _emailService; // For sending emails

        public MomoService(IConfiguration configuration, HttpClient httpClient, AutoSaleDbContext context, IEmailService emailService)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _context = context;
            _emailService = emailService;
        }

        public async Task<MomoPaymentResponseDto> CreatePaymentAsync(MomoPaymentRequestDto request)
        {
            var momoConfig = _configuration.GetSection("MomoSettings");
            string partnerCode = momoConfig["PartnerCode"] ?? throw new ArgumentNullException("MomoSettings:PartnerCode is not configured.");
            string accessKey = momoConfig["AccessKey"] ?? throw new ArgumentNullException("MomoSettings:AccessKey is not configured.");
            string secretKey = momoConfig["SecretKey"] ?? throw new ArgumentNullException("MomoSettings:SecretKey is not configured.");
            string createPaymentUrl = momoConfig["MoMoApiURL"] ?? throw new ArgumentNullException("MomoSettings:MoMoApiURL is not configured.");
            string notifyUrl = momoConfig["NotifyUrl"] ?? throw new ArgumentNullException("MomoSettings:NotifyUrl is not configured.");

            // Generate unique IDs for Momo
            string requestId = Guid.NewGuid().ToString();
            string orderId = $"{request.SaleId}-{request.PaymentPurpose}-{Guid.NewGuid().ToString().Substring(0, 4)}"; // Combine SaleId, purpose, and unique part

            // ExtraData for internal use (e.g., to identify SaleId and purpose in IPN)
            string extraData = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{request.SaleId}|{request.PaymentPurpose}"));

            // Amount needs to be in Long (VND, no decimals)
            var amount = request.Amount.ToString();

            var rawHash = $"partnerCode={partnerCode}&accessKey={accessKey}&requestId={requestId}&amount={amount}&orderId={orderId}&orderInfo={request.PaymentPurpose} for Sale {request.SaleId}&returnUrl={request.ReturnUrl}&notifyUrl={notifyUrl}&extraData={extraData}";

            var signature = GenerateHmacSha256(rawHash, secretKey);

            var momoRequest = new
            {
                partnerCode = partnerCode,
                accessKey = accessKey,
                requestId = requestId,
                amount = amount,
                orderId = orderId,
                orderInfo = $"{request.PaymentPurpose} for Sale {request.SaleId}",
                returnUrl = request.ReturnUrl,
                notifyUrl = notifyUrl,
                extraData = extraData,
                requestType = "captureMoMoWallet", // Or "payWithATM" etc.
                signature = signature,
                lang = "en"
            };

            var jsonContent = new StringContent(JsonConvert.SerializeObject(momoRequest), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(createPaymentUrl, jsonContent);
            response.EnsureSuccessStatusCode(); // Throws an exception if the HTTP response status is an error code

            var responseString = await response.Content.ReadAsStringAsync();
            var momoResponse = JsonConvert.DeserializeObject<dynamic>(responseString);

            if (momoResponse?.payUrl != null)
            {
                return new MomoPaymentResponseDto
                {
                    PayUrl = momoResponse.payUrl,
                    Message = momoResponse.message,
                    OrderId = orderId,
                    RequestId = requestId
                };
            }
            else
            {
                throw new Exception($"Momo API error: {momoResponse?.message ?? "Unknown error"}");
            }
        }

        public async Task<MomoIpnResponseDto> ProcessMomoCallbackAsync(MomoIpnRequestDto ipnRequest)
        {
            var momoConfig = _configuration.GetSection("MomoSettings");
            string partnerCode = momoConfig["PartnerCode"] ?? throw new ArgumentNullException("MomoSettings:PartnerCode is not configured.");
            string accessKey = momoConfig["AccessKey"] ?? throw new ArgumentNullException("MomoSettings:AccessKey is not configured.");
            string secretKey = momoConfig["SecretKey"] ?? throw new ArgumentNullException("MomoSettings:SecretKey is not configured.");

            // Verify signature
            var rawHash = $"accessKey={accessKey}" +
                              $"&amount={ipnRequest.Amount}" +
                              $"&extraData={ipnRequest.ExtraData}" +
                              $"&message={ipnRequest.Message}" +
                              $"&orderId={ipnRequest.OrderId}" +
                              $"&orderInfo={ipnRequest.OrderInfo}" +
                              $"&orderType={ipnRequest.OrderType}" +
                              $"&partnerCode={ipnRequest.PartnerCode}" +
                              $"&payType={ipnRequest.PayType}" +
                              $"&requestId={ipnRequest.RequestId}" +
                              $"&responseTime={ipnRequest.ResponseTime}" + // Đảm bảo là chuỗi ngày giờ, không phải timestamp
                              $"&transId={ipnRequest.TransId}"; 
            var expectedSignature = GenerateHmacSha256(rawHash, secretKey);

            //if (!expectedSignature.Equals(ipnRequest.Signature, StringComparison.OrdinalIgnoreCase))
            //{
            //    return new MomoIpnResponseDto
            //    {
            //        PartnerCode = partnerCode,
            //        OrderId = ipnRequest.OrderId,
            //        RequestId = ipnRequest.RequestId,
            //        Message = "Invalid signature",
            //        ResultCode = "99" // Custom error code for invalid signature
            //    };
            //}

            // Extract SaleId and PaymentPurpose from ExtraData
            string decodedExtraData = Encoding.UTF8.GetString(Convert.FromBase64String(ipnRequest.ExtraData));
            var extraDataParts = decodedExtraData.Split('|');
            if (extraDataParts.Length < 2)
            {
                return new MomoIpnResponseDto
                {
                    PartnerCode = partnerCode,
                    OrderId = ipnRequest.OrderId,
                    RequestId = ipnRequest.RequestId,
                    Message = "Invalid ExtraData format",
                    ResultCode = "99"
                };
            }
            int saleId = int.Parse(extraDataParts[0]);
            string paymentPurpose = extraDataParts[1];

            // Find the CarSale record
            var carSale = await _context.CarSales
                                        .Include(cs => cs.StoreListing)
                                            .ThenInclude(sl => sl.CarListing)
                                                .ThenInclude(cl => cl.Model)
                                                    .ThenInclude(cm => cm.CarManufacturer)
                                        .Include(cs => cs.Customer) // Include customer for email
                                        .FirstOrDefaultAsync(cs => cs.SaleId == saleId);

            if (carSale == null)
            {
                return new MomoIpnResponseDto
                {
                    PartnerCode = partnerCode,
                    OrderId = ipnRequest.OrderId,
                    RequestId = ipnRequest.RequestId,
                    Message = "Order not found",
                    ResultCode = "01" // Custom error code for order not found
                };
            }

            // Check if the payment has already been processed for this purpose
            bool alreadyProcessed = false;
            if (paymentPurpose == "deposit" && carSale.DepositPaymentId != null)
            {
                var existingDepositPayment = await _context.Payments.FindAsync(carSale.DepositPaymentId);
                if (existingDepositPayment != null && existingDepositPayment.PaymentStatus == "completed")
                {
                    alreadyProcessed = true;
                }
            }
            else if (paymentPurpose == "full_payment" && carSale.FullPaymentId != null)
            {
                var existingFullPayment = await _context.Payments.FindAsync(carSale.FullPaymentId);
                if (existingFullPayment != null && existingFullPayment.PaymentStatus == "completed")
                {
                    alreadyProcessed = true;
                }
            }

            if (alreadyProcessed)
            {
                return new MomoIpnResponseDto
                {
                    PartnerCode = partnerCode,
                    OrderId = ipnRequest.OrderId,
                    RequestId = ipnRequest.RequestId,
                    Message = "This order has been updated to the payment status",
                    ResultCode = "02" // Custom error code for already processed
                };
            }


            // Process based on Momo ResultCode
            if (ipnRequest.ResultCode == "0") // "0" means successful payment
            {
                string paymentStatusName = "";
                if (paymentPurpose == "deposit")
                {
                    paymentStatusName = "Deposit Paid";
                }
                else if (paymentPurpose == "full_payment")
                {
                    paymentStatusName = "Payment Complete";
                }

                var newSaleStatus = await _context.SaleStatus.FirstOrDefaultAsync(s => s.StatusName == paymentStatusName);
                if (newSaleStatus == null)
                {
                    return new MomoIpnResponseDto
                    {
                        PartnerCode = partnerCode,
                        OrderId = ipnRequest.OrderId,
                        RequestId = ipnRequest.RequestId,
                        Message = $"Sale status '{paymentStatusName}' not found in database.",
                        ResultCode = "99"
                    };
                }

                // Create Payment record
                var payment = new Payment
                {
                    UserId = carSale.CustomerId,
                    ListingId = carSale.StoreListing.ListingId,
                    PaymentForSaleId = carSale.SaleId,
                    TransactionId = ipnRequest.TransId.ToString(), // Momo's transaction ID
                    Amount = ipnRequest.Amount / 1.0M, // Convert from long (VND) to decimal
                    PaymentMethod = "e_wallet_momo_test",
                    PaymentStatus = "completed",
                    PaymentPurpose = paymentPurpose,
                    DateOfPayment = DateTime.UtcNow, // Use current UTC time for consistency
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                // Update CarSale record
                if (paymentPurpose == "deposit")
                {
                    carSale.DepositPaymentId = payment.PaymentId;
                    carSale.SaleStatusId = newSaleStatus.SaleStatusId;
                    carSale.RemainingBalance = carSale.FinalPrice - (carSale.DepositAmount ?? 0);
                }
                else if (paymentPurpose == "full_payment")
                {
                    carSale.FullPaymentId = payment.PaymentId;
                    carSale.SaleStatusId = newSaleStatus.SaleStatusId;
                    carSale.RemainingBalance = 0;
                }
                carSale.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // --- Send Email Confirmation ---
                var customerEmail = carSale.Customer?.Email;
                var customerFullName = carSale.Customer?.FullName;
                var carName = $"{carSale.StoreListing.CarListing.Model.CarManufacturer.Name} {carSale.StoreListing.CarListing.Model.Name}";
                var paidAmountFormatted = (ipnRequest.Amount / 1.0M).ToString("N0") + " VND";

                string emailSubject = "";
                string emailBody = "";

                if (paymentPurpose == "deposit")
                {
                    DateTime? paymentDueDateForEmail = carSale.ExpectedDeliveryDate?.AddDays(-10);

                    emailSubject = $"Deposit Confirmation for Car {carName} - Order #{carSale.OrderNumber}";
                    emailBody = $@"
                        <!DOCTYPE html>
                        <html lang=""en"">
                        <head>
                            <meta charset=""UTF-8"">
                            <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                            <title>Deposit Confirmation</title>
                            <style>
                                body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0; }}
                                .container {{ width: 100%; max-width: 600px; margin: 20px auto; background-color: #ffffff; border-radius: 8px; box-shadow: 0 4px 8px rgba(0,0,0,0.05); overflow: hidden; }}
                                .header {{ background-color: #4CAF50; color: #ffffff; padding: 20px; text-align: center; }}
                                .header h1 {{ margin: 0; font-size: 24px; }}
                                .content {{ padding: 20px 30px; line-height: 1.6; color: #333333; }}
                                .content p {{ margin-bottom: 15px; }}
                                .highlight {{ background-color: #e8f5e9; padding: 15px; border-left: 5px solid #4CAF50; margin: 20px 0; border-radius: 4px; }}
                                .highlight ul {{ list-style: none; padding: 0; margin: 0; }}
                                .highlight ul li {{ margin-bottom: 8px; }}
                                .highlight ul li strong {{ color: #2e7d32; }}
                                .footer {{ background-color: #f0f0f0; padding: 15px; text-align: center; font-size: 12px; color: #666666; border-top: 1px solid #e0e0e0; }}
                            </style>
                        </head>
                        <body>
                            <div class=""container"">
                                <div class=""header"">
                                    <h1>Deposit Payment Successful!</h1>
                                </div>
                                <div class=""content"">
                                    <p>Dear <strong>{customerFullName}</strong>,</p>
                                    <p>We are pleased to confirm that your deposit payment for order <strong>#{carSale.OrderNumber}</strong> has been successfully processed via Momo.</p>

                                    <div class=""highlight"">
                                        <p><strong>Your Deposit Details:</strong></p>
                                        <ul>
                                            <li><strong>Vehicle:</strong> {carName}</li>
                                            <li><strong>Order Number:</strong> <strong>{carSale.OrderNumber}</strong></li>
                                            <li><strong>Deposit Amount:</strong> <strong>{paidAmountFormatted}</strong></li>
                                            <li><strong>Payment Method:</strong> E-Wallet (Momo)</li>
                                            <li><strong>Deposit Date:</strong> {payment.DateOfPayment.ToString("dd/MM/yyyy HH:mm")}</li>
                                            <li><strong>Total Vehicle Value:</strong> {carSale.FinalPrice.ToString("N0")} VND</li>
                                            <li><strong>Remaining Balance:</strong> {carSale.RemainingBalance?.ToString("N0") ?? "0"} VND</li>
                                            <li><strong>Estimated Delivery Date:</strong> {carSale.ExpectedDeliveryDate?.ToString("dd/MM/yyyy") ?? "N/A"}</li>
                                            <li><strong>Full Payment Due Date:</strong> {paymentDueDateForEmail?.ToString("dd/MM/yyyy") ?? "N/A"}</li>
                                        </ul>
                                    </div>

                                    <p>Our sales team will contact you shortly to finalize the purchase agreement and remaining payment procedures.</p>
                                    <p>If you have any questions, please do not hesitate to contact us.</p>
                                    <p>Sincerely,</p>
                                    <p><strong>AutoSaleDN Team</strong></p>
                                </div>
                                <div class=""footer"">
                                    <p>&copy; {DateTime.Now.Year} AutoSaleDN. All rights reserved.</p>
                                </div>
                            </div>
                        </body>
                        </html>
                    ";
                }
                else if (paymentPurpose == "full_payment")
                {
                    emailSubject = $"Full Payment Confirmation for Car {carName} - Order #{carSale.OrderNumber}";
                    emailBody = $@"
                        <!DOCTYPE html>
                        <html lang=""en"">
                        <head>
                            <meta charset=""UTF-8"">
                            <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                            <title>Full Payment Confirmation</title>
                            <style>
                                body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0; }}
                                .container {{ width: 100%; max-width: 600px; margin: 20px auto; background-color: #ffffff; border-radius: 8px; box-shadow: 0 4px 8px rgba(0,0,0,0.05); overflow: hidden; }}
                                .header {{ background-color: #007bff; color: #ffffff; padding: 20px; text-align: center; }}
                                .header h1 {{ margin: 0; font-size: 24px; }}
                                .content {{ padding: 20px 30px; line-height: 1.6; color: #333333; }}
                                .content p {{ margin-bottom: 15px; }}
                                .highlight {{ background-color: #e0f2f7; padding: 15px; border-left: 5px solid #007bff; margin: 20px 0; border-radius: 4px; }}
                                .highlight ul {{ list-style: none; padding: 0; margin: 0; }}
                                .highlight ul li {{ margin-bottom: 8px; }}
                                .highlight ul li strong {{ color: #0056b3; }}
                                .footer {{ background-color: #f0f0f0; padding: 15px; text-align: center; font-size: 12px; color: #666666; border-top: 1px solid #e0e0e0; }}
                            </style>
                        </head>
                        <body>
                            <div class=""container"">
                                <div class=""header"">
                                    <h1>Full Payment Successful!</h1>
                                </div>
                                <div class=""content"">
                                    <p>Dear <strong>{customerFullName}</strong>,</p>
                                    <p>We are pleased to confirm that your full payment for order <strong>#{carSale.OrderNumber}</strong> has been successfully processed via Momo.</p>

                                    <div class=""highlight"">
                                        <p><strong>Your Full Payment Details:</strong></p>
                                        <ul>
                                            <li><strong>Vehicle:</strong> {carName}</li>
                                            <li><strong>Order Number:</strong> <strong>{carSale.OrderNumber}</strong></li>
                                            <li><strong>Payment Amount:</strong> <strong>{paidAmountFormatted}</strong></li>
                                            <li><strong>Payment Method:</strong> E-Wallet (Momo)</li>
                                            <li><strong>Payment Date:</strong> {payment.DateOfPayment.ToString("dd/MM/yyyy HH:mm")}</li>
                                            <li><strong>Total Vehicle Value:</strong> {carSale.FinalPrice.ToString("N0")} VND</li>
                                            <li><strong>Remaining Balance:</strong> {carSale.RemainingBalance?.ToString("N0") ?? "0"} VND (Now 0)</li>
                                            <li><strong>Estimated Delivery Date:</strong> {carSale.ExpectedDeliveryDate?.ToString("dd/MM/yyyy") ?? "N/A"}</li>
                                            <li><strong>Actual Delivery Date:</strong> {carSale.ActualDeliveryDate?.ToString("dd/MM/yyyy") ?? "Pending Confirmation"}</li>
                                        </ul>
                                    </div>

                                    <p>Your order is now fully paid. Our team will contact you regarding the delivery or pickup of your vehicle.</p>
                                    <p>If you have any questions, please do not hesitate to contact us.</p>
                                    <p>Sincerely,</p>
                                    <p><strong>AutoSaleDN Team</strong></p>
                                </div>
                                <div class=""footer"">
                                    <p>&copy; {DateTime.Now.Year} AutoSaleDN. All rights reserved.</p>
                                </div>
                            </div>
                        </body>
                        </html>
                    ";
                }

                if (!string.IsNullOrEmpty(customerEmail))
                {
                    try
                    {
                        await _emailService.SendEmailAsync(customerEmail, emailSubject, emailBody);
                        Console.WriteLine($"Email sent successfully to: {customerEmail}");
                    }
                    catch (Exception emailEx)
                    {
                        Console.WriteLine($"Failed to send email to {customerEmail}: {emailEx.Message}");
                        // Log this error to a dedicated logging system
                    }
                }

                return new MomoIpnResponseDto
                {
                    PartnerCode = partnerCode,
                    OrderId = ipnRequest.OrderId,
                    RequestId = ipnRequest.RequestId,
                    Message = "Success",
                    ResultCode = "0"
                };
            }
            else
            {
                // Payment failed or cancelled
                // Update CarSale status to reflect failure or cancellation
                var failedStatus = await _context.SaleStatus.FirstOrDefaultAsync(s => s.StatusName == "Payment Failed");
                if (failedStatus != null)
                {
                    carSale.SaleStatusId = failedStatus.SaleStatusId;
                    carSale.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                return new MomoIpnResponseDto
                {
                    PartnerCode = partnerCode,
                    OrderId = ipnRequest.OrderId,
                    RequestId = ipnRequest.RequestId,
                    Message = ipnRequest.Message,
                    ResultCode = ipnRequest.ResultCode
                };
            }
        }

        private string GenerateHmacSha256(string message, string key)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var messageBytes = Encoding.UTF8.GetBytes(message);

            byte[] hashBytes;

            using (var hmac = new HMACSHA256(keyBytes))
            {
                hashBytes = hmac.ComputeHash(messageBytes);
            }

            var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

            return hashString;
        }
    }
}
