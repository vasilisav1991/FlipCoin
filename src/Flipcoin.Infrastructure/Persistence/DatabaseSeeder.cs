using Flipcoin.Application.Abstractions.Persistence;
using Flipcoin.Domain.Entities;
using Flipcoin.Domain.Enums;
using Flipcoin.Domain.Wallets;
using Microsoft.AspNetCore.Identity;

namespace Flipcoin.Infrastructure.Persistence;

/// <summary>
/// Populates the database with a known set of demo accounts on first run:
/// one admin (no wallet) and two players (each with a wallet and a starting
/// balance). Idempotent — it does nothing if any users already exist, so it is
/// safe to run on every startup. Persists through the repositories + unit of
/// work rather than the DbContext directly.
/// </summary>
public class DatabaseSeeder
{
    /// <summary>Plain-text password for every seeded account (demo only).</summary>
    public const string SeedPassword = "Password123!";

    private const decimal PlayerStartingBalance = 100m;

    private readonly IUserRepository _users;
    private readonly IWalletRepository _wallets;
    private readonly IUnitOfWork _unitOfWork;

    public DatabaseSeeder(IUserRepository users, IWalletRepository wallets, IUnitOfWork unitOfWork)
    {
        _users = users;
        _wallets = wallets;
        _unitOfWork = unitOfWork;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await _users.AnyAsync(cancellationToken))
        {
            return;
        }

        var hasher = new PasswordHasher<User>();

        // PasswordHasher<T>'s default implementation ignores the user argument,
        // so we can produce the hash before the User instance exists (the User
        // constructor requires a non-empty hash). Each call salts independently.
        string Hash() => hasher.HashPassword(user: null!, SeedPassword);

        var admin = new User("admin@flipcoin.local", Hash(), UserRole.Admin);
        var player1 = new User("player1@flipcoin.local", Hash(), UserRole.Player);
        var player2 = new User("player2@flipcoin.local", Hash(), UserRole.Player);

        await _users.AddAsync(admin, cancellationToken);
        await _users.AddAsync(player1, cancellationToken);
        await _users.AddAsync(player2, cancellationToken);

        // Only players hold funds; the admin has no wallet.
        await _wallets.AddAsync(new Wallet(player1.Id, WalletAddressGenerator.Generate(), PlayerStartingBalance), cancellationToken);
        await _wallets.AddAsync(new Wallet(player2.Id, WalletAddressGenerator.Generate(), PlayerStartingBalance), cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
