using Flipcoin.Domain.Enums;

namespace Flipcoin.Application.Game;

/// <summary>
/// Intent to play one round, wagering <paramref name="Stake"/> FLIP.
/// </summary>
public record PlayGameCommand(Guid UserId, CoinSide Choice, decimal Stake);
