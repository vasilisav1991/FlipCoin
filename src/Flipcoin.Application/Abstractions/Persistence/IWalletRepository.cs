using Flipcoin.Domain.Entities;

namespace Flipcoin.Application.Abstractions.Persistence;

/// <summary>
/// Persistence operations for <see cref="Wallet"/> aggregates. Adding a wallet
/// does not commit — call <see cref="IUnitOfWork.SaveChangesAsync"/> to persist.
/// Read/query methods are added alongside the use cases that need them.
/// </summary>
public interface IWalletRepository
{
    Task AddAsync(Wallet wallet, CancellationToken cancellationToken = default);
}
