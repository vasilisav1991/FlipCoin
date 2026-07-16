using Flipcoin.Application.Abstractions.Auth;
using Flipcoin.Application.Abstractions.Persistence;

namespace Flipcoin.Application.Auth;

/// <summary>
/// Authenticates a user by email + password and, on success, returns a signed
/// JWT. A wrong email and a wrong password fail identically so the endpoint
/// cannot be used to enumerate registered emails.
/// </summary>
public class LoginUserHandler
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public LoginUserHandler(
        IUserRepository users,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator tokenGenerator)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<LoginUserResult> HandleAsync(
        LoginUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByEmailAsync(command.Email, cancellationToken);
        if (user is null)
        {
            throw new InvalidCredentialsException();
        }

        if (!_passwordHasher.Verify(user.PasswordHash, command.Password))
        {
            throw new InvalidCredentialsException();
        }

        var token = _tokenGenerator.GenerateToken(user);
        return new LoginUserResult(token);
    }
}
