namespace Flipcoin.Application.Auth;

/// <summary>
/// Thrown by login when the email is unknown or the password is wrong. The same
/// exception is used for both cases so the API cannot be used to discover which
/// emails are registered. Mapped to HTTP 401 by the exception middleware in Phase 4.
/// </summary>
public class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException()
        : base("Invalid email or password.")
    {
    }
}
