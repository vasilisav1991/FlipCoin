namespace Flipcoin.Application.Admin;

/// <summary>A user and their wallet (address/balance are null for admins).</summary>
public record AdminWalletDto(string Email, string Role, string? Address, decimal? Balance);

/// <summary>A ledger entry in the global audit log, with its owning wallet address.</summary>
public record AdminTransactionDto(
    string WalletAddress,
    string Type,
    decimal Amount,
    string? CounterpartyAddress,
    DateTime Timestamp,
    decimal BalanceAfter);

/// <summary>A played round in the global game history.</summary>
public record AdminGameRoundDto(
    Guid UserId,
    decimal Stake,
    string Choice,
    string Outcome,
    bool Won,
    decimal Payout,
    DateTime PlayedAt);
