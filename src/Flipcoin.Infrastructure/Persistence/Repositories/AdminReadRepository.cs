using Flipcoin.Application.Abstractions.Persistence;
using Flipcoin.Application.Admin;
using Flipcoin.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Flipcoin.Infrastructure.Persistence.Repositories;

public class AdminReadRepository : IAdminReadRepository
{
    private readonly AppDbContext _context;

    public AdminReadRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AdminWalletDto>> GetWalletsAsync(CancellationToken cancellationToken = default)
    {
        // Include the wallet (absent for admins) and map in memory so the enum's
        // ToString is not pushed into SQL.
        var users = await _context.Users
            .Include(u => u.Wallet)
            .OrderBy(u => u.Email)
            .ToListAsync(cancellationToken);

        return users
            .Select(u => new AdminWalletDto(
                u.Email,
                u.Role.ToString(),
                u.Wallet?.Address,
                u.Wallet?.Balance))
            .ToList();
    }

    public async Task<(IReadOnlyList<AdminTransactionDto> Items, int TotalCount)> GetTransactionsAsync(
        TransactionType? type, int skip, int take, CancellationToken cancellationToken = default)
    {
        var query = _context.Transactions.AsQueryable();
        if (type.HasValue)
        {
            query = query.Where(t => t.Type == type.Value);
        }

        var total = await query.CountAsync(cancellationToken);

        var rows = await query
            .OrderByDescending(t => t.Timestamp)
            .Skip(skip)
            .Take(take)
            .Join(_context.Wallets, t => t.WalletId, w => w.Id, (t, w) => new { Transaction = t, w.Address })
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(x => new AdminTransactionDto(
                x.Address,
                x.Transaction.Type.ToString(),
                x.Transaction.Amount,
                x.Transaction.CounterpartyAddress,
                x.Transaction.Timestamp,
                x.Transaction.BalanceAfter))
            .ToList();

        return (items, total);
    }

    public async Task<(IReadOnlyList<AdminGameRoundDto> Items, int TotalCount)> GetGameRoundsAsync(
        int skip, int take, CancellationToken cancellationToken = default)
    {
        var total = await _context.GameRounds.CountAsync(cancellationToken);

        var rounds = await _context.GameRounds
            .OrderByDescending(g => g.PlayedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        var items = rounds
            .Select(g => new AdminGameRoundDto(
                g.UserId, g.Stake, g.Choice.ToString(), g.Outcome.ToString(), g.Won, g.Payout, g.PlayedAt))
            .ToList();

        return (items, total);
    }
}
