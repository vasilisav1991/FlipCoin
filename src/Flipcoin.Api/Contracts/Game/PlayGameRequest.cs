using Flipcoin.Domain.Enums;

namespace Flipcoin.Api.Contracts.Game;

/// <summary>
/// Request body for POST /api/game/play. Omit <see cref="Stake"/> (or send null)
/// for a practice round.
/// </summary>
public record PlayGameRequest(CoinSide Choice, decimal? Stake);
