using GuideGo_Repository.DTOs;
using Microsoft.AspNetCore.Http;

namespace GuideGo_Service.Interfaces;

public interface IVnPayService
{
    /// <summary>Creates a Payment record and returns the VNPay redirect URL.</summary>
    Task<string> CreatePaymentUrlAsync(Guid bookingId, string ipAddress);

    /// <summary>Handles the redirect return from VNPay (user-facing callback).</summary>
    Task<VnPayReturnResponseDto> ProcessReturnAsync(IQueryCollection queryParams);

    /// <summary>Handles the IPN callback from VNPay (server-to-server, silent).</summary>
    Task<VnPayReturnResponseDto> ProcessIpnAsync(IQueryCollection queryParams);
}
