using Flipcoin.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Flipcoin.Infrastructure.Persistence;

/// <summary>
/// The EF Core unit-of-work for Flipcoin. Exposes the aggregate roots as
/// <see cref="DbSet{TEntity}"/>s and applies every <c>IEntityTypeConfiguration</c>
/// found in this assembly, so mapping details live next to each entity rather
/// than piling up in one method.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<GameRound> GameRounds => Set<GameRound>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Discovers and applies UserConfiguration, WalletConfiguration, etc.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Postgres's built-in xmin row version guards wallets against lost
        // updates: a save whose read row has since changed throws
        // DbUpdateConcurrencyException (surfaced as 409). Provider-specific, so
        // skipped for the InMemory provider the integration tests run on —
        // those tests do not exercise this behaviour.
        if (Database.IsNpgsql())
        {
            modelBuilder.Entity<Wallet>().Property<uint>("xmin").IsRowVersion();
        }
    }
}
