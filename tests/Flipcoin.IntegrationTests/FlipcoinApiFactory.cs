using Flipcoin.Api.HostedServices;
using Flipcoin.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Flipcoin.IntegrationTests;

/// <summary>
/// Hosts the real API for integration tests, but swaps PostgreSQL for the EF
/// Core in-memory provider (no Docker, no native dependency). Required settings
/// are supplied as environment variables because Program reads them before the
/// host is built, which is earlier than WebApplicationFactory's own
/// configuration hooks run. Demo data is created in <see cref="InitializeAsync"/>
/// using the application's own DatabaseSeeder.
/// </summary>
public class FlipcoinApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly string _databaseName = $"flipcoin-tests-{Guid.NewGuid()}";

    public FlipcoinApiFactory()
    {
        // Visible to Program's pre-Build configuration reads. The connection
        // string is a dummy; the DbContext is replaced with in-memory below.
        Environment.SetEnvironmentVariable("ConnectionStrings__Postgres", "Host=unused");
        Environment.SetEnvironmentVariable("Jwt__Key", "integration-tests-signing-key-0123456789-abcdefghij");
        Environment.SetEnvironmentVariable("Jwt__Issuer", "Flipcoin");
        Environment.SetEnvironmentVariable("Jwt__Audience", "FlipcoinClient");
        Environment.SetEnvironmentVariable("Jwt__ExpiryMinutes", "60");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Replace the Npgsql-backed DbContext with the in-memory provider.
            // Remove every AppDbContext option/config descriptor first, including
            // the IDbContextOptionsConfiguration<AppDbContext> that EF Core 9+
            // registers, otherwise both providers stay applied to the options.
            var toRemove = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                d.ServiceType == typeof(DbContextOptions) ||
                d.ServiceType == typeof(AppDbContext) ||
                d.ServiceType.Name.Contains("DbContextOptionsConfiguration"))
                .ToList();
            foreach (var d in toRemove)
            {
                services.Remove(d);
            }

            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(_databaseName));

            // Remove the startup seeder; the factory seeds explicitly below so
            // every test class starts from a known, freshly seeded database.
            var seeder = services.Single(d => d.ImplementationType == typeof(DatabaseSeederHostedService));
            services.Remove(seeder);
        });
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
    }
}
