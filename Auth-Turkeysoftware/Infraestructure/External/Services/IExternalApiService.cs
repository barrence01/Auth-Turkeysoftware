using Auth_Turkeysoftware.Domain.Models.VOs;

namespace Auth_Turkeysoftware.Infraestructure.External.Services
{
    public interface IExternalApiService
    {
        Task<IpDetailsVO?> GetIpDetails(string ipAddress);
    }
}
