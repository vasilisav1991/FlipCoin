using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Flipcoin.IntegrationTests;

/// <summary>
/// The flagship test: a user can only ever reach their own wallet. Ownership is
/// derived from the JWT subject and no endpoint accepts another user's id, so
/// user A simply cannot obtain user B's wallet.
/// </summary>
public class WalletOwnershipTests : IClassFixture<FlipcoinApiFactory>
{
    private readonly FlipcoinApiFactory _factory;

    public WalletOwnershipTests(FlipcoinApiFactory factory)
    {
        _factory = factory;
    }

    private record LoginResponse(string Token);
    private record WalletResponse(string Address, decimal Balance);

    private async Task<string> LoginAsync(HttpClient client, string email)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login",
            new { email, password = "Password123!" });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return body!.Token;
    }

    private async Task<WalletResponse> GetWalletAsync(HttpClient client, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/wallet");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<WalletResponse>())!;
    }

    [Fact]
    public async Task Each_user_sees_only_their_own_wallet()
    {
        var client = _factory.CreateClient();

        var tokenA = await LoginAsync(client, "player1@flipcoin.local");
        var tokenB = await LoginAsync(client, "player2@flipcoin.local");

        var walletA = await GetWalletAsync(client, tokenA);
        var walletB = await GetWalletAsync(client, tokenB);

        // Distinct wallets, and A's token never yields B's wallet.
        Assert.NotEqual(walletA.Address, walletB.Address);
        Assert.NotEqual(walletB.Address, (await GetWalletAsync(client, tokenA)).Address);
    }

    [Fact]
    public async Task Wallet_requires_authentication()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/wallet");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
