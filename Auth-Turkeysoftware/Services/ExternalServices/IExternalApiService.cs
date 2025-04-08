using Auth_Turkeysoftware.Models.DTOs;

namespace Auth_Turkeysoftware.Services.ExternalServices
{
    public interface IExternalApiService
    {
       Task<IpDetailsDto?> GetIpDetails(string ipAddress);
    }
}
