using Flipcoin.Domain.Entities;
using Flipcoin.Domain.Enums;
using Flipcoin.Domain.Wallets;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Flipcoin.Infrastructure.Persistence;

/// <summary>
/// Populates the database with a known set of demo accounts on first run:
/// one admin (no wallet) and two players (each with a wallet and a starting
/// balance). Idempotent — it does nothing if any users already exist, so it is
/// safe to run on every startup.
/// </summary>
public class DatabaseSeeder
{
    /// <summary>Plain-text password for every seeded account (demo only).</summary>
    public const string SeedPassword = "Password123!";

    private const decimal PlayerStartingBalance = 100m;

    private readonly AppDbContext _context;

    public DatabaseSeeder(AppDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await _context.Users.AnyAsync(cancellationToken))
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

        _context.Users.AddRange(admin, player1, player2);

        // Only players hold funds; the admin has no wallet.
        _context.Wallets.Add(new Wallet(player1.Id, WalletAddressGenerator.Generate(), PlayerStartingBalance));
        _context.Wallets.Add(new Wallet(player2.Id, WalletAddressGenerator.Generate(), PlayerStartingBalance));

        await _context.SaveChangesAsync(cancellationToken);
    }
}
