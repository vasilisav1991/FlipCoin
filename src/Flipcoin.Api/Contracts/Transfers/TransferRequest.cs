namespace Flipcoin.Api.Contracts.Transfers;

/// <summary>Request body for POST /api/transfers.</summary>
public record TransferRequest(string ToAddress, decimal Amount);
