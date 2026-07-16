using Flipcoin.Domain.Enums;

namespace Flipcoin.Application.Game;

/// <summary>The outcome of a played round and the resulting wallet balance.</summary>
public record PlayGameResult(
    CoinSide Choice,
    CoinSide Outcome,
    bool Won,
    decimal Payout,
    decimal NewBalance);
