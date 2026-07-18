namespace Flipcoin.Application.Auth;

/// <summary>
/// Thrown by registration when the email is already taken. Mapped to HTTP 409
/// by the global exception handler.
/// </summary>
public class EmailAlreadyInUseException : Exception
{
    public EmailAlreadyInUseException(string email)
        : base($"Email '{email}' is already registered.")
    {
    }
}
