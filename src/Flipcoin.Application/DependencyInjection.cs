using Flipcoin.Application.Admin;
using Flipcoin.Application.Auth;
using Flipcoin.Application.Game;
using Flipcoin.Application.Transfers;
using Flipcoin.Application.Wallets;
using Microsoft.Extensions.DependencyInjection;

namespace Flipcoin.Application;

/// <summary>
/// Registers the Application layer's use case handlers. Its only dependency is
/// the (interface-only) DI abstractions package, so Application stays free of
/// any framework or infrastructure concerns.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<RegisterUserHandler>();
        services.AddScoped<LoginUserHandler>();
        services.AddScoped<GetMyWalletHandler>();
        services.AddScoped<GetMyTransactionsHandler>();
        services.AddScoped<TransferHandler>();
        services.AddScoped<PlayGameHandler>();
        services.AddScoped<AdminQueryService>();

        return services;
    }
}
