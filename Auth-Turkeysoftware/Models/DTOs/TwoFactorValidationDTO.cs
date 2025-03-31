namespace Auth_Turkeysoftware.Models.DTOs
{
    public class TwoFactorValidationDTO
    {
        public bool IsTwoFactorCodeEmpty { get; set; } = false;
        public bool IsMaxNumberOfTriesExceeded { get; set; } = false;
        public bool IsTwoFactorCodeExpired { get; set; } = false;
        public bool IsTwoFactorCodeInvalid { get; set; } = false;

        public TwoFactorValidationDTO() { }

        public TwoFactorValidationDTO(bool isEmpty, bool hasExceeded, bool isInvalid)
        {
            IsTwoFactorCodeEmpty = isEmpty;
            IsMaxNumberOfTriesExceeded = hasExceeded;
            IsTwoFactorCodeInvalid = isInvalid;
        }

        /// <summary>
        /// Retorna se a validação de dois fatores ocorreu com sucesso
        /// </summary>
        /// <returns>Um booleano indicando se a validação do código de autenticação de dois fatores ocorreu com sucesso </returns>
        public bool HasSucceeded() => !(IsTwoFactorCodeEmpty || IsMaxNumberOfTriesExceeded || IsTwoFactorCodeExpired || IsTwoFactorCodeInvalid);
    }
}