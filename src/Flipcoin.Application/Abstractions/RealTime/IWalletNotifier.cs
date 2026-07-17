namespace Flipcoin.Application.Abstractions.RealTime;

/// <summary>
/// Pushes a wallet's new balance to that user's connected clients in real time.
/// Best-effort: a delivery failure must never fail the use case that already
/// committed the money movement. Implemented with SignalR in the API layer.
/// </summary>
public interface IWalletNotifier
{
    Task WalletChangedAsync(Guid userId, decimal newBalance, CancellationToken cancellationToken = default);
}
