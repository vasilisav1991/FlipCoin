using Flipcoin.Client.Auth;

namespace Flipcoin.Client.Services;

/// <summary>
/// Coordinates login/register/logout: calls the API, stores/clears the token,
/// notifies the authentication state provider, and starts/stops the real-time
/// wallet state so the UI updates live.
/// </summary>
public class AuthService
{
    private readonly ApiClient _api;
    private readonly TokenStore _tokenStore;
    private readonly JwtAuthenticationStateProvider _authStateProvider;
    private readonly WalletState _walletState;

    public AuthService(
        ApiClient api,
        TokenStore tokenStore,
        JwtAuthenticationStateProvider authStateProvider,
        WalletState walletState)
    {
        _api = api;
        _tokenStore = tokenStore;
        _authStateProvider = authStateProvider;
        _walletState = walletState;
    }

    public async Task LoginAsync(string email, string password)
    {
        var token = await _api.LoginAsync(email, password);
        await _tokenStore.SetTokenAsync(token);
        _authStateProvider.NotifyAuthenticationChanged();
        await _walletState.StartAsync();
    }

    public async Task RegisterAsync(string email, string password)
    {
        await _api.RegisterAsync(email, password);
        await LoginAsync(email, password);
    }

    public async Task LogoutAsync()
    {
        await _walletState.StopAsync();
        await _tokenStore.RemoveTokenAsync();
        _authStateProvider.NotifyAuthenticationChanged();
    }
}
