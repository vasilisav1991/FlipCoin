using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;

namespace Flipcoin.Client.Auth;

/// <summary>
/// Builds the app's authentication state from the stored JWT: it decodes the
/// token's payload into claims (including the role, which drives AuthorizeView).
/// The token is not re-validated here — the API is the authority — but an expired
/// token is treated as signed out.
/// </summary>
public class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
    private const string RoleClaim = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

    private static readonly AuthenticationState Anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    private readonly TokenStore _tokenStore;

    public JwtAuthenticationStateProvider(TokenStore tokenStore)
    {
        _tokenStore = tokenStore;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _tokenStore.GetTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
        {
            return Anonymous;
        }

        var claims = ParseClaims(token).ToList();
        if (IsExpired(claims))
        {
            await _tokenStore.RemoveTokenAsync();
            return Anonymous;
        }

        // Tell ClaimsPrincipal which claim carries the name and the role.
        var identity = new ClaimsIdentity(claims, authenticationType: "jwt",
            nameType: "email", roleType: RoleClaim);
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    /// <summary>Re-evaluate the state after login/logout.</summary>
    public void NotifyAuthenticationChanged()
        => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

    private static IEnumerable<Claim> ParseClaims(string token)
    {
        var payload = token.Split('.')[1];
        var json = Base64UrlDecode(payload);
        var map = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json)
                  ?? new Dictionary<string, JsonElement>();

        return map.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString()));
    }

    private static bool IsExpired(IEnumerable<Claim> claims)
    {
        var exp = claims.FirstOrDefault(c => c.Type == "exp")?.Value;
        if (long.TryParse(exp, out var seconds))
        {
            return DateTimeOffset.FromUnixTimeSeconds(seconds) <= DateTimeOffset.UtcNow;
        }

        return false;
    }

    private static string Base64UrlDecode(string value)
    {
        var padded = value.Replace('-', '+').Replace('_', '/');
        padded = padded.PadRight(padded.Length + (4 - padded.Length % 4) % 4, '=');
        return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(padded));
    }
}
