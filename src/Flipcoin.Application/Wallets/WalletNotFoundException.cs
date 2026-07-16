namespace Flipcoin.Application.Wallets;

/// <summary>
/// Thrown when an operation needs the caller's wallet but they have none (e.g.
/// an admin trying to transfer or play). Mapped to HTTP 404.
/// </summary>
public class WalletNotFoundException : Exception
{
    public WalletNotFoundException()
        : base("The current user has no wallet.")
    {
    }
}
