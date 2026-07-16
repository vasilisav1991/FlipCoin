using Flipcoin.Application.Abstractions.Persistence;

namespace Flipcoin.Application.Wallets;

/// <summary>
/// Returns the caller's own wallet. The user id comes from the authenticated
/// principal (the JWT subject), never from client input, so a caller can only
/// ever read their own wallet. Returns null when the user has no wallet (admins).
/// </summary>
public class GetMyWalletHandler
{
    private readonly IWalletRepository _wallets;

    public GetMyWalletHandler(IWalletRepository wallets)
    {
        _wallets = wallets;
    }

    public async Task<WalletDto?> HandleAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var wallet = await _wallets.GetByUserIdAsync(userId, cancellationToken);
        return wallet is null ? null : new WalletDto(wallet.Address, wallet.Balance);
    }
}
