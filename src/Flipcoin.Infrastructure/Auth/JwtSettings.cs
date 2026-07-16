namespace Flipcoin.Infrastructure.Auth;

/// <summary>
/// Strongly-typed JWT configuration, bound from the "Jwt" configuration section.
/// The same values are used to sign tokens (JwtTokenGenerator) and to validate
/// them (the API's JWT bearer setup).
/// </summary>
public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; } = 60;
}
