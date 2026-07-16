using Flipcoin.Application.Abstractions.Persistence;
using Flipcoin.Domain.Entities;
using Flipcoin.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Flipcoin.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<bool> AnyAsync(CancellationToken cancellationToken = default)
        => _context.Users.AnyAsync(cancellationToken);

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = EmailNormalization.Normalize(email);
        return _context.Users.AnyAsync(u => u.Email == normalized, cancellationToken);
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = EmailNormalization.Normalize(email);
        return _context.Users.FirstOrDefaultAsync(u => u.Email == normalized, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
        => await _context.Users.AddAsync(user, cancellationToken);
}
