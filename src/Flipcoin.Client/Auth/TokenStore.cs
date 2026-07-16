using Microsoft.JSInterop;

namespace Flipcoin.Client.Auth;

/// <summary>
/// Persists the JWT in the browser's localStorage (survives refreshes) using the
/// built-in localStorage API via JS interop — no extra package or custom script.
/// </summary>
public class TokenStore
{
    private const string Key = "flipcoin_token";

    private readonly IJSRuntime _js;

    public TokenStore(IJSRuntime js)
    {
        _js = js;
    }

    public ValueTask<string?> GetTokenAsync()
        => _js.InvokeAsync<string?>("localStorage.getItem", Key);

    public ValueTask SetTokenAsync(string token)
        => _js.InvokeVoidAsync("localStorage.setItem", Key, token);

    public ValueTask RemoveTokenAsync()
        => _js.InvokeVoidAsync("localStorage.removeItem", Key);
}
