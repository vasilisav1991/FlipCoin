using Flipcoin.Domain.Enums;

namespace Flipcoin.Domain.Entities;

/// <summary>
/// A single, server-decided coin flip. Records what the player chose, the outcome
/// the server produced, the stake (0 for practice play), and the payout. Whether
/// the round was won is derived from choice == outcome, so it can never disagree
/// with the recorded sides.
/// </summary>
public class GameRound
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }

    /// <summary>Amount wagered. Zero for a practice round.</summary>
    public decimal Stake { get; private set; }

    public CoinSide Choice { get; private set; }
    public CoinSide Outcome { get; private set; }
    public bool Won { get; private set; }

    /// <summary>Coins credited to the player for this round (0 on a loss).</summary>
    public decimal Payout { get; private set; }

    public DateTime PlayedAt { get; private set; }

    // Required by EF Core for materialisation. Not for application use.
    private GameRound() { }

    public GameRound(Guid userId, decimal stake, CoinSide choice, CoinSide outcome, decimal payout)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required.", nameof(userId));
        if (stake < 0m)
            throw new ArgumentOutOfRangeException(nameof(stake), "Stake cannot be negative.");
        if (payout < 0m)
            throw new ArgumentOutOfRangeException(nameof(payout), "Payout cannot be negative.");

        Id = Guid.NewGuid();
        UserId = userId;
        Stake = stake;
        Choice = choice;
        Outcome = outcome;
        Won = choice == outcome;
        Payout = payout;
        PlayedAt = DateTime.UtcNow;
    }
}
