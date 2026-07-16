using Flipcoin.Application.Abstractions.Persistence;

namespace Flipcoin.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of <see cref="IUnitOfWork"/>. It shares the same
/// scoped <see cref="AppDbContext"/> instance as the repositories, so a single
/// <see cref="SaveChangesAsync"/> commits everything they have tracked in one
/// database transaction.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
