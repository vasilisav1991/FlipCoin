namespace Flipcoin.Application.Abstractions.Persistence;

/// <summary>
/// Commits the changes tracked across the repositories as a single unit. A use
/// case mutates entities through the repositories and then calls
/// <see cref="SaveChangesAsync"/> exactly once, so a set of related changes
/// (e.g. a transfer's debit + credit + ledger entries) persists atomically.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
