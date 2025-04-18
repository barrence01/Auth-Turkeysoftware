namespace Auth_Turkeysoftware.Utils
{
    /// <summary>
    /// Classe de utilitários para obter configurações do ambiente
    /// </summary>
    public static class ApiConfigUtils
    {
        /// <summary>
        /// Obtém variável de ambiente obrigatória
        /// </summary>
        /// <param name="name">Nome da variável</param>
        /// <returns>Valor da variável</returns>
        /// <exception cref="InvalidOperationException">
        /// Lançada quando a variável não está definida
        /// </exception>
        public static string GetRequiredEnvVar(string name) =>
            Environment.GetEnvironmentVariable(name) ??
            throw new InvalidOperationException($"Environment variable '{name}' is required.");

        /// <summary>
        /// Obtém variável de ambiente como inteiro
        /// </summary>
        /// <param name="name">Nome da variável</param>
        /// <returns>Valor convertido para inteiro</returns>
        /// <exception cref="InvalidOperationException">
        /// Lançada quando a variável não existe ou não é um inteiro válido
        /// </exception>
        public static int GettRequiredEnvVarAsInt(string name) =>
            int.TryParse(Environment.GetEnvironmentVariable(name), out var result) ? result
            : throw new InvalidOperationException($"Environment variable '{name}' must be an integer.");

        /// <summary>
        /// Obtém variável de ambiente obrigatória
        /// </summary>
        /// <param name="name">Nome da variável</param>
        /// <returns>Valor da variável</returns>
        public static string GetNonRequiredEnvVar(string name) =>
            Environment.GetEnvironmentVariable(name) ?? string.Empty;

        /// <summary>
        /// Obtém variável de ambiente como inteiro
        /// </summary>
        /// <param name="name">Nome da variável</param>
        /// <returns>Valor convertido para inteiro</returns>
        public static int GetNontRequiredEnvVarAsInt(string name) =>
            int.TryParse(Environment.GetEnvironmentVariable(name), out var result) ? result
            : default;

    }
}
