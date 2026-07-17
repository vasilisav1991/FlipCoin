using Flipcoin.Application.Abstractions.RealTime;
using Microsoft.AspNetCore.SignalR;

namespace Flipcoin.Api.RealTime;

/// <summary>
/// SignalR implementation of <see cref="IWalletNotifier"/>. Targets the user's
/// connections via their id (SignalR maps this to the JWT subject claim).
/// Swallows delivery failures so a push problem never breaks a committed transfer
/// or game round.
/// </summary>
public class SignalRWalletNotifier : IWalletNotifier
{
    private readonly IHubContext<WalletHub> _hub;
    private readonly ILogger<SignalRWalletNotifier> _logger;

    public SignalRWalletNotifier(IHubContext<WalletHub> hub, ILogger<SignalRWalletNotifier> logger)
    {
        _hub = hub;
        _logger = logger;
    }

    public async Task WalletChangedAsync(Guid userId, decimal newBalance, CancellationToken cancellationToken = default)
    {
        try
        {
            await _hub.Clients.User(userId.ToString())
                .SendAsync("WalletChanged", newBalance, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to push wallet update to user {UserId}", userId);
        }
    }
}
