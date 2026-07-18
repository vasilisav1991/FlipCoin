namespace Flipcoin.Domain.Enums;

/// <summary>
/// The kind of balance change a ledger entry represents. Every change to a
/// wallet balance is recorded as exactly one Transaction of one of these types.
/// </summary>
public enum TransactionType
{
    /// <summary>Coins received from another wallet via a transfer.</summary>
    TransferIn = 1,

    /// <summary>Coins sent to another wallet via a transfer.</summary>
    TransferOut = 2,

    /// <summary>Coins wagered on a staked game round (debit).</summary>
    Stake = 3,

    /// <summary>Winnings paid out from a won staked game round (credit).</summary>
    Payout = 4
}
