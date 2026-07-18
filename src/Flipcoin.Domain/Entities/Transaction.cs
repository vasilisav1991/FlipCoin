using Flipcoin.Domain.Enums;

namespace Flipcoin.Domain.Entities;

/// <summary>
/// An immutable ledger entry. Every change to a wallet balance produces exactly
/// one Transaction recording the type, the (always-positive) amount, an optional
/// counterparty wallet address, and the resulting balance (<see cref="BalanceAfter"/>).
/// Records are append-only — there are no mutators.
/// </summary>
public class Transaction
{
    public Guid Id { get; private set; }
    public Guid WalletId { get; private set; }
    public TransactionType Type { get; private set; }
    public decimal Amount { get; private set; }

    /// <summary>
    /// The other wallet's address for transfers (TransferIn/TransferOut); null for
    /// Stake/Payout which have no counterparty.
    /// </summary>
    public string? CounterpartyAddress { get; private set; }

    public DateTime Timestamp { get; private set; }

    /// <summary>The wallet balance immediately after this entry was applied.</summary>
    public decimal BalanceAfter { get; private set; }

    // Required by EF Core for materialisation. Not for application use.
    private Transaction() { }

    public Transaction(
        Guid walletId,
        TransactionType type,
        decimal amount,
        string? counterpartyAddress,
        decimal balanceAfter)
    {
        if (walletId == Guid.Empty)
            throw new ArgumentException("WalletId is required.", nameof(walletId));
        if (amount <= 0m)
            throw new ArgumentOutOfRangeException(nameof(amount),
                "A ledger amount is always recorded as a positive value.");
        if (balanceAfter < 0m)
            throw new ArgumentOutOfRangeException(nameof(balanceAfter),
                "Balance after a transaction cannot be negative.");

        Id = Guid.NewGuid();
        WalletId = walletId;
        Type = type;
        Amount = amount;
        CounterpartyAddress = counterpartyAddress;
        Timestamp = DateTime.UtcNow;
        BalanceAfter = balanceAfter;
    }
}
