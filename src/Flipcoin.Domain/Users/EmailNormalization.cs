namespace Flipcoin.Domain.Users;

/// <summary>
/// Single source of truth for how an email is normalized before it is stored or
/// looked up. Keeping this in one place means the value written by
/// <see cref="Entities.User"/> and the value used in lookup queries can never
/// drift apart (which would silently break logins).
/// </summary>
public static class EmailNormalization
{
    public static string Normalize(string email) => email.Trim().ToLowerInvariant();
}
