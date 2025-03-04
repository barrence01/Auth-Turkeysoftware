using Auth_Turkeysoftware.Models;

namespace Auth_Turkeysoftware.Services.ExternalServices
{
    public interface IExternalApiService
    {
       Task<IpDetailsModel> GetIpDetails(string ipAddress);
    }
}
