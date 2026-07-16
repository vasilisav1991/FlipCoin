using Flipcoin.Domain.Entities;

namespace Flipcoin.Application.Abstractions.Persistence;

/// <summary>
/// Read access to a wallet's ledger entries. Writes happen through the wallet
/// aggregate in later phases; for now this serves the paged history endpoint.
/// </summary>
public interface ITransactionRepository
{
    /// <summary>A page of a wallet's transactions, most recent first.</summary>
    Task<IReadOnlyList<Transaction>> GetByWalletPagedAsync(
        Guid walletId, int skip, int take, CancellationToken cancellationToken = default);

    /// <summary>Total number of transactions for the wallet (for paging metadata).</summary>
    Task<int> CountByWalletAsync(Guid walletId, CancellationToken cancellationToken = default);
}
