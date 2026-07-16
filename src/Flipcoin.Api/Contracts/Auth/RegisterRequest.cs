namespace Flipcoin.Api.Contracts.Auth;

/// <summary>Request body for POST /api/auth/register.</summary>
public record RegisterRequest(string Email, string Password);
