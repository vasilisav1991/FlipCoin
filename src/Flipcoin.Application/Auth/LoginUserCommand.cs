namespace Flipcoin.Application.Auth;

/// <summary>Input for authenticating an existing user.</summary>
public record LoginUserCommand(string Email, string Password);
