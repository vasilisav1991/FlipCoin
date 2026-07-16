namespace Flipcoin.Application.Wallets;

/// <summary>A user's wallet as exposed to the client: just the address and balance.</summary>
public record WalletDto(string Address, decimal Balance);
