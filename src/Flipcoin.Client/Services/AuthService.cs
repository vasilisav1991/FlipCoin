using Flipcoin.Client.Auth;

namespace Flipcoin.Client.Services;

/// <summary>
/// Coordinates login/register/logout: calls the API, stores/clears the token,
/// and notifies the authentication state provider so the UI updates.
/// </summary>
public class AuthService
{
    private readonly ApiClient _api;
    private readonly TokenStore _tokenStore;
    private readonly JwtAuthenticationStateProvider _authStateProvider;

    public AuthService(ApiClient api, TokenStore tokenStore, JwtAuthenticationStateProvider authStateProvider)
    {
        _api = api;
        _tokenStore = tokenStore;
        _authStateProvider = authStateProvider;
    }

    public async Task LoginAsync(string email, string password)
    {
        var token = await _api.LoginAsync(email, password);
        await _tokenStore.SetTokenAsync(token);
        _authStateProvider.NotifyAuthenticationChanged();
    }

    public async Task RegisterAsync(string email, string password)
    {
        await _api.RegisterAsync(email, password);
        await LoginAsync(email, password);
    }

    public async Task LogoutAsync()
    {
        await _tokenStore.RemoveTokenAsync();
        _authStateProvider.NotifyAuthenticationChanged();
    }
}
