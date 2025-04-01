using Auth_Turkeysoftware.Models.DTOs;
using Auth_Turkeysoftware.Repositories.DataBaseModels;

namespace Auth_Turkeysoftware.Services
{
    public interface IAuthenticationService
    {
        /// <summary>
        /// Gera um código de autenticação de dois fatores aleatório, armazena-o no cache e o envia para o email especificado.
        /// </summary>
        /// <param name="email">O endereço de email para enviar o código de dois fatores.</param>
        /// <returns>Uma tarefa que representa a operação assíncrona.</returns>
        Task SendTwoFactorCodeAsync(string email);

        /// <summary>
        /// Verifica o código de autenticação de dois fatores fornecido em relação ao código armazenado no cache.
        /// </summary>
        /// <param name="user">O usuário tentando autenticar.</param>
        /// <param name="twoFactorCode">O código de autenticação de dois fatores fornecido pelo usuário.</param>
        /// <returns>Um <see cref="TwoFactorValidationDTO"/> contendo o resultado da verificação.</returns>
        Task<TwoFactorValidationDTO> VerifyTwoFactor(ApplicationUser user, string? twoFactorCode);
    }
}
