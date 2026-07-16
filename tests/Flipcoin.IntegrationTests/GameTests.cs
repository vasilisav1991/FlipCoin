using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Flipcoin.IntegrationTests;

public class GameTests : IClassFixture<FlipcoinApiFactory>
{
    private readonly FlipcoinApiFactory _factory;

    public GameTests(FlipcoinApiFactory factory)
    {
        _factory = factory;
    }

    private record LoginResponse(string Token);
    private record PlayResponse(string Choice, string Outcome, bool Won, decimal Payout, decimal NewBalance);

    private async Task<HttpClient> AuthenticatedClientAsync(string email)
    {
        var client = _factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/login", new { email, password = "Password123!" });
        login.EnsureSuccessStatusCode();
        var token = (await login.Content.ReadFromJsonAsync<LoginResponse>())!.Token;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    [Fact]
    public async Task Practice_play_returns_a_result_and_never_reduces_balance()
    {
        var client = await AuthenticatedClientAsync("player1@flipcoin.local");

        var response = await client.PostAsJsonAsync("/api/game/play", new { choice = "Heads" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PlayResponse>();
        // Practice: won => +5, lost => +0. Balance starts at 100.
        Assert.Equal(result!.Won ? 105m : 100m, result.NewBalance);
        Assert.Equal(result.Won ? 5m : 0m, result.Payout);
    }

    [Fact]
    public async Task Stake_greater_than_balance_is_rejected()
    {
        var client = await AuthenticatedClientAsync("player2@flipcoin.local");

        var response = await client.PostAsJsonAsync("/api/game/play", new { choice = "Tails", stake = 100000m });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
