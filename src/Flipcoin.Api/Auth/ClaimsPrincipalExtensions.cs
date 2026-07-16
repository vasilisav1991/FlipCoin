using System.Security.Claims;

namespace Flipcoin.Api.Auth;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// The authenticated user's id, taken from the JWT subject. (The JWT handler
    /// maps the "sub" claim to <see cref="ClaimTypes.NameIdentifier"/>.) This is
    /// the only source of the current user's identity — never a request value.
    /// </summary>
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id)
            ? id
            : throw new InvalidOperationException("Authenticated user has no valid subject claim.");
    }
}
