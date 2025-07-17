namespace Auth_Turkeysoftware.Domain.Models.Result
{
    public class TwoFactorValidationResult : IResult
    {
        public bool IsTwoFactorCodeEmpty { get; set; } = false;
        public bool IsMaxNumberOfTriesExceeded { get; set; } = false;
        public bool IsTwoFactorCodeExpired { get; set; } = false;
        public bool IsTwoFactorCodeInvalid { get; set; } = false;

        public TwoFactorValidationResult() { }

        public TwoFactorValidationResult(bool isEmpty, bool hasExceeded, bool isInvalid)
        {
            IsTwoFactorCodeEmpty = isEmpty;
            IsMaxNumberOfTriesExceeded = hasExceeded;
            IsTwoFactorCodeInvalid = isInvalid;
        }

        /// <summary>
        /// Retorna se a validação de dois fatores ocorreu com sucesso
        /// </summary>
        /// <returns>Um booleano indicando se a validação do código de autenticação de dois fatores ocorreu com sucesso </returns>
        public bool IsSuccess() => !(IsTwoFactorCodeEmpty || IsMaxNumberOfTriesExceeded || IsTwoFactorCodeExpired || IsTwoFactorCodeInvalid);
    }
}