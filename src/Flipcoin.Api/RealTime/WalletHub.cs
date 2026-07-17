using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Flipcoin.Api.RealTime;

/// <summary>
/// SignalR hub for pushing wallet updates to the owning user. Clients only
/// receive ("WalletChanged"); there are no client-invokable methods. Requires
/// authentication — the user's connection is keyed by the JWT subject, so the
/// server can target <c>Clients.User(userId)</c>.
/// </summary>
[Authorize]
public class WalletHub : Hub
{
}
