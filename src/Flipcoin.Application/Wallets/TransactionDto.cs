namespace Flipcoin.Application.Wallets;

/// <summary>A single ledger entry as exposed to the client.</summary>
public record TransactionDto(
    string Type,
    decimal Amount,
    string? CounterpartyAddress,
    DateTime Timestamp,
    decimal BalanceAfter);
