using Auth_Turkeysoftware.Models.DTOs;

namespace Auth_Turkeysoftware.Services.ExternalServices
{
    public interface IExternalApiService
    {
       Task<IpDetailsDTO> GetIpDetails(string ipAddress);
    }
}
