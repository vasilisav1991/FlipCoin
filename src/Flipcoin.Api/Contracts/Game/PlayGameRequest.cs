using Flipcoin.Domain.Enums;

namespace Flipcoin.Api.Contracts.Game;

/// <summary>
/// Request body for POST /api/game/play. The stake is required and must be
/// positive.
/// </summary>
public record PlayGameRequest(CoinSide Choice, decimal Stake);
