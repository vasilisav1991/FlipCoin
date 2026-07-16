namespace Flipcoin.Domain.Entities;

/// <summary>
/// A player's holding of FLIP. One wallet per user (1:1). The balance is a
/// <see cref="decimal"/> — never a floating-point type — and can never be
/// constructed negative. Debit/credit behaviour that mutates the balance and
/// writes ledger entries is added in Phase 3, alongside the use cases that need it.
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
}
