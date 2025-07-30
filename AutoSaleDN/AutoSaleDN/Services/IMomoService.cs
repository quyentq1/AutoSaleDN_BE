using AutoSaleDN.DTO;

namespace AutoSaleDN.Services
{
    public interface IMomoService
    {
        Task<MomoPaymentResponseDto> CreatePaymentAsync(MomoPaymentRequestDto request);
        Task<MomoIpnResponseDto> ProcessMomoCallbackAsync(MomoIpnRequestDto ipnRequest);
    }
}
