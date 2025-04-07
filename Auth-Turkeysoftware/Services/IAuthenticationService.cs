using Auth_Turkeysoftware.Models.Results;
using Auth_Turkeysoftware.Repositories.DataBaseModels;

namespace Auth_Turkeysoftware.Services
{
    public interface IAuthenticationService
    {
        /// <summary>
        /// Gera um código de autenticação de dois fatores aleatório, armazena-o no cache e o envia para o email especificado.
        /// </summary>
        /// <param name="user">O objeto do usuário</param>
        /// <param name="TwoFactorMode">Tipo de autorização de 2 fatores</param>
        /// <returns>Uma tarefa que representa a operação assíncrona.</returns>
        Task SendTwoFactorCodeAsync(ApplicationUser user, int twoFactorMode);

        /// <summary>
        /// Verifica o código de autenticação de dois fatores fornecido em relação ao código armazenado no cache.
        /// </summary>
        /// <param name="user">O usuário tentando autenticar.</param>
        /// <param name="twoFactorCode">O código de autenticação de dois fatores fornecido pelo usuário.</param>
        /// <returns>Um <see cref="TwoFactorValidationResult"/> contendo o resultado da verificação.</returns>
        Task<TwoFactorValidationResult> VerifyTwoFactorAuthentication(ApplicationUser user, string? twoFactorCode);

        Task<List<TwoFactorAuthResponse>> ListActive2FAOptions(ApplicationUser user);
    }
}
