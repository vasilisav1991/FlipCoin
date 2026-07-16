using Flipcoin.Domain.Enums;
using Flipcoin.Domain.Users;

namespace Flipcoin.Domain.Entities;

/// <summary>
/// An account holder. A Player has exactly one <see cref="Wallet"/>; an Admin
/// has none (admins hold no funds). Identity is a <see cref="Guid"/> so ids are
/// non-sequential and cannot be enumerated by clients.
/// </summary>
public class User
{
    // Private setters + a private parameterless constructor let EF Core
    // materialise the object while preventing any other code from putting it
    // into an invalid state. State is only ever set through the public
    // constructor below, which enforces the invariants.
    public Guid Id { get; private set; }
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public UserRole Role { get; private set; }
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// The user's wallet. Null for admins; set for players. Populated by EF Core
    /// via the 1:1 relationship configured in the Infrastructure layer.
    /// </summary>
    public Wallet? Wallet { get; private set; }

    // Required by EF Core for materialisation. Not for application use.
    private User() { }

    public User(string email, string passwordHash, UserRole role)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(email));
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));

        Id = Guid.NewGuid();
        Email = EmailNormalization.Normalize(email);
        PasswordHash = passwordHash;
        Role = role;
        CreatedAt = DateTime.UtcNow;
    }
}
