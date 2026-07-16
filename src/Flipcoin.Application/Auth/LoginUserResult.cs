namespace Flipcoin.Application.Auth;

/// <summary>Result of a successful login: the signed JWT to send as a bearer token.</summary>
public record LoginUserResult(string Token);
