namespace Flipcoin.Application.Transfers;

/// <summary>Intent to transfer <paramref name="Amount"/> FLIP to a wallet address.</summary>
public record TransferCommand(Guid SenderUserId, string ToAddress, decimal Amount);
