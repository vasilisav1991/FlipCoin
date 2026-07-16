using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Flipcoin.IntegrationTests;

public class AdminTests : IClassFixture<FlipcoinApiFactory>
{
    private readonly FlipcoinApiFactory _factory;

    public AdminTests(FlipcoinApiFactory factory)
    {
        _factory = factory;
    }

    private record LoginResponse(string Token);

    private async Task<string> LoginAsync(HttpClient client, string email)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password = "Password123!" });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<LoginResponse>())!.Token;
    }

    [Fact]
    public async Task Admin_can_list_all_wallets()
    {
        var client = _factory.CreateClient();
        var token = await LoginAsync(client, "admin@flipcoin.local");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/admin/wallets");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var wallets = await response.Content.ReadFromJsonAsync<List<AdminWalletRow>>();
        // Admin + two seeded players.
        Assert.Equal(3, wallets!.Count);
        Assert.Contains(wallets, w => w.Email == "player1@flipcoin.local" && w.Balance == 100m);
    }

    [Fact]
    public async Task Player_is_forbidden_from_admin_endpoints()
    {
        var client = _factory.CreateClient();
        var token = await LoginAsync(client, "player1@flipcoin.local");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/admin/wallets");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private record AdminWalletRow(string Email, string Role, string? Address, decimal? Balance);
}
