using Flipcoin.Domain.Entities;

namespace Flipcoin.Application.Abstractions.Auth;

/// <summary>
/// Issues a signed JWT for an authenticated user (including the role claim).
/// Implemented in Infrastructure/Auth in Phase 2.2; the login use case depends
/// only on this abstraction.
/// </summary>
public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}
