namespace Auth_Turkeysoftware.Exceptions;

// Classe para exceções de negócio
[Serializable]
public class BusinessRuleException : Exception {

    public BusinessRuleException() {}

    public BusinessRuleException(string message) : base(message) {}

    public BusinessRuleException(string message, Exception innerException) : base(message, innerException) {}

}

[Serializable]
public class InvalidSessionException : BusinessRuleException
{

    public InvalidSessionException() { }

    public InvalidSessionException(string message) : base(message) { }

    public InvalidSessionException(string message, Exception innerException) : base(message, innerException) { }

}