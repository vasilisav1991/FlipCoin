namespace Flipcoin.Domain.Enums;

/// <summary>
/// The two roles in the system. A Player holds a wallet and can play/transfer;
/// an Admin holds no funds, cannot play, and only has the audit views.
/// </summary>
public enum UserRole
{
    Player = 0,
    Admin = 1
}
