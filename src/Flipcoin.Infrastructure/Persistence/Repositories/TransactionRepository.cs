using Flipcoin.Application.Abstractions.Persistence;
using Flipcoin.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Flipcoin.Infrastructure.Persistence.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly AppDbContext _context;

    public TransactionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Transaction>> GetByWalletPagedAsync(
        Guid walletId, int skip, int take, CancellationToken cancellationToken = default)
        => await _context.Transactions
            .Where(t => t.WalletId == walletId)
            .OrderByDescending(t => t.Timestamp)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

    public Task<int> CountByWalletAsync(Guid walletId, CancellationToken cancellationToken = default)
        => _context.Transactions.CountAsync(t => t.WalletId == walletId, cancellationToken);
}
