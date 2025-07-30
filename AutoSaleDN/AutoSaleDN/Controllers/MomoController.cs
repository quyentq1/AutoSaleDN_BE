using AutoSaleDN.DTO;

using AutoSaleDN.Services;
using Microsoft.AspNetCore.Mvc;

namespace AutoSaleDN.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MomoController : ControllerBase
    {
        private readonly IMomoService _momoService;

        public MomoController(IMomoService momoService)
        {
            _momoService = momoService;
        }

        [HttpPost("create_payment_url")]
        public async Task<IActionResult> CreatePaymentUrl([FromBody] MomoPaymentRequestDto request)
        {
            try
            {
                var response = await _momoService.CreatePaymentAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating Momo payment URL: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpPost("momo_ipn")] // Momo sends POST requests to this endpoint
        public async Task<IActionResult> MomoIpn([FromBody] MomoIpnRequestDto ipnRequest)
        {
            try
            {
                var response = await _momoService.ProcessMomoCallbackAsync(ipnRequest);
                // Momo expects a specific JSON response to acknowledge the IPN
                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing Momo IPN: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                // Return a failure response to Momo if processing fails internally
                return Ok(new MomoIpnResponseDto
                {
                    PartnerCode = ipnRequest.PartnerCode,
                    OrderId = ipnRequest.OrderId,
                    RequestId = ipnRequest.RequestId,
                    Message = "Internal server error during IPN processing",
                    ResultCode = "99"
                });
            }
        }
    }
}
