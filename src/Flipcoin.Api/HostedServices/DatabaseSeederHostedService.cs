using Flipcoin.Infrastructure.Persistence;

namespace Flipcoin.Api.HostedServices;

/// <summary>
/// Runs the database seeder once when the application host starts. Implemented
/// as a hosted service — rather than inline between Build() and Run() — so it
/// executes only on a real application start. EF Core's design-time tooling
/// builds the host but never starts it, so migrations no longer trigger seeding.
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
        // DatabaseSeeder is scoped, so resolve it inside a new scope.
        using var scope = _services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
