using System.Security.Cryptography;

namespace Flipcoin.Domain.Wallets;

/// <summary>
/// Generates wallet addresses of the form <c>FLIP-7f3a9c21</c>: the fixed prefix
/// plus 8 lowercase hex characters (4 random bytes). Uniqueness is ultimately
/// guaranteed by the unique index on Wallet.Address; on the astronomically rare
/// collision the caller retries. Randomness comes from
/// <see cref="RandomNumberGenerator"/> (cryptographic) rather than <c>Random</c>,
/// keeping all randomness in this project cryptographic and non-predictable.
/// </summary>
public static class WalletAddressGenerator
{
    private const string Prefix = "FLIP-";

    public static string Generate()
    {
        Span<byte> bytes = stackalloc byte[4];
        RandomNumberGenerator.Fill(bytes);
        return Prefix + Convert.ToHexStringLower(bytes);
    }
}
