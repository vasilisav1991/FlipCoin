using Flipcoin.Application.Abstractions.Auth;
using Flipcoin.Application.Abstractions.Persistence;
using Flipcoin.Infrastructure.Auth;
using Flipcoin.Infrastructure.Persistence;
using Flipcoin.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Flipcoin.Infrastructure;

/// <summary>
/// Registers everything the Infrastructure layer provides (the DbContext, the
/// repositories, the unit of work, and the seeder) so the API only needs a
/// single call to wire persistence.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        services.AddScoped<DatabaseSeeder>();

        return services;
    }
}
