using Flipcoin.Application.Abstractions.Persistence;
using Flipcoin.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Flipcoin.Infrastructure.Persistence.Repositories;

public class WalletRepository : IWalletRepository
{
    private readonly AppDbContext _context;

    public WalletRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Wallet?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

    public async Task AddAsync(Wallet wallet, CancellationToken cancellationToken = default)
        => await _context.Wallets.AddAsync(wallet, cancellationToken);
}
