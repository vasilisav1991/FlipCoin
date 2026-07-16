using Flipcoin.Application.Abstractions.Auth;
using Flipcoin.Application.Abstractions.Persistence;
using Flipcoin.Application.Auth;
using Flipcoin.Domain.Entities;
using Moq;

namespace Flipcoin.UnitTests.Auth;

public class RegisterUserHandlerTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IWalletRepository> _wallets = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();

    private RegisterUserHandler CreateHandler() =>
        new(_users.Object, _wallets.Object, _unitOfWork.Object, _passwordHasher.Object);

    [Fact]
    public async Task Registers_user_and_wallet_and_commits_once()
    {
        _users.Setup(u => u.ExistsByEmailAsync("new@flipcoin.local", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _passwordHasher.Setup(p => p.Hash("Password123!")).Returns("hashed");

        var handler = CreateHandler();

        var result = await handler.HandleAsync(new RegisterUserCommand("new@flipcoin.local", "Password123!"));

        Assert.NotEqual(Guid.Empty, result.UserId);
        Assert.StartsWith("FLIP-", result.WalletAddress);
        _users.Verify(u => u.AddAsync(It.Is<User>(x => x.PasswordHash == "hashed"), It.IsAny<CancellationToken>()), Times.Once);
        _wallets.Verify(w => w.AddAsync(It.IsAny<Wallet>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Rejects_duplicate_email()
    {
        _users.Setup(u => u.ExistsByEmailAsync("taken@flipcoin.local", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = CreateHandler();

        await Assert.ThrowsAsync<EmailAlreadyInUseException>(() =>
            handler.HandleAsync(new RegisterUserCommand("taken@flipcoin.local", "Password123!")));

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Rejects_password_shorter_than_minimum()
    {
        var handler = CreateHandler();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.HandleAsync(new RegisterUserCommand("new@flipcoin.local", "123")));

        _users.Verify(u => u.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
