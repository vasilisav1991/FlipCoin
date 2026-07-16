namespace Flipcoin.Api.Auth;

/// <summary>Named authorization policies used by the API's endpoints.</summary>
public static class AuthorizationPolicies
{
    /// <summary>Requires the Admin role. Applied to the admin-only endpoints.</summary>
    public const string AdminOnly = "AdminOnly";
}
