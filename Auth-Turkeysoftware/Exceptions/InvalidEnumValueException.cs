using System;

namespace Auth_Turkeysoftware.Exceptions;

public class InvalidEnumValueException : Exception {
    public InvalidEnumValueException() { }

    public InvalidEnumValueException(string message) : base(message) { }

    public InvalidEnumValueException(string message, Exception innerException) : base(message, innerException) { }

}
