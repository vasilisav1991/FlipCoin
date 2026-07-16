namespace Flipcoin.Application.Auth;

/// <summary>Result of a successful registration.</summary>
public record RegisterUserResult(Guid UserId, string WalletAddress);
