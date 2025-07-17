namespace Auth_Turkeysoftware.Domain.Models.Result
{
    /// <summary>  
    /// Interface que representa um resultado contendo dados.  
    /// </summary>  
    /// <typeparam name="T">O tipo de dados retornados.</typeparam>  
    public interface IResultData<T> : IResult
    {
        /// <summary>  
        /// Obtém os dados associados ao resultado.  
        /// </summary>  
        /// <returns>Os dados do tipo <typeparamref name="T"/>.</returns>  
        public T GetData();
    }
}
