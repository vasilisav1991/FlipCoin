using Flipcoin.Application.Abstractions.Auth;
using Flipcoin.Application.Abstractions.Persistence;
using Flipcoin.Application.Auth;
using Flipcoin.Domain.Entities;
using Flipcoin.Domain.Enums;
using Moq;

namespace Flipcoin.UnitTests.Auth;

public class LoginUserHandlerTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IJwtTokenGenerator> _tokenGenerator = new();

    private LoginUserHandler CreateHandler() =>
        new(_users.Object, _passwordHasher.Object, _tokenGenerator.Object);

    [Fact]
    public async Task Returns_token_for_valid_credentials()
    {
        var user = new User("player@flipcoin.local", "stored-hash", UserRole.Player);
        _users.Setup(u => u.GetByEmailAsync("player@flipcoin.local", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasher.Setup(p => p.Verify("stored-hash", "Password123!")).Returns(true);
        _tokenGenerator.Setup(t => t.GenerateToken(user)).Returns("jwt-token");

        var handler = CreateHandler();

        var result = await handler.HandleAsync(new LoginUserCommand("player@flipcoin.local", "Password123!"));

        Assert.Equal("jwt-token", result.Token);
    }

    [Fact]
    public async Task Rejects_unknown_email()
    {
        _users.Setup(u => u.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var handler = CreateHandler();

        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            handler.HandleAsync(new LoginUserCommand("unknown@flipcoin.local", "Password123!")));
    }

    [Fact]
    public async Task Rejects_wrong_password()
    {
        var user = new User("player@flipcoin.local", "stored-hash", UserRole.Player);
        _users.Setup(u => u.GetByEmailAsync("player@flipcoin.local", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasher.Setup(p => p.Verify("stored-hash", "wrong")).Returns(false);

        var handler = CreateHandler();

        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            handler.HandleAsync(new LoginUserCommand("player@flipcoin.local", "wrong")));

        _tokenGenerator.Verify(t => t.GenerateToken(It.IsAny<User>()), Times.Never);
    }
}
