using System.Security.Cryptography;
using Flipcoin.Application.Abstractions.Game;
using Flipcoin.Domain.Enums;

namespace Flipcoin.Infrastructure.Game;

/// <summary>
/// Coin flip backed by a cryptographic RNG. <see cref="RandomNumberGenerator"/>
/// (not <see cref="Random"/>) is used so outcomes are unbiased and unpredictable.
/// </summary>
public class CoinFlipper : ICoinFlipper
{
    public CoinSide Flip()
        => RandomNumberGenerator.GetInt32(2) == 0 ? CoinSide.Heads : CoinSide.Tails;
}
