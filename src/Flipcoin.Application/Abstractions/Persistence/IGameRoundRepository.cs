using Flipcoin.Domain.Entities;

namespace Flipcoin.Application.Abstractions.Persistence;

public interface IGameRoundRepository
{
    Task AddAsync(GameRound round, CancellationToken cancellationToken = default);
}
