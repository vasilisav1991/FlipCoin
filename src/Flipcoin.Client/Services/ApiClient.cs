using System.Net.Http.Headers;
using System.Net.Http.Json;
using Flipcoin.Client.Auth;
using Flipcoin.Client.Models;

namespace Flipcoin.Client.Services;

/// <summary>
/// Typed wrapper over the Flipcoin API. Attaches the bearer token to every
/// request and turns error responses (ProblemDetails) into an ApiException with
/// a readable message.
/// </summary>
public class ApiClient
{
    private readonly HttpClient _http;
    private readonly TokenStore _tokenStore;

    public ApiClient(HttpClient http, TokenStore tokenStore)
    {
        _http = http;
        _tokenStore = tokenStore;
    }

    // --- Auth ---
    public async Task<string> LoginAsync(string email, string password)
    {
        var result = await SendAsync<TokenResponse>(HttpMethod.Post, "api/auth/login",
            new { email, password });
        return result.Token;
    }

    public Task<RegisterResult> RegisterAsync(string email, string password)
        => SendAsync<RegisterResult>(HttpMethod.Post, "api/auth/register", new { email, password });

    // --- Wallet ---
    public Task<WalletDto> GetWalletAsync()
        => SendAsync<WalletDto>(HttpMethod.Get, "api/wallet");

    public Task<PagedResult<TransactionDto>> GetTransactionsAsync(int page = 1, int pageSize = 20)
        => SendAsync<PagedResult<TransactionDto>>(HttpMethod.Get, $"api/wallet/transactions?page={page}&pageSize={pageSize}");

    // --- Game ---
    public Task<PlayResult> PlayAsync(string choice, decimal? stake)
        => SendAsync<PlayResult>(HttpMethod.Post, "api/game/play", new { choice, stake });

    // --- Transfers ---
    public Task<TransferResult> TransferAsync(string toAddress, decimal amount)
        => SendAsync<TransferResult>(HttpMethod.Post, "api/transfers", new { toAddress, amount });

    // --- Admin ---
    public Task<List<AdminWallet>> GetAdminWalletsAsync()
        => SendAsync<List<AdminWallet>>(HttpMethod.Get, "api/admin/wallets");

    public Task<PagedResult<AdminTransaction>> GetAdminTransactionsAsync(int page, int pageSize, string? type)
    {
        var uri = $"api/admin/transactions?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(type))
        {
            uri += $"&type={type}";
        }

        return SendAsync<PagedResult<AdminTransaction>>(HttpMethod.Get, uri);
    }

    public Task<PagedResult<AdminGameRound>> GetAdminGameRoundsAsync(int page, int pageSize)
        => SendAsync<PagedResult<AdminGameRound>>(HttpMethod.Get, $"api/admin/game-rounds?page={page}&pageSize={pageSize}");

    private async Task<T> SendAsync<T>(HttpMethod method, string uri, object? body = null)
    {
        using var request = new HttpRequestMessage(method, uri);

        var token = await _tokenStore.GetTokenAsync();
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        using var response = await _http.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new ApiException(await ReadErrorAsync(response));
        }

        return (await response.Content.ReadFromJsonAsync<T>())!;
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
    {
        try
        {
            var problem = await response.Content.ReadFromJsonAsync<ApiProblemDetails>();
            if (problem?.Errors is { Count: > 0 })
            {
                return string.Join(" ", problem.Errors.SelectMany(e => e.Value));
            }

            return problem?.Detail ?? problem?.Title ?? $"Request failed ({(int)response.StatusCode}).";
        }
        catch
        {
            return $"Request failed ({(int)response.StatusCode}).";
        }
    }

    private record TokenResponse(string Token);
}
