using Flipcoin.Domain.Enums;

namespace Flipcoin.Application.Game;

/// <summary>
/// Intent to play one round. <paramref name="Stake"/> is null/omitted for a
/// practice round; a positive value stakes that many FLIP.
/// </summary>
public record PlayGameCommand(Guid UserId, CoinSide Choice, decimal? Stake);
