namespace Flipcoin.Application.Transfers;

/// <summary>Outcome of a completed transfer.</summary>
public record TransferResult(string ToAddress, decimal Amount, decimal NewBalance);
