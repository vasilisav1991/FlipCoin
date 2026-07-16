namespace Flipcoin.Application.Abstractions.Auth;

/// <summary>
/// Hashes and verifies passwords. The Application layer depends only on this
/// abstraction; the concrete algorithm lives in Infrastructure.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string hash, string password);
}
