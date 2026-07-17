using Flipcoin.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Flipcoin.Api.HostedServices;

/// <summary>
/// Brings the database to a ready state when the application host starts:
/// applies any pending EF Core migrations, then runs the (idempotent) demo
/// seeder. Implemented as a hosted service — rather than inline between
/// Build() and Run() — so it executes only on a real application start. EF
/// Core's design-time tooling builds the host but never starts it, so
/// migrations no longer trigger seeding. The integration tests replace the
/// database with an in-memory provider and remove this service entirely.
/// </summary>
public class DatabaseSeederHostedService : IHostedService
{
    private readonly IServiceProvider _services;

    public DatabaseSeederHostedService(IServiceProvider services)
    {
        _services = services;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // AppDbContext and DatabaseSeeder are scoped, so resolve them inside a new scope.
        using var scope = _services.CreateScope();

        // Creates the schema on a fresh database (e.g. the docker-compose
        // Postgres) and is a no-op when everything is already applied.
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);

        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
