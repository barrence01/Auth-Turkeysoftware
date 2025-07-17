namespace Auth_Turkeysoftware.Domain.Models.Result
{
    /// <summary>
    /// Interface que representa o resultado de uma operação.
    /// </summary>
    public interface IResult
    {
        /// <summary>
        /// Indica se a operação retornou com sucesso.
        /// </summary>
        /// <returns>True se a operação foi bem-sucedida, caso contrário, False.</returns>
        public bool IsSuccess();
    }
}
