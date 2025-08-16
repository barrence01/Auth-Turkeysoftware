namespace Auth_Turkeysoftware.Shared.Utils
{
    /// <summary>
    /// Classe utilitária para geração de chaves de cache relacionadas à autenticação.
    /// </summary>
    public static class AuthUtil
    {
        /// <summary>
        /// Gera uma chave para ser utilizada nas operações com cache para validação de dois fatores.
        /// </summary>
        /// <param name="email">Mail ou Username do usuário.</param>
        /// <returns>Uma string contendo a chave a ser utilizada.</returns>
        public static string Get2FACacheKey(string email)
        {
            return $"2FA:{email}";
        }

        /// <summary>
        /// Gera uma chave para ser utilizada nas operações com cache para habilitação de autenticação de dois fatores.
        /// </summary>
        /// <param name="email">Mail ou Username do usuário.</param>
        /// <returns>Uma string contendo a chave a ser utilizada.</returns>
        public static string Get2FAEnableCacheKey(string email)
        {
            return $"2FA-Email-Enable:{email}";
        }

        /// <summary>
        /// Gera uma chave para ser utilizada nas operações com cache para redefinição de senha.
        /// </summary>
        /// <param name="email">Mail ou Username do usuário.</param>
        /// <returns>Uma string contendo a chave a ser utilizada.</returns>
        public static string GetPassResetKey(string email)
        {
            return $"PassReset:{email}";
        }
    }
}
