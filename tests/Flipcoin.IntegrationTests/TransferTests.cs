using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Flipcoin.IntegrationTests;

public class TransferTests : IClassFixture<FlipcoinApiFactory>
{
    private readonly FlipcoinApiFactory _factory;

    public TransferTests(FlipcoinApiFactory factory)
    {
        _factory = factory;
    }

    private record LoginResponse(string Token);
    private record WalletResponse(string Address, decimal Balance);
    private record TransferResponse(string ToAddress, decimal Amount, decimal NewBalance);

    private async Task<string> LoginAsync(HttpClient client, string email)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login",
            new { email, password = "Password123!" });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<LoginResponse>())!.Token;
    }

    private static void Authorize(HttpClient client, string token)
        => client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    [Fact]
    public async Task Transfer_moves_funds_between_wallets()
    {
        var client = _factory.CreateClient();

        var senderToken = await LoginAsync(client, "player1@flipcoin.local");
        var recipientToken = await LoginAsync(client, "player2@flipcoin.local");

        Authorize(client, recipientToken);
        var recipientWallet = await client.GetFromJsonAsync<WalletResponse>("/api/wallet");

        Authorize(client, senderToken);
        var response = await client.PostAsJsonAsync("/api/transfers",
            new { toAddress = recipientWallet!.Address, amount = 40m });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<TransferResponse>();
        Assert.Equal(60m, result!.NewBalance);

        // Recipient balance grew from 100 to 140.
        Authorize(client, recipientToken);
        var recipientAfter = await client.GetFromJsonAsync<WalletResponse>("/api/wallet");
        Assert.Equal(140m, recipientAfter!.Balance);
    }

    [Fact]
    public async Task Transfer_exceeding_balance_is_rejected()
    {
        var client = _factory.CreateClient();

        var senderToken = await LoginAsync(client, "player1@flipcoin.local");
        var recipientToken = await LoginAsync(client, "player2@flipcoin.local");

        Authorize(client, recipientToken);
        var recipientWallet = await client.GetFromJsonAsync<WalletResponse>("/api/wallet");

        Authorize(client, senderToken);
        var response = await client.PostAsJsonAsync("/api/transfers",
            new { toAddress = recipientWallet!.Address, amount = 100000m });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
