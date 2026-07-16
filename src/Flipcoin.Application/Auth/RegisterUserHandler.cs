using Flipcoin.Application.Abstractions.Auth;
using Flipcoin.Application.Abstractions.Persistence;
using Flipcoin.Domain.Entities;
using Flipcoin.Domain.Enums;
using Flipcoin.Domain.Wallets;

namespace Flipcoin.Application.Auth;

/// <summary>
/// Registers a new player: creates the user and their wallet and persists both
/// in a single transaction. Registration always creates a Player (admins are
/// provisioned by seeding, not self-registration).
/// </summary>
public class RegisterUserHandler
{
    private const decimal StartingBalance = 100m;
    private const int MinPasswordLength = 6;

    private readonly IUserRepository _users;
    private readonly IWalletRepository _wallets;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterUserHandler(
        IUserRepository users,
        IWalletRepository wallets,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher)
    {
        _users = users;
        _wallets = wallets;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task<RegisterUserResult> HandleAsync(
        RegisterUserCommand command,
        CancellationToken cancellationToken = default)
    {
        // Minimal guard here; richer rules (email format, password policy) come
        // with FluentValidation in Phase 4.
        if (string.IsNullOrWhiteSpace(command.Password) || command.Password.Length < MinPasswordLength)
        {
            throw new ArgumentException(
                $"Password must be at least {MinPasswordLength} characters.", nameof(command));
        }

        if (await _users.ExistsByEmailAsync(command.Email, cancellationToken))
        {
            throw new EmailAlreadyInUseException(command.Email);
        }

        var passwordHash = _passwordHasher.Hash(command.Password);
        var user = new User(command.Email, passwordHash, UserRole.Player);
        var wallet = new Wallet(user.Id, WalletAddressGenerator.Generate(), StartingBalance);

        await _users.AddAsync(user, cancellationToken);
        await _wallets.AddAsync(wallet, cancellationToken);

        // A single SaveChanges wraps both inserts in one database transaction.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new RegisterUserResult(user.Id, wallet.Address);
    }
}
