namespace Flipcoin.Application.Auth;

/// <summary>Input for registering a new player account.</summary>
public record RegisterUserCommand(string Email, string Password);
