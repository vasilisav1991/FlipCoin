namespace Flipcoin.Domain.Exceptions;

/// <summary>
/// Thrown by the domain when a debit would take a wallet below zero. Enforcing
/// this in the entity (not just in validation) is what guarantees balances can
/// never go negative, whatever calls into the wallet.
/// </summary>
public class InsufficientBalanceException : Exception
{
    public InsufficientBalanceException(decimal balance, decimal attemptedDebit)
        : base($"Insufficient balance: current {balance}, attempted debit {attemptedDebit}.")
    {
    }
}
