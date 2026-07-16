using System.Text;
using Flipcoin.Api.Auth;
using Flipcoin.Domain.Enums;
using Flipcoin.Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Flipcoin.Api.Extensions;

/// <summary>
/// Wires JWT bearer authentication and the authorization policies. Binds
/// <see cref="JwtSettings"/> once so the same values sign tokens (in
/// Infrastructure) and validate them here.
/// </summary>
public static class AuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var section = configuration.GetSection(JwtSettings.SectionName);
        services.Configure<JwtSettings>(section);

        var settings = section.Get<JwtSettings>()
            ?? throw new InvalidOperationException("Missing 'Jwt' configuration section.");
        if (string.IsNullOrWhiteSpace(settings.Key))
        {
            throw new InvalidOperationException("Jwt:Key must be configured.");
        }

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = settings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = settings.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key)),
                    ValidateLifetime = true,
                    // No leeway on expiry; tokens are valid strictly until 'exp'.
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(
                AuthorizationPolicies.AdminOnly,
                policy => policy.RequireRole(nameof(UserRole.Admin)));
        });

        return services;
    }
}
