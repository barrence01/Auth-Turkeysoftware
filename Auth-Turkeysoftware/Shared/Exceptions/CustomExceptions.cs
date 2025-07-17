namespace Auth_Turkeysoftware.Shared.Exceptions;

// Classe para exceções de negócio
[Serializable]
public class BusinessException : Exception
{

    public BusinessException() { }

    public BusinessException(string message) : base(message) { }

    public BusinessException(string message, Exception innerException) : base(message, innerException) { }

}

[Serializable]
public class InvalidSessionException : BusinessException
{

    public InvalidSessionException() { }

    public InvalidSessionException(string message) : base(message) { }

    public InvalidSessionException(string message, Exception innerException) : base(message, innerException) { }

}