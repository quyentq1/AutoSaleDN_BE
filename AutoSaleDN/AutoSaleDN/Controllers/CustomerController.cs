using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoSaleDN.Models;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using AutoSaleDN.Services;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace AutoSaleDN.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Customer")] // Ensure only authenticated customers can access
    public class CustomerController : ControllerBase
    {
        private readonly AutoSaleDbContext _context;
        private readonly IEmailService _emailService;
        public CustomerController(AutoSaleDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // Helper method to get current authenticated UserId
        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                throw new UnauthorizedAccessException("User ID claim not found.");
            }
            return int.Parse(userIdClaim);
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = GetUserId();
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return NotFound("User not found.");
                return Ok(user);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] User model)
        {
            try
            {
                var userId = GetUserId();
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return NotFound("User not found.");

                user.FullName = model.FullName;
                user.Email = model.Email; // Consider if email should be editable
                user.Mobile = model.Mobile;
                user.Province = model.Province;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return Ok(new { message = "Profile updated successfully." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto) // Sử dụng DTO chung
        {
            try
            {
                var userId = GetUserId();
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return NotFound("User not found.");

                if (!BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.Password))
                    return BadRequest(new { message = "Old password incorrect." });

                user.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Password changed successfully." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpGet("{showroom_id}/seller")]
        public async Task<ActionResult<User>> GetSellerByShowroomId(int showroom_id)
        {
            try
            {
                // Find a user (seller) associated with the given StoreLocationId
                // Assuming that a User with Role "Seller" will have their StoreLocationId set.
                var seller = await _context.Users
                                           .Include(u => u.StoreLocation) // Include StoreLocation if needed for details
                                           .FirstOrDefaultAsync(u => u.StoreLocationId == showroom_id && u.Role == "Seller");

                if (seller == null)
                {
                    // If no specific seller is found, try to find the showroom itself
                    var showroom = await _context.StoreLocations.FindAsync(showroom_id);
                    if (showroom == null)
                    {
                        return NotFound($"Showroom with ID '{showroom_id}' not found.");
                    }
                    // If showroom exists but no seller is explicitly linked, return a generic message
                    return NotFound($"No seller found for showroom with ID '{showroom_id}'.");
                }

                // Return only necessary seller information, avoid sensitive data
                return Ok(new
                {
                    seller.UserId,
                    seller.FullName,
                    seller.Email,
                    seller.Mobile,
                    seller.StoreLocationId,
                    ShowroomName = seller.StoreLocation?.Name, // Include showroom name
                    ShowroomAddress = seller.StoreLocation?.Address // Include showroom address
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }


        [HttpGet("addresses")]
        public async Task<IActionResult> GetAddresses()
        {
            try
            {
                var userId = GetUserId();
                var addresses = await _context.DeliveryAddresses
                                            .Where(a => a.UserId == userId)
                                            .OrderByDescending(a => a.IsDefault) // Prioritize default address
                                            .ToListAsync();
                return Ok(addresses);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpPost("addresses")]
        public async Task<IActionResult> AddAddress([FromBody] DeliveryAddress model)
        {
            try
            {
                var userId = GetUserId();
                model.UserId = userId;
                model.CreatedAt = DateTime.UtcNow;
                model.UpdatedAt = DateTime.UtcNow;

                // If setting as default, ensure other addresses for this user are not default
                if (model.IsDefault)
                {
                    var existingDefault = await _context.DeliveryAddresses
                                                        .FirstOrDefaultAsync(da => da.UserId == userId && da.IsDefault);
                    if (existingDefault != null)
                    {
                        existingDefault.IsDefault = false;
                        _context.DeliveryAddresses.Update(existingDefault);
                    }
                }

                _context.DeliveryAddresses.Add(model);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Address added successfully.", addressId = model.AddressId });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpPut("addresses/{id}")]
        public async Task<IActionResult> UpdateAddress(int id, [FromBody] DeliveryAddress model)
        {
            try
            {
                var userId = GetUserId();
                var address = await _context.DeliveryAddresses.FirstOrDefaultAsync(a => a.AddressId == id && a.UserId == userId);
                if (address == null) return NotFound("Address not found or unauthorized.");

                address.Address = model.Address;
                address.Note = model.Note;
                address.RecipientName = model.RecipientName;
                address.RecipientPhone = model.RecipientPhone;
                address.AddressType = model.AddressType; // Update AddressType
                address.UpdatedAt = DateTime.UtcNow;

                // Handle setting/unsetting as default
                if (model.IsDefault && !address.IsDefault) // If new model is default and current is not
                {
                    var existingDefault = await _context.DeliveryAddresses
                                                        .FirstOrDefaultAsync(da => da.UserId == userId && da.IsDefault);
                    if (existingDefault != null)
                    {
                        existingDefault.IsDefault = false;
                        _context.DeliveryAddresses.Update(existingDefault);
                    }
                    address.IsDefault = true;
                }
                else if (!model.IsDefault && address.IsDefault) // If new model is not default and current is
                {
                    address.IsDefault = false;
                }

                _context.DeliveryAddresses.Update(address);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Address updated successfully." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpDelete("addresses/{id}")]
        public async Task<IActionResult> DeleteAddress(int id)
        {
            try
            {
                var userId = GetUserId();
                var address = await _context.DeliveryAddresses.FirstOrDefaultAsync(a => a.AddressId == id && a.UserId == userId);
                if (address == null) return NotFound("Address not found or unauthorized.");

                _context.DeliveryAddresses.Remove(address);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Address deleted successfully." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }


        // NEW API: Create a deposit order
        [HttpPost("orders/deposit")]
        public async Task<IActionResult> CreateDepositOrder([FromBody] CreateDepositOrderDto dto)
        {
            try
            {
                var userId = GetUserId();

                // Validate DTO input
                if (dto.ListingId <= 0 || dto.DepositAmount <= 0)
                {
                    return BadRequest(new { message = "Invalid listing ID or deposit amount." });
                }

                // Get SaleStatus for "Pending Deposit"
                var pendingDepositStatus = await _context.SaleStatus.FirstOrDefaultAsync(s => s.StatusName == "Pending Deposit");
                if (pendingDepositStatus == null)
                {
                    return StatusCode(500, new { message = "Sale status 'Pending Deposit' not found in database. Please ensure it is seeded." });
                }

                // Get CarListing and StoreListing details
                var storeListing = await _context.StoreListings
                                                 .Include(sl => sl.CarListing)
                                                 .FirstOrDefaultAsync(sl => sl.ListingId == dto.ListingId && sl.StoreLocationId == dto.SelectedShowroomId);

                if (storeListing == null)
                {
                    return NotFound(new { message = "Car listing or selected showroom not found." });
                }

                // Create a unique OrderNumber (you might have a more sophisticated generation logic)
                var orderNumber = $"ORD-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";

                // Create CarSale (Order) record
                var carSale = new CarSale
                {
                    OrderNumber = orderNumber,
                    StoreListingId = storeListing.StoreListingId,
                    CustomerId = userId,
                    SaleStatusId = pendingDepositStatus.SaleStatusId, // Initially set to Pending Deposit
                    FinalPrice = dto.TotalPrice,
                    DepositAmount = dto.DepositAmount,
                    RemainingBalance = dto.TotalPrice - dto.DepositAmount,
                    DeliveryOption = dto.DeliveryOption,
                    OrderType = "deposit_first",
                    ExpectedDeliveryDate = dto.ExpectedDeliveryDate, // From frontend DTO
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Handle delivery address
                if (dto.DeliveryOption == "shipping")
                {
                    if (dto.UseUserProfileAddress)
                    {
                        var userAddress = await _context.DeliveryAddresses.FirstOrDefaultAsync(da => da.UserId == userId && da.IsDefault);
                        if (userAddress == null)
                        {
                            return BadRequest(new { message = "User profile address not found. Please add a default address or provide a new shipping address." });
                        }
                        carSale.ShippingAddressId = userAddress.AddressId;
                    }
                    else if (dto.ShippingAddressInfo != null)
                    {
                        var newShippingAddress = new DeliveryAddress
                        {
                            UserId = userId,
                            Address = dto.ShippingAddressInfo.Address,
                            RecipientName = dto.ShippingAddressInfo.Name,
                            RecipientPhone = dto.ShippingAddressInfo.Phone,
                            AddressType = "other_shipping",
                            IsDefault = false,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        _context.DeliveryAddresses.Add(newShippingAddress);
                        await _context.SaveChangesAsync();
                        carSale.ShippingAddressId = newShippingAddress.AddressId;
                    }
                    else
                    {
                        return BadRequest(new { message = "Shipping address information is missing." });
                    }
                }
                else if (dto.DeliveryOption == "pickup")
                {
                    carSale.PickupStoreLocationId = dto.SelectedShowroomId;
                }

                _context.CarSales.Add(carSale);
                await _context.SaveChangesAsync(); // Save CarSale to get SaleId

                // Create Payment record for deposit
                // Set PaymentStatus to "pending" if using external gateway like Momo/VNPay
                var depositPayment = new Payment
                {
                    UserId = userId,
                    ListingId = dto.ListingId,
                    PaymentForSaleId = carSale.SaleId, // Link to the new CarSale record
                    TransactionId = $"DEP-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                    Amount = dto.DepositAmount,
                    PaymentMethod = dto.DepositPaymentMethod,
                    PaymentStatus = (dto.DepositPaymentMethod == "e_wallet_momo_test" || dto.DepositPaymentMethod == "e_wallet_vnpay_test") ? "pending" : "completed", // Set to pending if external gateway
                    PaymentPurpose = "deposit",
                    DateOfPayment = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Payments.Add(depositPayment);
                await _context.SaveChangesAsync(); // Save Payment to get PaymentId

                // Update CarSale with DepositPaymentId
                carSale.DepositPaymentId = depositPayment.PaymentId;
                carSale.UpdatedAt = DateTime.UtcNow; // Update timestamp
                await _context.SaveChangesAsync(); // Save updated CarSale

                // --- NO EMAIL/SMS HERE FOR MOMO/VNPAY. IT WILL BE SENT FROM MOMO/VNPAY CALLBACK ---
                // For other payment methods (e.g., bank transfer, installment plan), send email/SMS directly
                if (dto.DepositPaymentMethod != "e_wallet_momo_test" && dto.DepositPaymentMethod != "e_wallet_vnpay_test")
                {
                    // Update CarSale status to "Deposit Paid" for non-gateway payments
                    var depositPaidStatus = await _context.SaleStatus.FirstOrDefaultAsync(s => s.StatusName == "Deposit Paid");
                    if (depositPaidStatus == null)
                    {
                        return StatusCode(500, new { message = "Sale status 'Deposit Paid' not found in database. Please ensure it is seeded." });
                    }
                    carSale.SaleStatusId = depositPaidStatus.SaleStatusId;
                    carSale.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync(); // Save updated CarSale with new status

                    // Fetch necessary details for email/SMS after saving carSale and depositPayment
                    var fullCarSale = await _context.CarSales
                        .Include(cs => cs.Customer)
                        .Include(cs => cs.StoreListing).ThenInclude(sl => sl.CarListing).ThenInclude(cl => cl.Model).ThenInclude(cm => cm.CarManufacturer)
                        .Include(cs => cs.DepositPayment)
                        .FirstOrDefaultAsync(cs => cs.SaleId == carSale.SaleId);

                    if (fullCarSale == null)
                    {
                        return StatusCode(500, new { message = "Failed to retrieve full order details for notification." });
                    }

                    var customerEmail = fullCarSale.Customer?.Email;
                    var customerFullName = fullCarSale.Customer?.FullName;
                    var customerMobile = fullCarSale.Customer?.Mobile;
                    var carName = $"{fullCarSale.StoreListing.CarListing.Model.CarManufacturer.Name} {fullCarSale.StoreListing.CarListing.Model.Name}";
                    var depositAmountFormatted = fullCarSale.DepositAmount + " VND";
                    DateTime? paymentDueDateForEmail = fullCarSale.ExpectedDeliveryDate?.AddDays(-10);

                    var emailSubject = $"Deposit Confirmation for Car {carName} - Order #{fullCarSale.OrderNumber}";
                    var emailBody = $@"
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
                                    <p>We are pleased to confirm that your deposit payment for order <strong>#{fullCarSale.OrderNumber}</strong> has been successfully processed.</p>

                                    <div class=""highlight"">
                                        <p><strong>Your Deposit Details:</strong></p>
                                        <ul>
                                            <li><strong>Vehicle:</strong> {carName}</li>
                                            <li><strong>Order Number:</strong> <strong>{fullCarSale.OrderNumber}</strong></li>
                                            <li><strong>Deposit Amount:</strong> <strong>{depositAmountFormatted}</strong></li>
                                            <li><strong>Payment Method:</strong> {fullCarSale.DepositPayment?.PaymentMethod ?? "N/A"}</li>
                                            <li><strong>Deposit Date:</strong> {fullCarSale.DepositPayment?.DateOfPayment.ToString("dd/MM/yyyy HH:mm") ?? "N/A"}</li>
                                            <li><strong>Total Vehicle Value:</strong> {fullCarSale.FinalPrice.ToString("N0")} VND</li>
                                            <li><strong>Remaining Balance:</strong> {fullCarSale.RemainingBalance?.ToString("N0") ?? "0"} VND</li>
                                            <li><strong>Estimated Delivery Date:</strong> {fullCarSale.ExpectedDeliveryDate?.ToString("dd/MM/yyyy") ?? "N/A"}</li>
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
                        }
                    }

                    if (!string.IsNullOrEmpty(customerMobile))
                    {
                        var smsMessage = $"AutoSaleDN: Order #{fullCarSale.OrderNumber} deposit {depositAmountFormatted} successful. Remaining: {fullCarSale.RemainingBalance?.ToString("N0")} VND. Est. Delivery: {fullCarSale.ExpectedDeliveryDate?.ToString("dd/MM/yyyy")}. Full Payment Due: {paymentDueDateForEmail?.ToString("dd/MM/yyyy")}.";
                        Console.WriteLine($"Simulating SMS to: {customerMobile}");
                        Console.WriteLine($"Message: {smsMessage}");
                    }
                }

                DateTime? paymentDueDate = carSale.ExpectedDeliveryDate?.AddDays(-10);

                return Ok(new
                {
                    message = "Deposit order created successfully.",
                    orderId = carSale.SaleId,
                    orderNumber = carSale.OrderNumber,
                    depositPaymentId = depositPayment.PaymentId,
                    expectedDeliveryDate = carSale.ExpectedDeliveryDate,
                    paymentDueDate = paymentDueDate,
                    paymentGatewayRedirect = (dto.DepositPaymentMethod == "e_wallet_momo_test" || dto.DepositPaymentMethod == "e_wallet_vnpay_test") // Indicate if frontend needs to redirect
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CreateDepositOrder: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        // NEW API: Process full payment for an existing order
        [HttpPost("orders/full-payment")]
        public async Task<IActionResult> ProcessFullPayment(int orderId, [FromBody] ProcessFullPaymentDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return BadRequest(new { message = "Payment data is required." });
                }

                var userId = GetUserId();

                var carSale = await _context.CarSales
                                            .Include(cs => cs.SaleStatus)
                                            .Include(cs => cs.StoreListing)
                                            .FirstOrDefaultAsync(cs => cs.SaleId == orderId && cs.CustomerId == userId);

                if (carSale == null)
                {
                    return NotFound(new { message = "Order not found or unauthorized." });
                }

                // Kiểm tra trạng thái đơn hàng - chỉ cho phép thanh toán nếu chưa hoàn thành
                if (carSale.SaleStatus?.StatusName == "Payment Complete" ||
                    carSale.SaleStatus?.StatusName == "Delivered")
                {
                    return BadRequest(new { message = "This order has already been fully paid or completed." });
                }

                // Đối với đơn hàng deposit_first, kiểm tra xem có cần thanh toán đầy đủ hay chỉ phần còn lại
                decimal paymentAmount;
                string paymentPurpose;

                if (string.IsNullOrEmpty(carSale.OrderType))
                {
                    return BadRequest(new { message = "Order type is not specified." });
                }

                if (carSale.OrderType == "deposit_first")
                {
                    // Nếu là đơn deposit_first, kiểm tra xem đã thanh toán deposit chưa
                    if (carSale.SaleStatus?.StatusName == "Deposit Paid" ||
                        carSale.SaleStatus?.StatusName == "Pending Full Payment")
                    {
                        // Đã có deposit, chỉ thanh toán phần còn lại
                        paymentAmount = carSale.RemainingBalance ?? carSale.FinalPrice;
                        paymentPurpose = "remaining_payment";
                    }
                    else
                    {
                        // Chưa có deposit, thanh toán toàn bộ
                        paymentAmount = carSale.FinalPrice;
                        paymentPurpose = "full_payment";
                    }
                }
                else
                {
                    // Đơn hàng thanh toán một lần (full_payment_only)
                    paymentAmount = carSale.FinalPrice;
                    paymentPurpose = "full_payment";
                }

                if (paymentAmount <= 0)
                {
                    return BadRequest(new { message = "Invalid payment amount." });
                }

                // Create Payment record for full payment
                var fullPayment = new Payment
                {
                    UserId = userId,
                    ListingId = carSale.StoreListing?.ListingId ?? 0,
                    PaymentForSaleId = carSale.SaleId,
                    TransactionId = $"FULL-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                    Amount = paymentAmount,
                    PaymentMethod = dto?.PaymentMethod ?? "unknown",
                    PaymentStatus = (dto?.PaymentMethod == "e_wallet_momo_test" || dto?.PaymentMethod == "e_wallet_vnpay_test") ? "pending" : "completed",
                    PaymentPurpose = paymentPurpose,
                    DateOfPayment = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Payments.Add(fullPayment);
                await _context.SaveChangesAsync();

                // Update CarSale record
                carSale.FullPaymentId = fullPayment.PaymentId;
                carSale.ActualDeliveryDate = dto?.ActualDeliveryDate;
                carSale.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // For non-gateway payments, update status immediately
                if (dto?.PaymentMethod != "e_wallet_momo_test" && dto?.PaymentMethod != "e_wallet_vnpay_test")
                {
                    // Update CarSale status to "Payment Complete"
                    var paymentCompleteStatus = await _context.SaleStatus.FirstOrDefaultAsync(s => s.StatusName == "Payment Complete");
                    if (paymentCompleteStatus == null)
                    {
                        return StatusCode(500, new { message = "Sale status 'Payment Complete' not found in database. Please ensure it is seeded." });
                    }

                    carSale.SaleStatusId = paymentCompleteStatus.SaleStatusId;
                    carSale.RemainingBalance = 0;
                    carSale.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    // Send email notification
                    await SendFullPaymentEmail(carSale.SaleId);
                }

                return Ok(new
                {
                    message = "Full payment processed successfully.",
                    orderId = carSale.SaleId,
                    paymentId = fullPayment.PaymentId,
                    paymentAmount = paymentAmount,
                    paymentPurpose = paymentPurpose,
                    paymentGatewayRedirect = (dto?.PaymentMethod == "e_wallet_momo_test" || dto?.PaymentMethod == "e_wallet_vnpay_test")
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ProcessFullPayment: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        private async Task SendFullPaymentEmail(int saleId)
        {
            try
            {
                var fullCarSale = await _context.CarSales
                    .Include(cs => cs.Customer)
                    .Include(cs => cs.StoreListing).ThenInclude(sl => sl.CarListing).ThenInclude(cl => cl.Model).ThenInclude(cm => cm.CarManufacturer)
                    .Include(cs => cs.FullPayment)
                    .FirstOrDefaultAsync(cs => cs.SaleId == saleId);

                if (fullCarSale?.Customer?.Email == null) return;

                var customerEmail = fullCarSale.Customer.Email;
                var customerFullName = fullCarSale.Customer.FullName;
                var carName = $"{fullCarSale.StoreListing.CarListing.Model.CarManufacturer.Name} {fullCarSale.StoreListing.CarListing.Model.Name}";
                var fullPaymentAmountFormatted = fullCarSale.FullPayment?.Amount.ToString("N0") + " VND";

                var emailSubject = $"Full Payment Confirmation for Car {carName} - Order #{fullCarSale.OrderNumber}";
                var emailBody = $@"
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
                        <h1>Payment Successful!</h1>
                    </div>
                    <div class=""content"">
                        <p>Dear <strong>{customerFullName}</strong>,</p>
                        <p>We are pleased to confirm that your payment for order <strong>#{fullCarSale.OrderNumber}</strong> has been successfully processed.</p>

                        <div class=""highlight"">
                            <p><strong>Payment Details:</strong></p>
                            <ul>
                                <li><strong>Vehicle:</strong> {carName}</li>
                                <li><strong>Order Number:</strong> <strong>{fullCarSale.OrderNumber}</strong></li>
                                <li><strong>Payment Amount:</strong> <strong>{fullPaymentAmountFormatted}</strong></li>
                                <li><strong>Payment Method:</strong> {fullCarSale.FullPayment?.PaymentMethod ?? "N/A"}</li>
                                <li><strong>Payment Date:</strong> {fullCarSale.FullPayment?.DateOfPayment.ToString("dd/MM/yyyy HH:mm") ?? "N/A"}</li>
                                <li><strong>Total Vehicle Value:</strong> {fullCarSale.FinalPrice.ToString("N0")} VND</li>
                                <li><strong>Remaining Balance:</strong> 0 VND</li>
                                <li><strong>Estimated Delivery Date:</strong> {fullCarSale.ExpectedDeliveryDate?.ToString("dd/MM/yyyy") ?? "N/A"}</li>
                                <li><strong>Actual Delivery Date:</strong> {fullCarSale.ActualDeliveryDate?.ToString("dd/MM/yyyy") ?? "Pending Confirmation"}</li>
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

                await _emailService.SendEmailAsync(customerEmail, emailSubject, emailBody);
                Console.WriteLine($"Email sent successfully to: {customerEmail}");
            }
            catch (Exception emailEx)
            {
                Console.WriteLine($"Failed to send full payment email: {emailEx.Message}");
            }
        }

        // DTOs (unchanged)
        public class CreateDepositOrderDto
        {
            [Required]
            public int ListingId { get; set; }
            [Required]
            public decimal TotalPrice { get; set; }
            [Required]
            public decimal DepositAmount { get; set; }
            [Required]
            public string DeliveryOption { get; set; } = null!;
            public int? SelectedShowroomId { get; set; }
            public bool UseUserProfileAddress { get; set; }
            public ShippingAddressInfoDto? ShippingAddressInfo { get; set; }
            [Required]
            public string DepositPaymentMethod { get; set; } = null!;
            public DateTime? ExpectedDeliveryDate { get; set; }
        }

        public class ShippingAddressInfoDto
        {
            [Required]
            public string Name { get; set; } = null!;
            [Required]
            public string Address { get; set; } = null!;
            [Required]
            public string Phone { get; set; } = null!;
        }

        public class ProcessFullPaymentDto
        {
            [Required]
            public string PaymentMethod { get; set; } = null!;
            public DateTime? ActualDeliveryDate { get; set; }
        }

        [HttpGet("orders")]
        public async Task<IActionResult> GetMyOrders([FromQuery] int? statusId = null)
        {
            try
            {
                // IMPORTANT: Implement GetUserId() properly based on your authentication setup.
                // This usually involves reading claims from HttpContext.User.
                var userId = GetUserId(); // Assuming this method correctly gets the current authenticated UserId

                if (userId == 0)
                {
                    return Unauthorized(new { message = "User not authenticated or user ID is invalid." });
                }

                var query = _context.CarSales
                    .Where(s => s.CustomerId == userId);

                query = query
                    .Include(cs => cs.SaleStatus)
                    .Include(cs => cs.StoreListing)
                        .ThenInclude(sl => sl.CarListing)
                            .ThenInclude(cl => cl.Model)
                                .ThenInclude(cm => cm.CarManufacturer)
                    .Include(cs => cs.StoreListing)
                        .ThenInclude(sl => sl.CarListing)
                            .ThenInclude(cl => cl.CarImages)
                    .Include(cs => cs.StoreListing)
                        .ThenInclude(sl => sl.CarListing)
                            .ThenInclude(cl => cl.Specifications)
                    .Include(cs => cs.StoreListing)
                        .ThenInclude(sl => sl.StoreLocation)
                            .ThenInclude(loc => loc.Users)
                    .Include(cs => cs.ShippingAddress)
                    .Include(cs => cs.PickupStoreLocation)
                        .ThenInclude(loc => loc.Users)
                    .Include(cs => cs.DepositPayment)
                    .Include(cs => cs.FullPayment);

                if (statusId.HasValue)
                {
                    query = query.Where(s => s.SaleStatusId == statusId.Value);
                }

                var orders = await query
                    .OrderByDescending(cs => cs.CreatedAt)
                    .Select(cs => new
                    {
                        // Basic Order Details
                        OrderId = cs.SaleId, // Renamed to OrderId for clarity as per frontend
                        cs.OrderNumber,
                        cs.FinalPrice,
                        cs.DepositAmount,
                        cs.RemainingBalance,
                        OrderDate = cs.CreatedAt, // Use CreatedAt as OrderDate
                        cs.DeliveryOption,
                        cs.ExpectedDeliveryDate,
                        cs.ActualDeliveryDate,
                        cs.OrderType,

                        // Car Details - Nested Object
                        CarDetails = cs.StoreListing.CarListing != null ? new
                        {
                            ListingId = cs.StoreListing.CarListing.ListingId,
                            Make = cs.StoreListing.CarListing.Model.CarManufacturer.Name,
                            Model = cs.StoreListing.CarListing.Model.Name,
                            Year = cs.StoreListing.CarListing.Year,
                            Mileage = cs.StoreListing.CarListing.Mileage,
                            Condition = cs.StoreListing.CarListing.Condition,
                            Engine = cs.StoreListing.CarListing.Specifications != null && cs.StoreListing.CarListing.Specifications.Any() ? cs.StoreListing.CarListing.Specifications.FirstOrDefault().Engine : null,
                            Transmission = cs.StoreListing.CarListing.Specifications != null && cs.StoreListing.CarListing.Specifications.Any() ? cs.StoreListing.CarListing.Specifications.FirstOrDefault().Transmission : null,
                            FuelType = cs.StoreListing.CarListing.Specifications != null && cs.StoreListing.CarListing.Specifications.Any() ? cs.StoreListing.CarListing.Specifications.FirstOrDefault().FuelType : null,
                            ImageUrl = cs.StoreListing.CarListing.CarImages.FirstOrDefault().Url // Assuming CarImages is a collection
                        } : null,

                        // Sale Status Display Logic (matching frontend)
                        CurrentSaleStatus = (cs.SaleStatus != null) ?
                            (cs.SaleStatus.StatusName == "Available" || cs.SaleStatus.StatusName == "Pending Deposit" ? "Available" : // Pending Deposit -> Available
                             cs.SaleStatus.StatusName == "Sold" ? "Sold" :
                             cs.SaleStatus.StatusName == "On Hold" ? "On Hold" :
                             cs.SaleStatus.StatusName == "Deposit Paid" || cs.SaleStatus.StatusName == "Pending Full Payment" ? "Deposit Paid" : // Pending Full Payment -> Deposit Paid
                             cs.SaleStatus.StatusName == "Payment Complete" ? "Sold" : // Payment Complete -> Sold
                             cs.SaleStatus.StatusName) // Default to original status if not mapped
                             : "Available", // Default if no status

                        // Payment Details - Nested Objects

                        SellerDetails = cs.StoreListing.StoreLocation != null ? new
                        {

                            // Seller/Manager Information (assuming there's a manager/owner for each store)
                            SellerInfo = cs.StoreListing.StoreLocation.Users
                        .Where(u => u.StoreLocationId == cs.StoreListing.StoreLocation.StoreLocationId)
                        .Select(u => new
                        {
                            UserId = u.UserId,
                            FullName = u.FullName,
                            Email = u.Email,
                            PhoneNumber = u.Mobile,
                            Role = u.Role
                        }).FirstOrDefault()
                        } : null,
                        DepositPaymentDetails = cs.DepositPayment != null ? new
                        {
                            cs.DepositPayment.PaymentId,
                            cs.DepositPayment.Amount,
                            cs.DepositPayment.PaymentMethod,
                            PaymentStatus = cs.DepositPayment.PaymentStatus, // Corrected field name
                            DateOfPayment = cs.DepositPayment.DateOfPayment // Corrected field name
                        } : null,
                        FullPaymentDetails = cs.FullPayment != null ? new
                        {
                            cs.FullPayment.PaymentId,
                            cs.FullPayment.Amount,
                            cs.FullPayment.PaymentMethod,
                            PaymentStatus = cs.FullPayment.PaymentStatus, // Corrected field name
                            DateOfPayment = cs.FullPayment.DateOfPayment // Corrected field name
                        } : null,

                        // Delivery/Pickup Details - Nested Objects
                        ShippingAddressDetails = cs.ShippingAddress != null ? new
                        {
                            cs.ShippingAddress.AddressId,
                            cs.ShippingAddress.RecipientName,
                            cs.ShippingAddress.Address,
                        } : null,
                        PickupLocationDetails = cs.PickupStoreLocation != null ? new
                        {
                            cs.PickupStoreLocation.StoreLocationId,
                            cs.PickupStoreLocation.Name,
                            cs.PickupStoreLocation.Address,
                        } : null,
                    })
                    .ToListAsync();

                return Ok(orders);
            }
            catch (UnauthorizedAccessException ex)
            {
                // This catch block is for when GetUserId() throws UnauthorizedAccessException
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetMyOrders: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        // Existing API: GetOrderDetail - Updated to include more details
        [HttpGet("orders/{id}")]
        public async Task<IActionResult> GetOrderDetail(int id)
        {
            try
            {
                var userId = GetUserId();
                // Break down the Include/ThenInclude chain for better type inference
                var orderQuery = _context.CarSales.Where(s => s.SaleId == id && s.CustomerId == userId);

                orderQuery = orderQuery.Include(cs => cs.SaleStatus);
                orderQuery = orderQuery.Include(cs => cs.StoreListing)
                                       .ThenInclude(sl => sl.CarListing)
                                           .ThenInclude(cl => cl.Model)
                                               .ThenInclude(cm => cm.CarManufacturer);
                orderQuery = orderQuery.Include(cs => cs.ShippingAddress);
                orderQuery = orderQuery.Include(cs => cs.PickupStoreLocation);
                orderQuery = orderQuery.Include(cs => cs.DepositPayment);
                orderQuery = orderQuery.Include(cs => cs.FullPayment);

                var order = await orderQuery.FirstOrDefaultAsync();

                if (order == null) return NotFound("Order not found or unauthorized.");

                // Map to an anonymous object or a DTO to return relevant data
                return Ok(new
                {
                    order.SaleId,
                    order.OrderNumber,
                    CarDetails = new
                    {
                        ListingId = order.StoreListing.CarListing.ListingId,
                        ModelName = order.StoreListing.CarListing.Model.Name,
                        ManufacturerName = order.StoreListing.CarListing.Model.CarManufacturer.Name,
                        Price = order.StoreListing.CarListing.Price,
                        Year = order.StoreListing.CarListing.Year,
                        Condition = order.StoreListing.CarListing.Condition,
                        Vin = order.StoreListing.CarListing.Vin,
                        ImageUrls = _context.CarImages.Where(ci => ci.ListingId == order.StoreListing.CarListing.ListingId).Select(ci => ci.Url).ToList(),
                        VideoUrls = _context.CarVideos.Where(cv => cv.ListingId == order.StoreListing.CarListing.ListingId).Select(cv => cv.Url).ToList()
                    },
                    order.FinalPrice,
                    order.DepositAmount,
                    order.RemainingBalance,
                    Status = order.SaleStatus.StatusName,
                    order.DeliveryOption,
                    ShippingAddress = order.ShippingAddress != null ? new
                    {
                        order.ShippingAddress.AddressId,
                        order.ShippingAddress.Address,
                        order.ShippingAddress.RecipientName,
                        order.ShippingAddress.RecipientPhone,
                        order.ShippingAddress.AddressType
                    } : null,
                    PickupLocation = order.PickupStoreLocation != null ? new
                    {
                        order.PickupStoreLocation.StoreLocationId,
                        order.PickupStoreLocation.Name,
                        order.PickupStoreLocation.Address,
                    } : null,
                    order.ExpectedDeliveryDate,
                    order.ActualDeliveryDate,
                    DepositPayment = order.DepositPayment != null ? new
                    {
                        order.DepositPayment.PaymentId,
                        order.DepositPayment.Amount,
                        order.DepositPayment.PaymentMethod,
                        order.DepositPayment.PaymentStatus,
                        order.DepositPayment.DateOfPayment
                    } : null,
                    FullPayment = order.FullPayment != null ? new
                    {
                        order.FullPayment.PaymentId,
                        order.FullPayment.Amount,
                        order.FullPayment.PaymentMethod,
                        order.FullPayment.PaymentStatus,
                        order.FullPayment.DateOfPayment
                    } : null,
                    order.OrderType,
                    order.CreatedAt,
                    order.UpdatedAt
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetOrderDetail: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }


        // Existing API: CancelOrder - Updated to use SaleStatus name
        [HttpPut("orders/{id}/cancel")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            try
            {
                var userId = GetUserId();
                var sale = await _context.CarSales.FirstOrDefaultAsync(s => s.SaleId == id && s.CustomerId == userId);
                if (sale == null) return NotFound("Order not found or unauthorized.");

                var cancelledStatus = await _context.SaleStatus.FirstOrDefaultAsync(s => s.StatusName == "Cancelled");
                if (cancelledStatus == null)
                {
                    return StatusCode(500, new { message = "Sale status 'Cancelled' not found. Please ensure it is seeded." });
                }

                sale.SaleStatusId = cancelledStatus.SaleStatusId;
                sale.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return Ok(new { message = "Order cancelled successfully." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpPost("reviews")]
        public async Task<IActionResult> AddReview([FromBody] Review model)
        {
            try
            {
                var userId = GetUserId();
                model.UserId = userId;
                model.CreatedAt = DateTime.UtcNow;
                _context.Reviews.Add(model);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Review added successfully." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpGet("blogs")]
        public async Task<IActionResult> GetBlogs()
        {
            try
            {
                var blogs = await _context.BlogPosts.ToListAsync();
                return Ok(blogs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpGet("blogs/{id}")]
        public async Task<IActionResult> GetBlogDetail(int id)
        {
            try
            {
                var blog = await _context.BlogPosts.FindAsync(id);
                if (blog == null) return NotFound("Blog post not found.");
                return Ok(blog);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpGet("chats")]
        public IActionResult GetChats() => Ok(new { message = "Chat list (implement as needed)" });

        [HttpGet("chats/{id}")]
        public IActionResult GetChatDetail(int id) => Ok(new { message = "Chat detail (implement as needed)" });

        [HttpPost("chats/{id}/send")]
        public IActionResult SendChat(int id, [FromBody] string message) => Ok(new { message = "Message sent (implement as needed)" });

        [HttpGet("promotions")]
        public async Task<IActionResult> GetPromotions()
        {
            try
            {
                var promotions = await _context.Promotions.ToListAsync();
                return Ok(promotions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }
    }
}
