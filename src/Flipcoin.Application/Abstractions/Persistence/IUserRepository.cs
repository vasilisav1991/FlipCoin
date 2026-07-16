using Flipcoin.Domain.Entities;

namespace Flipcoin.Application.Abstractions.Persistence;

/// <summary>
/// Persistence operations for <see cref="User"/> aggregates. Adding a user does
/// not commit — call <see cref="IUnitOfWork.SaveChangesAsync"/> to persist.
/// Read/query methods are added alongside the use cases that need them.
/// </summary>
public interface IUserRepository
{
    /// <summary>True if any user exists (used to keep seeding idempotent).</summary>
    Task<bool> AnyAsync(CancellationToken cancellationToken = default);

    /// <summary>True if a user with the given email already exists.</summary>
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>The user with the given email, or null if none exists.</summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task AddAsync(User user, CancellationToken cancellationToken = default);
}
