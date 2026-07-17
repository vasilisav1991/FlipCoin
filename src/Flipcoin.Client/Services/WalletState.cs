using Flipcoin.Client.Auth;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;

namespace Flipcoin.Client.Services;

/// <summary>
/// Shared, reactive wallet state for the signed-in user. Holds the current
/// address/balance, keeps a SignalR connection to the API's wallet hub, and
/// updates the balance in real time when the server pushes a "WalletChanged"
/// event (e.g. an incoming transfer). Components subscribe to <see cref="OnChange"/>.
/// </summary>
public class WalletState : IAsyncDisposable
{
    private readonly ApiClient _api;
    private readonly TokenStore _tokenStore;
    private readonly string _apiBaseUrl;

    private HubConnection? _hub;
    private bool _started;

    public WalletState(ApiClient api, TokenStore tokenStore, IConfiguration configuration)
    {
        _api = api;
        _tokenStore = tokenStore;
        _apiBaseUrl = configuration["ApiBaseUrl"]!;
    }

    public string? Address { get; private set; }
    public decimal Balance { get; private set; }
    public bool HasWallet { get; private set; }

    public event Action? OnChange;

    /// <summary>Loads the wallet and opens the real-time connection. Idempotent.</summary>
    public async Task StartAsync()
    {
        if (_started)
        {
            return;
        }

        _started = true;

        try
        {
            var wallet = await _api.GetWalletAsync();
            Address = wallet.Address;
            Balance = wallet.Balance;
            HasWallet = true;
        }
        catch (ApiException)
        {
            // Admins have no wallet — nothing to track.
            HasWallet = false;
        }

        NotifyChanged();

        _hub = new HubConnectionBuilder()
            .WithUrl($"{_apiBaseUrl}/hubs/wallet", options =>
                options.AccessTokenProvider = async () => await _tokenStore.GetTokenAsync())
            .WithAutomaticReconnect()
            .Build();

        _hub.On<decimal>("WalletChanged", newBalance =>
        {
            SetBalance(newBalance);
            return Task.CompletedTask;
        });

        try
        {
            await _hub.StartAsync();
        }
        catch
        {
            // Real-time is an enhancement; the app still works over plain HTTP
            // if the hub can't connect.
        }
    }

    /// <summary>Closes the connection and clears state (on logout).</summary>
    public async Task StopAsync()
    {
        _started = false;

        if (_hub is not null)
        {
            await _hub.DisposeAsync();
            _hub = null;
        }

        Address = null;
        Balance = 0m;
        HasWallet = false;
        NotifyChanged();
    }

    /// <summary>Update the balance locally (e.g. from an action's response) for instant feedback.</summary>
    public void SetBalance(decimal balance)
    {
        Balance = balance;
        HasWallet = true;
        NotifyChanged();
    }

    private void NotifyChanged() => OnChange?.Invoke();

    public async ValueTask DisposeAsync()
    {
        if (_hub is not null)
        {
            await _hub.DisposeAsync();
        }
    }
}
