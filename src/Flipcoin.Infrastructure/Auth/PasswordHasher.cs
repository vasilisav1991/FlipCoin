using Flipcoin.Application.Abstractions.Auth;
using Flipcoin.Domain.Entities;
using IdentityHasher = Microsoft.AspNetCore.Identity.PasswordHasher<Flipcoin.Domain.Entities.User>;
using Microsoft.AspNetCore.Identity;

namespace Flipcoin.Infrastructure.Auth;

/// <summary>
/// Password hashing backed by ASP.NET Core's <see cref="IdentityHasher"/>
/// (PBKDF2 with a per-hash salt and iteration count). The underlying hasher is
/// generic over the user type but its default implementation ignores the user
/// argument, so a single shared instance with a null user is safe.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private readonly IdentityHasher _hasher = new();

    public string Hash(string password)
        => _hasher.HashPassword(user: null!, password);

    public bool Verify(string hash, string password)
        => _hasher.VerifyHashedPassword(user: null!, hash, password) != PasswordVerificationResult.Failed;
}
