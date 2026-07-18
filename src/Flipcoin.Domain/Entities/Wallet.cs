using Flipcoin.Domain.Enums;
using Flipcoin.Domain.Exceptions;

namespace Flipcoin.Domain.Entities;

/// <summary>
/// A player's holding of FLIP. One wallet per user (1:1). The balance is a
/// <see cref="decimal"/> — never a floating-point type — and can never go
/// negative: every debit and credit funnels through the single
/// <see cref="Apply"/> method, which also writes the matching ledger entry.
/// </summary>
public class Wallet
{
    // Backing field for the transactions navigation. Exposed read-only so callers
    // cannot bypass the (future) domain methods and mutate the ledger directly.
    private readonly List<Transaction> _transactions = new();

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Address { get; private set; } = null!;
    public decimal Balance { get; private set; }

    public IReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();

    // Required by EF Core for materialisation. Not for application use.
    private Wallet() { }

    public Wallet(Guid userId, string address, decimal startingBalance)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required.", nameof(userId));
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Address is required.", nameof(address));
        if (startingBalance < 0m)
            throw new ArgumentOutOfRangeException(nameof(startingBalance),
                "Starting balance cannot be negative.");

        Id = Guid.NewGuid();
        UserId = userId;
        Address = address;
        Balance = startingBalance;
    }

    /// <summary>Debits this wallet for coins sent to another wallet.</summary>
    public Transaction SendTransfer(decimal amount, string toAddress)
        => Apply(amount, sign: -1, TransactionType.TransferOut, toAddress);

    /// <summary>Credits this wallet for coins received from another wallet.</summary>
    public Transaction ReceiveTransfer(decimal amount, string fromAddress)
        => Apply(amount, sign: +1, TransactionType.TransferIn, fromAddress);

    /// <summary>Debits this wallet for a wager placed on a game round.</summary>
    public Transaction PlaceStake(decimal amount)
        => Apply(amount, sign: -1, TransactionType.Stake, counterpartyAddress: null);

    /// <summary>Credits this wallet with winnings from a won staked round.</summary>
    public Transaction ReceivePayout(decimal amount)
        => Apply(amount, sign: +1, TransactionType.Payout, counterpartyAddress: null);

    /// <summary>Credits this wallet with a practice-play reward.</summary>
    public Transaction ReceiveReward(decimal amount)
        => Apply(amount, sign: +1, TransactionType.Reward, counterpartyAddress: null);

    /// <summary>
    /// Applies a signed balance change and records the matching ledger entry.
    /// The amount is always positive; <paramref name="sign"/> decides credit (+1)
    /// or debit (-1). A debit that would make the balance negative is rejected,
    /// so the wallet can never hold less than zero.
    /// </summary>
    private Transaction Apply(decimal amount, int sign, TransactionType type, string? counterpartyAddress)
    {
        if (amount <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive.");
        }

        var newBalance = Balance + (sign * amount);
        if (newBalance < 0m)
        {
            throw new InsufficientBalanceException(Balance, amount);
        }

        Balance = newBalance;

        var transaction = new Transaction(Id, type, amount, counterpartyAddress, Balance);
        _transactions.Add(transaction);
        return transaction;
    }
}
