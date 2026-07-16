using Flipcoin.Application.Abstractions.Persistence;
using Flipcoin.Domain.Entities;

namespace Flipcoin.Infrastructure.Persistence.Repositories;

public class GameRoundRepository : IGameRoundRepository
{
    private readonly AppDbContext _context;

    public GameRoundRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(GameRound round, CancellationToken cancellationToken = default)
        => await _context.GameRounds.AddAsync(round, cancellationToken);
}
