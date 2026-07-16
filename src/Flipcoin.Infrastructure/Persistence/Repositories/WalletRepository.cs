using Flipcoin.Application.Abstractions.Persistence;
using Flipcoin.Domain.Entities;

namespace Flipcoin.Infrastructure.Persistence.Repositories;

public class WalletRepository : IWalletRepository
{
    private readonly AppDbContext _context;

    public WalletRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Wallet wallet, CancellationToken cancellationToken = default)
        => await _context.Wallets.AddAsync(wallet, cancellationToken);
}
