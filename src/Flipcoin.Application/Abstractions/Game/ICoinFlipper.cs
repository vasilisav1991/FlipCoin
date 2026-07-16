using Flipcoin.Domain.Enums;

namespace Flipcoin.Application.Abstractions.Game;

/// <summary>
/// Produces the server-side coin flip. Abstracted so the outcome can be made
/// deterministic in tests; the real implementation uses a cryptographic RNG.
/// </summary>
public interface ICoinFlipper
{
    CoinSide Flip();
}
