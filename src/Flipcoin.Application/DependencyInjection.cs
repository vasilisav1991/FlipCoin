using Flipcoin.Application.Auth;
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

        // LoginUserHandler is registered in Phase 2.2, once IJwtTokenGenerator
        // has an implementation — otherwise DI validation on build would fail.

        return services;
    }
}
